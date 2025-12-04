using UniversalBusinessSystem.Core.Entities;

namespace UniversalBusinessSystem.Core.Services;

public interface IModuleService
{
    Task<List<Module>> GetAvailableModulesAsync();
    Task<List<Module>> GetActiveModulesAsync(Guid organizationId);
    Task<bool> ActivateModuleAsync(Guid organizationId, Guid moduleId, string? configuration = null);
    Task<bool> DeactivateModuleAsync(Guid organizationId, Guid moduleId);
    Task<bool> IsModuleActiveAsync(Guid organizationId, string moduleKey);
    Task<Module?> GetModuleByKeyAsync(string moduleKey);
    Task<List<Module>> GetModulesByTypeAsync(ModuleType moduleType);
    Task<bool> UpdateModuleConfigurationAsync(Guid organizationId, Guid moduleId, string configuration);
}

public interface IModule
{
    string Name { get; }
    string Key { get; }
    string Description { get; }
    ModuleType ModuleType { get; }
    Task<bool> InitializeAsync(Guid organizationId);
    Task<bool> ShutdownAsync(Guid organizationId);
    Task<bool> ConfigureAsync(string configuration);
    List<string> GetRequiredPermissions();
    List<string> GetDependencies();
}
