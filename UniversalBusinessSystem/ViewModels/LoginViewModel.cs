using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using UniversalBusinessSystem.Core.Entities;
using UniversalBusinessSystem.Core.Services;

namespace UniversalBusinessSystem.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe = false;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError = false;

    public event EventHandler? LoginSuccess;
    public event EventHandler? NavigateToRegistration;

    public LoginViewModel(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                SetError("Please enter username and password");
                return;
            }

            IsLoading = true;
            ClearError();

            var result = await _authService.LoginAsync(Username, Password);

            if (result.Success && result.User != null)
            {
                LoginSuccess?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                SetError(result.ErrorMessage ?? "Login failed");
            }
        }
        catch (Exception ex)
        {
            SetError($"An error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NavigateToRegistrationWindow()
    {
        NavigateToRegistration?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                SetError("Please enter your email address");
                return;
            }

            IsLoading = true;
            ClearError();

            var result = await _authService.ForgotPasswordAsync(Username);

            if (result)
            {
                MessageBox.Show("Password reset instructions have been sent to your email.", 
                    "Password Reset", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                SetError("Failed to send password reset email");
            }
        }
        catch (Exception ex)
        {
            SetError($"An error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
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
}
