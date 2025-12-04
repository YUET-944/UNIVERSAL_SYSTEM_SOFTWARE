using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using UniversalBusinessSystem.Core.Entities;

namespace UniversalBusinessSystem.Data;

public class DatabaseService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _databasePath;

    public DatabaseService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _databasePath = GetDatabasePath();
    }

    public string GetDatabasePath()
    {
        var appDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "UniversalBusinessSystem"
        );

        if (!Directory.Exists(appDataRoot))
        {
            Directory.CreateDirectory(appDataRoot);
        }

        var databaseDirectory = Path.Combine(appDataRoot, "database");
        if (!Directory.Exists(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        return Path.Combine(databaseDirectory, "UniversalBusinessSystem.db");
    }

    public async Task<bool> InitializeDatabaseAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            var migrations = context.Database.GetMigrations();
            if (migrations.Any())
            {
                await context.Database.MigrateAsync().ConfigureAwait(false);
            }
            else
            {
                await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
            }

            // Ensure initial data is seeded
            await SeedInitialDataAsync(context).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            return false;
        }
    }

    private async Task SeedInitialDataAsync(UniversalBusinessSystemDbContext context)
    {
        // Check if admin user exists
        var adminUser = await context.Users
            .FirstOrDefaultAsync(u => u.Username == "ALGOHUB").ConfigureAwait(false);

        if (adminUser == null)
        {
            // Create default organization if not exists
            var defaultOrg = await context.Organizations
                .FirstOrDefaultAsync(o => o.Name == "Default Organization").ConfigureAwait(false);

            if (defaultOrg == null)
            {
                defaultOrg = new Organization
                {
                    Name = "Default Organization",
                    Description = "Default organization for initial setup",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Organizations.Add(defaultOrg);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Create admin role
            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Administrator" && r.OrganizationId == defaultOrg.Id)
                .ConfigureAwait(false);

            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Name = "Administrator",
                    Description = "System administrator with full access",
                    OrganizationId = defaultOrg.Id,
                    IsActive = true,
                    IsSystemRole = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Roles.Add(adminRole);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Assign all permissions to admin role
            var allPermissions = await context.Permissions.ToListAsync().ConfigureAwait(false);
            foreach (var permission in allPermissions)
            {
                var existingRolePermission = await context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == permission.Id)
                    .ConfigureAwait(false);

                if (existingRolePermission == null)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync().ConfigureAwait(false);

            // Create admin user
            adminUser = new User
            {
                Username = "ALGOHUB",
                Email = "admin@algohub.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("ALGOHUB"),
                FirstName = "Algo",
                LastName = "Hub",
                OrganizationId = defaultOrg.Id,
                RoleId = adminRole.Id,
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<bool> CreateOrganizationAsync(Organization organization, User owner, ShopType shopType)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            // Create organization
            organization.CreatedAt = DateTime.UtcNow;
            organization.IsActive = true;
            context.Organizations.Add(organization);
            await context.SaveChangesAsync().ConfigureAwait(false);

            // Create owner role
            var ownerRole = new Role
            {
                Name = "Owner",
                Description = "Business owner with full access",
                OrganizationId = organization.Id,
                IsActive = true,
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Roles.Add(ownerRole);
            await context.SaveChangesAsync().ConfigureAwait(false);

            // Assign all permissions to owner role
            var allPermissions = await context.Permissions.ToListAsync().ConfigureAwait(false);
            foreach (var permission in allPermissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = ownerRole.Id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync().ConfigureAwait(false);

            // Create staff role
            var staffRole = new Role
            {
                Name = "Staff",
                Description = "Staff member with limited access",
                OrganizationId = organization.Id,
                IsActive = true,
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Roles.Add(staffRole);
            await context.SaveChangesAsync().ConfigureAwait(false);

            // Assign basic permissions to staff role
            var basicPermissions = await context.Permissions
                .Where(p => p.Category == PermissionCategory.Products ||
                           p.Category == PermissionCategory.Categories ||
                           p.Category == PermissionCategory.Inventory)
                .ToListAsync()
                .ConfigureAwait(false);

            foreach (var permission in basicPermissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = staffRole.Id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync().ConfigureAwait(false);

            // Update user with organization and role
            owner.OrganizationId = organization.Id;
            owner.RoleId = ownerRole.Id;
            owner.IsEmailVerified = true;
            owner.CreatedAt = DateTime.UtcNow;
            context.Users.Update(owner);
            await context.SaveChangesAsync().ConfigureAwait(false);

            // Activate default modules for this shop type
            await ActivateDefaultModulesAsync(context, organization.Id, shopType).ConfigureAwait(false);

            // Activate default units for this shop type
            await ActivateDefaultUnitsAsync(context, organization.Id, shopType).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create organization: {ex.Message}");
            return false;
        }
    }

    private async Task ActivateDefaultModulesAsync(UniversalBusinessSystemDbContext context, Guid organizationId, ShopType shopType)
    {
        if (!string.IsNullOrEmpty(shopType.DefaultModules))
        {
            var moduleIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(shopType.DefaultModules);
            if (moduleIds != null)
            {
                foreach (var moduleId in moduleIds)
                {
                    if (Guid.TryParse(moduleId, out var guid))
                    {
                        var existingOrgModule = await context.OrganizationModules
                            .FirstOrDefaultAsync(om => om.OrganizationId == organizationId && om.ModuleId == guid)
                            .ConfigureAwait(false);

                        if (existingOrgModule == null)
                        {
                            context.OrganizationModules.Add(new OrganizationModule
                            {
                                OrganizationId = organizationId,
                                ModuleId = guid,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task ActivateDefaultUnitsAsync(UniversalBusinessSystemDbContext context, Guid organizationId, ShopType shopType)
    {
        if (!string.IsNullOrEmpty(shopType.DefaultUnits))
        {
            var unitIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(shopType.DefaultUnits);
            if (unitIds != null)
            {
                foreach (var unitId in unitIds)
                {
                    if (Guid.TryParse(unitId, out var guid))
                    {
                        var existingOrgUnit = await context.OrganizationUnits
                            .FirstOrDefaultAsync(ou => ou.OrganizationId == organizationId && ou.UnitId == guid)
                            .ConfigureAwait(false);

                        if (existingOrgUnit == null)
                        {
                            context.OrganizationUnits.Add(new OrganizationUnit
                            {
                                OrganizationId = organizationId,
                                UnitId = guid,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}
