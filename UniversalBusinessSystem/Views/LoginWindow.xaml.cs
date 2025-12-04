using System.Windows;
using System.Windows.Controls;
using UniversalBusinessSystem.ViewModels;

namespace UniversalBusinessSystem.Views;

public partial class LoginWindow : Window
{
    private LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
        _viewModel = App.GetService<LoginViewModel>();
        DataContext = _viewModel;

        // Handle login success
        _viewModel.LoginSuccess += (sender, e) =>
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        };

        // Handle navigation to registration
        _viewModel.NavigateToRegistration += (sender, e) =>
        {
            var registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            Close();
        };
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.Password = passwordBox.Password;
        }
    }
}
