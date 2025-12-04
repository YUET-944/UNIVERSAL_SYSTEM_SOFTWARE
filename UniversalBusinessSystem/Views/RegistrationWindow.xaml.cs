using System.Windows;
using System.Windows.Controls;
using UniversalBusinessSystem.ViewModels;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace UniversalBusinessSystem.Views;

public partial class RegistrationWindow : Window
{
    private RegistrationViewModel _viewModel;

    public RegistrationWindow()
    {
        InitializeComponent();
        _viewModel = App.GetService<RegistrationViewModel>();
        DataContext = _viewModel;

        Debug.WriteLine($"[Registration] DataContext: {_viewModel.GetType().Name}");

        Loaded += RegistrationWindow_Loaded;

        // Handle registration success
        _viewModel.RegistrationSuccess += (sender, e) =>
        {
            MessageBox.Show("Account created successfully! Please check your email for verification.", 
                "Registration Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        };

        // Handle navigation to login
        _viewModel.NavigateToLogin += (sender, e) =>
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        };
    }

    private async void RegistrationWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Registration] Failed to initialize view model: {ex}");
            MessageBox.Show($"Unable to load registration data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.Password = passwordBox.Password;
        }
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.ConfirmPassword = passwordBox.Password;
        }
    }

    private async void CreateAccountButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Registration] CreateAccount button clicked");

        if (_viewModel.RegisterCommand is IAsyncRelayCommand asyncCmd && asyncCmd.CanExecute(null))
        {
            Debug.WriteLine("[Registration] RegisterCommand can execute – awaiting async execution");
            await asyncCmd.ExecuteAsync(null);
        }
        else if (_viewModel.RegisterCommand.CanExecute(null))
        {
            Debug.WriteLine("[Registration] RegisterCommand is synchronous – executing");
            _viewModel.RegisterCommand.Execute(null);
        }
        else
        {
            Debug.WriteLine("[Registration] RegisterCommand.CanExecute returned false");
            MessageBox.Show("Create Account command is currently disabled.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void TestButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Registration] Test button clicked");
        MessageBox.Show("Test button works!", "Diagnostics", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DiagnoseRegistration_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("=== REGISTRATION DIAGNOSTICS ===");

        if (DataContext == null)
        {
            Debug.WriteLine("[Diagnostic] DataContext is null");
            MessageBox.Show("DataContext is null", "Diagnostics", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var viewModel = DataContext;
        var vmType = viewModel.GetType();
        Debug.WriteLine($"[Diagnostic] ViewModel Type: {vmType.FullName}");

        // Log ICommand properties
        var commandProperties = vmType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(prop => typeof(ICommand).IsAssignableFrom(prop.PropertyType))
            .ToList();

        Debug.WriteLine("[Diagnostic] ICommand properties:");
        foreach (var prop in commandProperties)
        {
            var command = prop.GetValue(viewModel) as ICommand;
            Debug.WriteLine($"  - {prop.Name}: {(command != null ? command.GetType().Name : "null")}");
            if (command != null)
            {
                Debug.WriteLine($"    CanExecute: {command.CanExecute(null)}");
            }
        }

        // Look for methods decorated with RelayCommand
        var relayMethods = vmType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(m => m.GetCustomAttributes().Any(attr => attr.GetType().Name.Contains("RelayCommand")))
            .ToList();

        Debug.WriteLine("[Diagnostic] RelayCommand methods:");
        foreach (var method in relayMethods)
        {
            Debug.WriteLine($"  - {method.Name} (ReturnType: {method.ReturnType.Name})");
        }

        // Check key registration properties
        string[] propertiesToCheck = { "Username", "Email", "Password", "ConfirmPassword", "IsBusy", "CanRegister" };
        foreach (var propertyName in propertiesToCheck)
        {
            var property = vmType.GetProperty(propertyName);
            if (property != null)
            {
                var value = property.GetValue(viewModel);
                Debug.WriteLine($"[Diagnostic] {propertyName}: {value ?? "(null)"}");
            }
        }

        MessageBox.Show("Diagnostics complete. Check Output window.", "Diagnostics", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
