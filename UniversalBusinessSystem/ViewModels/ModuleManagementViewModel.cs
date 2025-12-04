using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using UniversalBusinessSystem.Core.Entities;
using UniversalBusinessSystem.Core.Services;
using UniversalBusinessSystem.Data;
using UniversalBusinessSystem.Services;

namespace UniversalBusinessSystem.ViewModels;

public partial class ModuleManagementViewModel : ObservableObject
{
    private readonly UniversalBusinessSystemDbContext _context;
    private readonly IModuleService _moduleService;
    private readonly IModuleRegistry _moduleRegistry;
    private readonly IAuthenticationService _authService;
    private readonly IUserPreferencesService _preferencesService;
    private readonly List<ModuleManagementItem> _availableSource = new();
    private readonly List<ModuleManagementItem> _activeSource = new();
    private bool _suppressPreferenceSave;
    private Guid? _organizationId;

    [ObservableProperty]
    private ObservableCollection<ModuleManagementItem> _availableModules = new();

    [ObservableProperty]
    private ObservableCollection<ModuleManagementItem> _activeModules = new();

    [ObservableProperty]
    private ModuleManagementItem? _selectedAvailableModule;

    [ObservableProperty]
    private ModuleManagementItem? _selectedActiveModule;

    [ObservableProperty]
    private ModuleTypeFilterOption? _selectedAvailableModuleTypeFilter;

    [ObservableProperty]
    private ModuleTypeFilterOption? _selectedActiveModuleTypeFilter;

    [ObservableProperty]
    private SortOptionItem? _selectedSortOption;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public List<ModuleTypeFilterOption> ModuleTypeFilters { get; }
    public List<SortOptionItem> SortOptions { get; }

