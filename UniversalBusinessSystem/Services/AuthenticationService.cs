using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;
using System.Security.Cryptography;
using UniversalBusinessSystem.Core.Entities;
using UniversalBusinessSystem.Core.Services;
using UniversalBusinessSystem.Data;

namespace UniversalBusinessSystem.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IServiceProvider _serviceProvider;
    private User? _currentUser;

    public AuthenticationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<AuthenticationResult> LoginAsync(string username, string password)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            Microsoft.Extensions.Logging.ILogger? logger = null;
            try
            {
                logger = scope.ServiceProvider.GetService<Microsoft.Extensions.Logging.ILogger<AuthenticationService>>();
            }
            catch
            {
                // logger is optional
            }

            logger?.LogInformation("[LoginAsync] Begin login for {Username}", username);

            var user = await context.Users
                .Include(u => u.Organization)
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            logger?.LogInformation("[LoginAsync] Query complete. User found: {UserFound}", user != null);

            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password"
                };
            }

            if (!user.IsActive)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Account is deactivated"
                };
            }

            if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = $"Account is locked until {user.LockedUntil.Value:yyyy-MM-dd HH:mm:ss}"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                }

                context.Users.Update(user);
                await context.SaveChangesAsync();

                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password"
                };
            }

            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            user.LastLoginAt = DateTime.UtcNow;

            context.Users.Update(user);
            await context.SaveChangesAsync();

            _currentUser = user;

            return new AuthenticationResult
            {
                Success = true,
                User = user,
                Token = GenerateToken(user)
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoginAsync] Exception: {ex}");
            try
            {
                var logDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "UniversalBusinessSystem",
                    "logs");

                Directory.CreateDirectory(logDirectory);

                var logPath = Path.Combine(logDirectory, "login_error.txt");
                File.WriteAllText(logPath,
                    $"Timestamp: {DateTime.UtcNow:O}{Environment.NewLine}" +
                    $"Username: {username}{Environment.NewLine}" +
                    $"Error: {ex}{Environment.NewLine}");
            }
            catch
            {
                // ignore logging failures
            }

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Login failed: {ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}",
                        "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = $"Login failed: {ex.Message}{Environment.NewLine}{ex.StackTrace}"
            };
        }
    }

    public async Task<RegistrationResult> RegisterAsync(RegistrationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.OrganizationName))
            {
                return new RegistrationResult
                {
                    Success = false,
                    ErrorMessage = "All required fields must be filled"
                };
            }

            if (request.Password != request.ConfirmPassword)
            {
                return new RegistrationResult
                {
                    Success = false,
                    ErrorMessage = "Passwords do not match"
                };
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();
            var databaseService = scope.ServiceProvider.GetRequiredService<DatabaseService>();

            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser != null)
            {
                return new RegistrationResult
                {
                    Success = false,
                    ErrorMessage = "Username or email already exists"
                };
            }

            var shopType = await context.ShopTypes.FindAsync(request.ShopTypeId);
            if (shopType == null)
            {
                return new RegistrationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid shop type selected"
                };
            }

            var organization = new Organization
            {
                Name = request.OrganizationName,
                Description = request.OrganizationDescription,
                Address = request.OrganizationAddress,
                Email = request.Email,
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                IsActive = true,
                IsEmailVerified = false,
                EmailVerificationToken = GenerateEmailVerificationToken(),
                EmailVerificationExpires = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            var success = await databaseService.CreateOrganizationAsync(organization, user, shopType);

            if (!success)
            {
                return new RegistrationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to create organization"
                };
            }

            if (request.SelectedUnitIds.Any())
            {
                await AddCustomUnitsAsync(context, organization.Id, request.SelectedUnitIds);
            }

            return new RegistrationResult
            {
                Success = true,
                User = user,
                EmailVerificationToken = user.EmailVerificationToken
            };
        }
        catch (Exception ex)
        {
            return new RegistrationResult
            {
                Success = false,
                ErrorMessage = $"Registration failed: {ex.Message}"
            };
        }
    }

    private async Task AddCustomUnitsAsync(UniversalBusinessSystemDbContext context, Guid organizationId, List<Guid> unitIds)
    {
        foreach (var unitId in unitIds)
        {
            var existingOrgUnit = await context.OrganizationUnits
                .FirstOrDefaultAsync(ou => ou.OrganizationId == organizationId && ou.UnitId == unitId);

            if (existingOrgUnit == null)
            {
                context.OrganizationUnits.Add(new OrganizationUnit
                {
                    OrganizationId = organizationId,
                    UnitId = unitId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
    }

    public Task<bool> LogoutAsync(Guid userId)
    {
        try
        {
            _currentUser = null;
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token &&
                                        u.EmailVerificationExpires > DateTime.UtcNow);

            if (user == null)
            {
                return false;
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationExpires = null;

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return true;
            }

            user.EmailVerificationToken = GenerateEmailVerificationToken();
            user.EmailVerificationExpires = DateTime.UtcNow.AddHours(1);

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token &&
                                        u.EmailVerificationExpires > DateTime.UtcNow);

            if (user == null)
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.EmailVerificationToken = null;
            user.EmailVerificationExpires = null;
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<User?> GetCurrentUserAsync()
    {
        return Task.FromResult(_currentUser);
    }

    public Task<bool> IsUserLoggedInAsync()
    {
        return Task.FromResult(_currentUser != null);
    }

    private static string GenerateToken(User user)
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    private static string GenerateEmailVerificationToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
