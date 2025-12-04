using UniversalBusinessSystem.Core.Entities;

namespace UniversalBusinessSystem.Core.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(string username, string password);
    Task<RegistrationResult> RegisterAsync(RegistrationRequest request);
    Task<bool> LogoutAsync(Guid userId);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<User?> GetCurrentUserAsync();
    Task<bool> IsUserLoggedInAsync();
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public User? User { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Token { get; set; }
}

public class RegistrationResult
{
    public bool Success { get; set; }
    public User? User { get; set; }
    public string? ErrorMessage { get; set; }
    public string? EmailVerificationToken { get; set; }
}

public class RegistrationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string OrganizationDescription { get; set; } = string.Empty;
    public string OrganizationAddress { get; set; } = string.Empty;
    public Guid ShopTypeId { get; set; }
    public List<Guid> SelectedUnitIds { get; set; } = new();
}