    public ModuleManagementViewModel(
        UniversalBusinessSystemDbContext context,
        IModuleService moduleService,
        IModuleRegistry moduleRegistry,
        IAuthenticationService authService,
        IUserPreferencesService preferencesService)
    {
        _context = context;
        _moduleService = moduleService;
        _moduleRegistry = moduleRegistry;
        _authService = authService;
        _preferencesService = preferencesService;

        ModuleTypeFilters = BuildModuleTypeFilters();
        SortOptions = BuildSortOptions();
        SelectedAvailableModuleTypeFilter = ModuleTypeFilters.FirstOrDefault();
        SelectedActiveModuleTypeFilter = ModuleTypeFilters.FirstOrDefault();
        SelectedSortOption = SortOptions.FirstOrDefault();

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
                await LoadPreferencesAsync();
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize module management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadDataAsync()
    {
        if (!_organizationId.HasValue)
        {
            return;
        }

        try
        {
            await LoadAvailableModulesAsync();
            await LoadActiveModulesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load module data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadAvailableModulesAsync()
    {
        if (!_organizationId.HasValue)
        {
            AvailableModules.Clear();
            return;
        }

        var allModules = await _moduleService.GetAvailableModulesAsync();
        var activeModuleIds = await _context.OrganizationModules
            .Where(om => om.OrganizationId == _organizationId.Value && om.IsActive)
            .Select(om => om.ModuleId)
            .ToListAsync();

        _availableSource.Clear();
        _availableSource.AddRange(allModules
            .Where(m => !activeModuleIds.Contains(m.Id))
            .Select(MapToItem));

        ApplyFilters();
    }

    private async Task LoadActiveModulesAsync()
    {
        if (!_organizationId.HasValue)
        {
            ActiveModules.Clear();
            return;
        }

        _activeSource.Clear();
        _activeSource.AddRange((await _moduleService.GetActiveModulesAsync(_organizationId.Value))
            .Select(MapToItem));

        ApplyFilters();
    }

    [RelayCommand]
    private async Task ActivateModule()
    {
        if (SelectedAvailableModule == null || !_organizationId.HasValue)
        {
            return;
        }

        try
        {
            var module = SelectedAvailableModule;
            var success = await _moduleService.ActivateModuleAsync(_organizationId.Value, module.ModuleId);

            if (!success)
            {
                MessageBox.Show("Failed to activate module", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _availableSource.RemoveAll(item => item.ModuleId == module.ModuleId);
            _activeSource.Add(module);
            SelectedAvailableModule = null;
            ApplyFilters();
            MessageBox.Show($"Module '{module.Name}' activated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to activate module: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeactivateModule()
    {
        if (SelectedActiveModule == null)
        {
            return;
        }

        await ConfirmAndDeactivateAsync(SelectedActiveModule);
    }

    [RelayCommand]
    private async Task DeactivateModuleWithParameter(ModuleManagementItem module)
    {
        if (module == null)
        {
            return;
        }

        await ConfirmAndDeactivateAsync(module);
    }

    private async Task ConfirmAndDeactivateAsync(ModuleManagementItem module)
    {
        if (!_organizationId.HasValue)
        {
            return;
        }

        var result = MessageBox.Show($"Are you sure you want to deactivate '{module.Name}'?", "Confirm Deactivation", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var success = await _moduleService.DeactivateModuleAsync(_organizationId.Value, module.ModuleId);

            if (!success)
            {
                MessageBox.Show("Failed to deactivate module", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _activeSource.RemoveAll(item => item.ModuleId == module.ModuleId);
            _availableSource.Add(module);
            if (ReferenceEquals(SelectedActiveModule, module))
            {
                SelectedActiveModule = null;
            }
            ApplyFilters();
            MessageBox.Show($"Module '{module.Name}' deactivated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to deactivate module: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private ModuleManagementItem MapToItem(Module module)
    {
        var descriptor = _moduleRegistry.GetDescriptor(module.Key);
        return new ModuleManagementItem
        {
            ModuleId = module.Id,
            Key = module.Key,
            Name = module.Name,
            Description = module.Description,
            Icon = descriptor?.Icon ?? module.Icon ?? "Puzzle",
            ModuleType = module.ModuleType
        };
    }

    private static void ReplaceCollection(ObservableCollection<ModuleManagementItem> target, IEnumerable<ModuleManagementItem> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void ApplyFilters()
    {
        ReplaceCollection(AvailableModules, SortItems(ApplySearchFilter(ApplyModuleTypeFilter(_availableSource, SelectedAvailableModuleTypeFilter))));
        ReplaceCollection(ActiveModules, SortItems(ApplySearchFilter(ApplyModuleTypeFilter(_activeSource, SelectedActiveModuleTypeFilter))));
    }

    private IEnumerable<ModuleManagementItem> ApplyModuleTypeFilter(IEnumerable<ModuleManagementItem> source, ModuleTypeFilterOption? filter)
    {
        if (filter?.ModuleType.HasValue == true)
        {
            return source.Where(item => item.ModuleType == filter.ModuleType.Value);
        }

        return source;
    }

    private IEnumerable<ModuleManagementItem> ApplySearchFilter(IEnumerable<ModuleManagementItem> source)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return source;
        }

        var term = SearchText.Trim().ToLowerInvariant();
        return source.Where(item =>
            item.Name.ToLowerInvariant().Contains(term) ||
            (item.Description != null && item.Description.ToLowerInvariant().Contains(term)) ||
            item.Key.ToLowerInvariant().Contains(term));
    }

    private IEnumerable<ModuleManagementItem> SortItems(IEnumerable<ModuleManagementItem> source)
    {
        var sortOption = SelectedSortOption?.Option ?? ModuleSortOption.NameAscending;

        return sortOption switch
        {
            ModuleSortOption.NameDescending => source.OrderByDescending(item => item.Name),
            ModuleSortOption.ModuleType => source.OrderBy(item => item.ModuleType).ThenBy(item => item.Name),
            ModuleSortOption.Key => source.OrderBy(item => item.Key),
            _ => source.OrderBy(item => item.Name)
        };
    }

    private List<ModuleTypeFilterOption> BuildModuleTypeFilters()
    {
        var filters = new List<ModuleTypeFilterOption>
        {
            new ModuleTypeFilterOption("All Types", null)
        };

        filters.AddRange(Enum.GetValues<ModuleType>().Select(mt => new ModuleTypeFilterOption(mt.ToString(), mt)));
        return filters;
    }

    private List<SortOptionItem> BuildSortOptions()
    {
        return new List<SortOptionItem>
        {
            new SortOptionItem("Name (A-Z)", ModuleSortOption.NameAscending),
            new SortOptionItem("Name (Z-A)", ModuleSortOption.NameDescending),
            new SortOptionItem("Module Type", ModuleSortOption.ModuleType),
            new SortOptionItem("Key", ModuleSortOption.Key)
        };
    }

    partial void OnSelectedAvailableModuleTypeFilterChanged(ModuleTypeFilterOption? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedActiveModuleTypeFilterChanged(ModuleTypeFilterOption? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedSortOptionChanged(SortOptionItem? value)
    {
        ApplyFilters();
        SavePreferencesAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
        SavePreferencesAsync();
    }

    partial void OnSelectedAvailableModuleChanged(ModuleManagementItem? value)
    {
    }

    partial void OnSelectedActiveModuleChanged(ModuleManagementItem? value)
    {
    }

    private async Task LoadPreferencesAsync()
    {
        if (!_organizationId.HasValue)
        {
            return;
        }

        var preferences = await _preferencesService.GetModuleManagementPreferencesAsync(_organizationId.Value);
        if (preferences == null)
        {
            return;
        }

        _suppressPreferenceSave = true;
        try
        {
            SearchText = preferences.SearchText ?? string.Empty;
            SelectedSortOption = SortOptions.FirstOrDefault(option => option.Option == preferences.SortOption) ?? SortOptions.First();
            SelectedAvailableModuleTypeFilter = ModuleTypeFilters.FirstOrDefault(filter => filter.ModuleType == preferences.AvailableFilter) ?? ModuleTypeFilters.First();
            SelectedActiveModuleTypeFilter = ModuleTypeFilters.FirstOrDefault(filter => filter.ModuleType == preferences.ActiveFilter) ?? ModuleTypeFilters.First();
        }
        finally
        {
            _suppressPreferenceSave = false;
        }
    }

    private async void SavePreferencesAsync()
    {
        if (_suppressPreferenceSave || !_organizationId.HasValue)
        {
            return;
        }

        var preferences = new ModuleManagementPreferences
        {
            SearchText = SearchText,
            SortOption = SelectedSortOption?.Option ?? ModuleSortOption.NameAscending,
            AvailableFilter = SelectedAvailableModuleTypeFilter?.ModuleType,
            ActiveFilter = SelectedActiveModuleTypeFilter?.ModuleType
        };

        await _preferencesService.SaveModuleManagementPreferencesAsync(_organizationId.Value, preferences);
    }
}

public class ModuleManagementItem
{
    public Guid ModuleId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "Puzzle";
    public ModuleType ModuleType { get; set; }
}

public class ModuleTypeFilterOption
{
    public ModuleTypeFilterOption(string displayName, ModuleType? moduleType)
    {
        DisplayName = displayName;
        ModuleType = moduleType;
    }

    public string DisplayName { get; }
    public ModuleType? ModuleType { get; }
}

public class SortOptionItem
{
    public SortOptionItem(string displayName, ModuleSortOption option)
    {
        DisplayName = displayName;
        Option = option;
    }

    public string DisplayName { get; }
    public ModuleSortOption Option { get; }
}
