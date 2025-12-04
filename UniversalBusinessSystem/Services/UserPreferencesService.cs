using System.IO;
using System.Text.Json;
using UniversalBusinessSystem.Core.Services;

namespace UniversalBusinessSystem.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private readonly string _preferencesPath;

    public UserPreferencesService()
    {
        var appDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UniversalBusinessSystem");
        var configDirectory = Path.Combine(appDataRoot, "config");
        Directory.CreateDirectory(configDirectory);
        _preferencesPath = Path.Combine(configDirectory, "preferences.json");
    }

    public async Task<ModuleManagementPreferences?> GetModuleManagementPreferencesAsync(Guid organizationId)
    {
        if (!File.Exists(_preferencesPath))
        {
            return null;
        }

        await using var stream = File.OpenRead(_preferencesPath);
        var allPreferences = await JsonSerializer.DeserializeAsync<Dictionary<Guid, ModuleManagementPreferences>>(stream);
        if (allPreferences != null && allPreferences.TryGetValue(organizationId, out var preferences))
        {
            return preferences;
        }

        return null;
    }

    public async Task SaveModuleManagementPreferencesAsync(Guid organizationId, ModuleManagementPreferences preferences)
    {
        Dictionary<Guid, ModuleManagementPreferences> allPreferences;
        if (File.Exists(_preferencesPath))
        {
            await using var readStream = File.OpenRead(_preferencesPath);
            allPreferences = await JsonSerializer.DeserializeAsync<Dictionary<Guid, ModuleManagementPreferences>>(readStream) ?? new();
        }
        else
        {
            allPreferences = new();
        }

        allPreferences[organizationId] = preferences;

        await using var writeStream = File.Create(_preferencesPath);
        await JsonSerializer.SerializeAsync(writeStream, allPreferences, new JsonSerializerOptions { WriteIndented = true });
    }
}
