using UniversalBusinessSystem.Core.Entities;

namespace UniversalBusinessSystem.Core.Services;

public interface IUserPreferencesService
{
    Task<ModuleManagementPreferences?> GetModuleManagementPreferencesAsync(Guid organizationId);
    Task SaveModuleManagementPreferencesAsync(Guid organizationId, ModuleManagementPreferences preferences);
}

public class ModuleManagementPreferences
{
    public string? SearchText { get; set; }
    public ModuleSortOption SortOption { get; set; } = ModuleSortOption.NameAscending;
    public ModuleType? AvailableFilter { get; set; }
    public ModuleType? ActiveFilter { get; set; }
}

public enum ModuleSortOption
{
    NameAscending,
    NameDescending,
    ModuleType,
    Key
}
