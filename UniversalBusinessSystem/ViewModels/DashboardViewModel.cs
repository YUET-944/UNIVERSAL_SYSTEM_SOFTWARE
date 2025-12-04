using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using UniversalBusinessSystem.Core.Entities;
using UniversalBusinessSystem.Core.Services;
using UniversalBusinessSystem.Data;
using UniversalBusinessSystem.Services;

namespace UniversalBusinessSystem.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly UniversalBusinessSystemDbContext _context;
    private readonly IAuthenticationService _authService;
    private readonly IModuleService _moduleService;
    private readonly IModuleRegistry _moduleRegistry;
    private Guid? _organizationId;

    [ObservableProperty]
    private List<StatCard> _statCards = new();

    [ObservableProperty]
    private List<QuickAction> _quickActions = new();

    [ObservableProperty]
    private List<RecentActivity> _recentActivities = new();

    [ObservableProperty]
    private List<ModuleSummaryItem> _moduleSummaries = new();

    public DashboardViewModel(
        UniversalBusinessSystemDbContext context,
        IAuthenticationService authService,
        IModuleService moduleService,
        IModuleRegistry moduleRegistry)
    {
        _context = context;
        _authService = authService;
        _moduleService = moduleService;
        _moduleRegistry = moduleRegistry;

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            _organizationId = user?.Organization?.Id;

            if (_organizationId.HasValue)
            {
                await LoadDashboardDataAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialise dashboard data: {ex.Message}");
        }
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            await LoadStatCardsAsync();
            LoadQuickActions();
            LoadRecentActivities();
            await LoadModuleSummariesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load dashboard data: {ex.Message}");
        }
    }

    private async Task LoadStatCardsAsync()
    {
        if (!_organizationId.HasValue)
        {
            StatCards = new List<StatCard>();
            return;
        }

        var orgId = _organizationId.Value;

        var productCount = await _context.Products
            .Where(p => p.OrganizationId == orgId)
            .CountAsync();

        var categoryCount = await _context.Categories
            .Where(c => c.OrganizationId == orgId)
            .CountAsync();

        var userCount = await _context.Users
            .Where(u => u.OrganizationId == orgId)
            .CountAsync();

        var activeModuleCount = await _context.OrganizationModules
            .Where(om => om.OrganizationId == orgId && om.IsActive)
            .CountAsync();

        StatCards = new List<StatCard>
        {
            new StatCard { Icon = "Package", Title = "Products", Value = productCount.ToString() },
            new StatCard { Icon = "Folder", Title = "Categories", Value = categoryCount.ToString() },
            new StatCard { Icon = "Account", Title = "Users", Value = userCount.ToString() },
            new StatCard { Icon = "Puzzle", Title = "Active Modules", Value = activeModuleCount.ToString() }
        };
    }

    private void LoadQuickActions()
    {
        QuickActions = new List<QuickAction>
        {
            new QuickAction { Icon = "Plus", Title = "Add Product", Description = "Create a new product in inventory", Command = AddProductCommand },
            new QuickAction { Icon = "Plus", Title = "Add Category", Description = "Add a new category to organise products", Command = AddCategoryCommand },
            new QuickAction { Icon = "AccountPlus", Title = "Add User", Description = "Invite a team member to your shop", Command = AddUserCommand },
            new QuickAction { Icon = "Cog", Title = "Module Management", Description = "Enable or disable modules for your organisation", Command = ManageModulesCommand }
        };
    }

    private void LoadRecentActivities()
    {
        RecentActivities = new List<RecentActivity>
        {
            new RecentActivity { Icon = "Information", Description = "System started successfully", Time = DateTime.Now.AddMinutes(-5) },
            new RecentActivity { Icon = "AccountPlus", Description = "New user registered", Time = DateTime.Now.AddHours(-2) },
            new RecentActivity { Icon = "PackagePlus", Description = "Product added to inventory", Time = DateTime.Now.AddHours(-4) },
            new RecentActivity { Icon = "Puzzle", Description = "Module activated", Time = DateTime.Now.AddDays(-1) }
        };
    }

    [RelayCommand]
    private void AddProduct()
    {
        QuickActionRequested?.Invoke(this, new DashboardNavigationEventArgs(DashboardDestination.InventoryProducts));
    }

    [RelayCommand]
    private void AddCategory()
    {
        QuickActionRequested?.Invoke(this, new DashboardNavigationEventArgs(DashboardDestination.InventoryCategories));
    }

    [RelayCommand]
    private void AddUser()
    {
        QuickActionRequested?.Invoke(this, new DashboardNavigationEventArgs(DashboardDestination.UserManagement));
    }

    [RelayCommand]
    private void ManageModules()
    {
        QuickActionRequested?.Invoke(this, new DashboardNavigationEventArgs(DashboardDestination.ModuleManagement));
    }

    private async Task LoadModuleSummariesAsync()
    {
        if (!_organizationId.HasValue)
        {
            ModuleSummaries = new List<ModuleSummaryItem>();
            return;
        }

        var activeModules = await _moduleService.GetActiveModulesAsync(_organizationId.Value);
        ModuleSummaries = activeModules
            .Select(module =>
            {
                var descriptor = _moduleRegistry.GetDescriptor(module.Key);
                return new ModuleSummaryItem
                {
                    Name = module.Name,
                    Icon = descriptor?.Icon ?? module.Icon ?? "Puzzle",
                    Description = module.Description ?? descriptor?.Name ?? string.Empty
                };
            })
            .OrderBy(summary => summary.Name)
            .ToList();
    }
}

public class StatCard
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class QuickAction
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IRelayCommand? Command { get; set; }
}

public class RecentActivity
{
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Time { get; set; }
}

public class ModuleSummaryItem
{
    public string Icon { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public enum DashboardDestination
{
    InventoryProducts,
    InventoryCategories,
    UserManagement,
    ModuleManagement
}

public class DashboardNavigationEventArgs : EventArgs
{
    public DashboardNavigationEventArgs(DashboardDestination destination)
    {
        Destination = destination;
    }

    public DashboardDestination Destination { get; }
}

public partial class DashboardViewModel
{
    public event EventHandler<DashboardNavigationEventArgs>? QuickActionRequested;
}
