using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniversalBusinessSystem.Core.Entities;
using UniversalBusinessSystem.Core.Services;
using UniversalBusinessSystem.Data;

namespace UniversalBusinessSystem.Services;

public class ModuleService : IModuleService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IModuleRegistry _moduleRegistry;
    private readonly Dictionary<string, IModule> _loadedModules;

    public ModuleService(IServiceProvider serviceProvider, IModuleRegistry moduleRegistry)
    {
        _serviceProvider = serviceProvider;
        _moduleRegistry = moduleRegistry;
        _loadedModules = new Dictionary<string, IModule>();
    }

    public async Task<List<Module>> GetAvailableModulesAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            return await context.Modules
                .Where(m => m.IsActive)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }
        catch
        {
            return new List<Module>();
        }
    }

    public async Task<List<Module>> GetActiveModulesAsync(Guid organizationId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            return await context.OrganizationModules
                .Where(om => om.OrganizationId == organizationId && om.IsActive)
                .Select(om => om.Module)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }
        catch
        {
            return new List<Module>();
        }
    }

    public async Task<bool> ActivateModuleAsync(Guid organizationId, Guid moduleId, string? configuration = null)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            var module = await context.Modules.FindAsync(moduleId);
            if (module == null || !module.IsActive)
            {
                return false;
            }

            var existingOrgModule = await context.OrganizationModules
                .FirstOrDefaultAsync(om => om.OrganizationId == organizationId && om.ModuleId == moduleId);

            if (existingOrgModule != null)
            {
                if (!existingOrgModule.IsActive)
                {
                    existingOrgModule.IsActive = true;
                    existingOrgModule.Configuration = configuration;
                    existingOrgModule.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                context.OrganizationModules.Add(new OrganizationModule
                {
                    OrganizationId = organizationId,
                    ModuleId = moduleId,
                    IsActive = true,
                    Configuration = configuration,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();

            await InitializeModuleAsync(module.Key, organizationId, configuration);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeactivateModuleAsync(Guid organizationId, Guid moduleId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            var orgModule = await context.OrganizationModules
                .FirstOrDefaultAsync(om => om.OrganizationId == organizationId && om.ModuleId == moduleId);

            if (orgModule != null)
            {
                orgModule.IsActive = false;
                orgModule.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                await ShutdownModuleAsync(orgModule.Module.Key, organizationId);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsModuleActiveAsync(Guid organizationId, string moduleKey)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            return await context.OrganizationModules
                .AnyAsync(om => om.OrganizationId == organizationId &&
                               om.Module.Key == moduleKey &&
                               om.IsActive);
        }
        catch
        {
            return false;
        }
    }

    public async Task<Module?> GetModuleByKeyAsync(string moduleKey)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            return await context.Modules
                .FirstOrDefaultAsync(m => m.Key == moduleKey && m.IsActive);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Module>> GetModulesByTypeAsync(ModuleType moduleType)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            return await context.Modules
                .Where(m => m.ModuleType == moduleType && m.IsActive)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }
        catch
        {
            return new List<Module>();
        }
    }

    public async Task<bool> UpdateModuleConfigurationAsync(Guid organizationId, Guid moduleId, string configuration)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            var orgModule = await context.OrganizationModules
                .FirstOrDefaultAsync(om => om.OrganizationId == organizationId && om.ModuleId == moduleId);

            if (orgModule != null)
            {
                orgModule.Configuration = configuration;
                orgModule.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                await ConfigureModuleAsync(orgModule.Module.Key, organizationId, configuration);

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task InitializeModuleAsync(string moduleKey, Guid organizationId, string? configuration)
    {
        try
        {
            if (!_loadedModules.TryGetValue(moduleKey, out var module))
            {
                module = LoadModule(moduleKey);
                if (module != null)
                {
                    _loadedModules[moduleKey] = module;
                }
            }

            if (module != null)
            {
                await module.InitializeAsync(organizationId);

                if (!string.IsNullOrEmpty(configuration))
                {
                    await module.ConfigureAsync(configuration);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize module {moduleKey}: {ex.Message}");
        }
    }

    private async Task ShutdownModuleAsync(string moduleKey, Guid organizationId)
    {
        try
        {
            if (_loadedModules.TryGetValue(moduleKey, out var module))
            {
                await module.ShutdownAsync(organizationId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to shutdown module {moduleKey}: {ex.Message}");
        }
    }

    private async Task ConfigureModuleAsync(string moduleKey, Guid organizationId, string configuration)
    {
        try
        {
            if (_loadedModules.TryGetValue(moduleKey, out var module))
            {
                await module.ConfigureAsync(configuration);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to configure module {moduleKey}: {ex.Message}");
        }
    }

    private IModule? LoadModule(string moduleKey)
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic);

            foreach (var assembly in assemblies)
            {
                var moduleTypes = assembly.GetTypes()
                    .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var moduleType in moduleTypes)
                {
                    if (ActivatorUtilities.CreateInstance(_serviceProvider, moduleType) is IModule candidate &&
                        string.Equals(candidate.Key, moduleKey, StringComparison.OrdinalIgnoreCase))
                    {
                        if (_moduleRegistry.GetDescriptor(moduleKey) is { } descriptor)
                        {
                            // Integration hook if metadata needs to be synced
                        }

                        return candidate;
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
