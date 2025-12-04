using System.Windows;
using UniversalBusinessSystem.ViewModels;

namespace UniversalBusinessSystem.Views;

public partial class MainWindow : Window
{
    private MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = App.GetService<MainViewModel>();
        DataContext = _viewModel;

        // Handle logout
        _viewModel.LogoutRequested += (sender, e) =>
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        };
    }
}
