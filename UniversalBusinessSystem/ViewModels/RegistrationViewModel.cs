using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using UniversalBusinessSystem.Core.Entities;
using UniversalBusinessSystem.Core.Services;
using UniversalBusinessSystem.Data;
using UniversalBusinessSystem.Views;

namespace UniversalBusinessSystem.ViewModels;

public partial class RegistrationViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _organizationName = string.Empty;

    [ObservableProperty]
    private string _organizationDescription = string.Empty;

    [ObservableProperty]
    private string _organizationAddress = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private ShopType? _selectedShopType;

    [ObservableProperty]
    private List<ShopType> _shopTypes = new();

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError = false;

    public event EventHandler? RegistrationSuccess;
    public event EventHandler? NavigateToLogin;

    public RegistrationViewModel(IAuthenticationService authService, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private async Task Register()
    {
        try
        {
            Debug.WriteLine("=== REGISTRATION STARTED ===");
            Debug.WriteLine($"[Register] Username: {Username}");
            Debug.WriteLine($"[Register] Email: {Email}");
            Debug.WriteLine($"[Register] Password set: {!string.IsNullOrEmpty(Password)}");
            Debug.WriteLine($"[Register] Confirm password set: {!string.IsNullOrEmpty(ConfirmPassword)}");

            if (!ValidateRegistration())
            {
                Debug.WriteLine("[Register] Validation failed");
                return;
            }

            Debug.WriteLine("[Register] Validation passed");
            IsLoading = true;
            ClearError();

            var request = new RegistrationRequest
            {
                Username = Username,
                Email = Email,
                Password = Password,
                ConfirmPassword = ConfirmPassword,
                FirstName = FirstName,
                LastName = LastName,
                Phone = Phone,
                OrganizationName = OrganizationName,
                OrganizationDescription = OrganizationDescription,
                OrganizationAddress = OrganizationAddress,
                ShopTypeId = SelectedShopType?.Id ?? Guid.Empty
            };

            Debug.WriteLine("[Register] Sending request to authentication service");
            var result = await _authService.RegisterAsync(request);

            if (result.Success)
            {
                Debug.WriteLine("[Register] Registration successful");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show("Account created successfully! Please log in.", "Registration Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    var login = new LoginWindow();
                    login.Show();

                    Application.Current.Windows.OfType<RegistrationWindow>()
                        .FirstOrDefault()?.Close();
                });

                RegistrationSuccess?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Debug.WriteLine($"[Register] Registration failed: {result.ErrorMessage}");
                SetError(result.ErrorMessage ?? "Registration failed");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Register] Exception: {ex}");
            SetError($"An error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            Debug.WriteLine("=== REGISTRATION COMPLETED ===");
        }
    }

    [RelayCommand]
    private void NavigateToLoginWindow()
    {
        NavigateToLogin?.Invoke(this, EventArgs.Empty);
    }

    private bool ValidateRegistration()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || 
            string.IsNullOrWhiteSpace(LastName) ||
            string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(OrganizationName) ||
            SelectedShopType == null)
        {
            SetError("Please fill in all required fields");
            return false;
        }

        if (!Email.Contains('@'))
        {
            SetError("Please enter a valid email address");
            return false;
        }

        if (Password.Length < 6)
        {
            SetError("Password must be at least 6 characters long");
            return false;
        }

        if (Password != ConfirmPassword)
        {
            SetError("Passwords do not match");
            return false;
        }

        return true;
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    public async Task InitializeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UniversalBusinessSystemDbContext>();
        ShopTypes = await ctx.ShopTypes.AsNoTracking().OrderBy(t => t.Name).ToListAsync();
    }
}
