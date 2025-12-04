using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using UniversalBusinessSystem.Core.Entities;
using UniversalBusinessSystem.Core.Services;
using UniversalBusinessSystem.Services;
using UniversalBusinessSystem.Views;

namespace UniversalBusinessSystem.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly IModuleService _moduleService;
    private readonly IModuleRegistry _moduleRegistry;
    private readonly IServiceProvider _serviceProvider;
    private DashboardViewModel? _dashboardViewModel;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private Organization? _currentOrganization;

    [ObservableProperty]
    private List<Module> _activeModules = new();

    [ObservableProperty]
    private UserControl? _currentContent;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private DateTime _currentTime = DateTime.Now;

    [ObservableProperty]
    private List<ModuleNavigationItem> _moduleNavigationItems = new();

    public event EventHandler? LogoutRequested;

    public MainViewModel(
        IAuthenticationService authService,
        IModuleService moduleService,
        IModuleRegistry moduleRegistry,
        IServiceProvider serviceProvider)
    {
        _authService = authService;
        _moduleService = moduleService;
        _moduleRegistry = moduleRegistry;
        _serviceProvider = serviceProvider;
        
        InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Get current user
            CurrentUser = await _authService.GetCurrentUserAsync();
            
            if (CurrentUser?.Organization != null)
            {
                CurrentOrganization = CurrentUser.Organization;
                
                // Load active modules
                await LoadActiveModulesAsync();
                BuildNavigationItems();

                // Show dashboard by default
                ShowDashboard();
            }
            
            // Start time update timer
            StartTimer();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Initialization failed: {ex.Message}";
        }
    }

    private async Task LoadActiveModulesAsync()
    {
        try
        {
            if (CurrentOrganization != null)
            {
                ActiveModules = await _moduleService.GetActiveModulesAsync(CurrentOrganization.Id);
                StatusMessage = $"Loaded {ActiveModules.Count} active modules";
                BuildNavigationItems();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load modules: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NavigateToModuleAsync(string moduleKey)
    {
        try
        {
            var navigationItem = ModuleNavigationItems.FirstOrDefault(n => n.Key == moduleKey);
            var moduleName = navigationItem?.Name ?? moduleKey;

            StatusMessage = $"Loading {moduleName}...";

            var descriptor = _moduleRegistry.GetDescriptor(moduleKey);
            if (descriptor != null)
            {
                CurrentContent = descriptor.ViewFactory(_serviceProvider);
                StatusMessage = $"{moduleName} loaded";
            }
            else
            {
                StatusMessage = $"Module '{moduleName}' is not yet implemented";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load module: {ex.Message}";
        }
    }

    [RelayCommand]
    private void NavigateToModuleManagement()
    {
        try
        {
            var moduleManagementView = _serviceProvider.GetRequiredService<ModuleManagementView>();
            CurrentContent = moduleManagementView;
            StatusMessage = "Module Management loaded";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load module management: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Logout()
    {
        try
        {
            if (CurrentUser != null)
            {
                _authService.LogoutAsync(CurrentUser.Id).Wait();
            }
            
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Logout failed: {ex.Message}";
        }
    }

    private void ShowDashboard()
    {
        try
        {
            var dashboardView = _serviceProvider.GetRequiredService<DashboardView>();

            if (_dashboardViewModel != null)
            {
                _dashboardViewModel.QuickActionRequested -= OnDashboardQuickActionRequested;
            }

            if (dashboardView.DataContext is DashboardViewModel dashboardViewModel)
            {
                _dashboardViewModel = dashboardViewModel;
                _dashboardViewModel.QuickActionRequested += OnDashboardQuickActionRequested;
            }

            CurrentContent = dashboardView;
            StatusMessage = "Dashboard loaded";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load dashboard: {ex.Message}";
        }
    }

    private void StartTimer()
    {
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += (sender, e) => CurrentTime = DateTime.Now;
        timer.Start();
    }

    private void BuildNavigationItems()
    {
        ModuleNavigationItems = ActiveModules
            .Select(module =>
            {
                var descriptor = _moduleRegistry.GetDescriptor(module.Key);
                return new ModuleNavigationItem
                {
                    Key = module.Key,
                    Name = module.Name,
                    Icon = descriptor?.Icon ?? module.Icon ?? "Puzzle"
                };
            })
            .OrderBy(item => item.Name)
            .ToList();
    }

    private void OnDashboardQuickActionRequested(object? sender, DashboardNavigationEventArgs e)
    {
        try
        {
            switch (e.Destination)
            {
                case DashboardDestination.InventoryProducts:
                case DashboardDestination.InventoryCategories:
                    ShowInventory();
                    break;
                case DashboardDestination.ModuleManagement:
                    NavigateToModuleManagement();
                    break;
                case DashboardDestination.UserManagement:
                    ShowUserManagement();
                    break;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Quick action failed: {ex.Message}";
        }
    }

    private void ShowInventory()
    {
        try
        {
            var inventoryView = _serviceProvider.GetRequiredService<InventoryView>();
            CurrentContent = inventoryView;
            StatusMessage = "Inventory loaded";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load inventory: {ex.Message}";
        }
    }

    private void ShowUserManagement()
    {
        try
        {
            CurrentContent = _serviceProvider.GetRequiredService<UserManagementView>();
            StatusMessage = "User management loaded";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load user management: {ex.Message}";
        }
    }
}

public class ModuleNavigationItem
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "Puzzle";
}
