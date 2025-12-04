using System.IO;
using System.Text.Json.Serialization;
using Serilog.Events;

namespace UniversalBusinessSystem.Settings;

public class AppSettings
{
    [JsonPropertyName("database")]
    public DatabaseSettings Database { get; set; } = new();

    [JsonPropertyName("logging")]
    public LoggingSettings Logging { get; set; } = new();

    [JsonPropertyName("security")]
    public SecuritySettings Security { get; set; } = new();

    [JsonPropertyName("modules")]
    public ModuleSettings Modules { get; set; } = new();

    public static AppSettings CreateDefault(string databasePath, string logDirectory)
    {
        return new AppSettings
        {
            Database = new DatabaseSettings
            {
                ConnectionString = $"Data Source={databasePath}",
                EnableEncryption = false
            },
            Logging = new LoggingSettings
            {
                LogLevel = "Information",
                LogPath = logDirectory
            },
            Security = new SecuritySettings
            {
                MaxFailedLoginAttempts = 5,
                LockoutDurationMinutes = 30,
                RequireEmailVerification = false
            },
            Modules = new ModuleSettings
            {
                AutoLoadCore = true,
                ModuleDirectory = "Modules"
            }
        };
    }

    public void NormalizePaths(string databaseDirectory, string logsDirectory)
    {
        if (string.IsNullOrWhiteSpace(Database.ConnectionString))
        {
            Database.ConnectionString = $"Data Source={Path.Combine(databaseDirectory, "UniversalBusinessSystem.db")}";
        }

        if (string.IsNullOrWhiteSpace(Logging.LogPath))
        {
            Logging.LogPath = logsDirectory;
        }
    }
}

public class DatabaseSettings
{
    [JsonPropertyName("connectionString")]
    public string ConnectionString { get; set; } = string.Empty;

    [JsonPropertyName("enableEncryption")]
    public bool EnableEncryption { get; set; }
}

public class LoggingSettings
{
    [JsonPropertyName("logLevel")]
    public string LogLevel { get; set; } = "Information";

    [JsonPropertyName("logPath")]
    public string LogPath { get; set; } = string.Empty;

    public LogEventLevel GetLogEventLevel()
    {
        return LogLevel?.ToLowerInvariant() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}

public class SecuritySettings
{
    [JsonPropertyName("maxFailedLoginAttempts")]
    public int MaxFailedLoginAttempts { get; set; } = 5;

    [JsonPropertyName("lockoutDurationMinutes")]
    public int LockoutDurationMinutes { get; set; } = 30;

    [JsonPropertyName("requireEmailVerification")]
    public bool RequireEmailVerification { get; set; }
}

public class ModuleSettings
{
    [JsonPropertyName("autoLoadCore")]
    public bool AutoLoadCore { get; set; } = true;

    [JsonPropertyName("moduleDirectory")]
    public string ModuleDirectory { get; set; } = "Modules";
}
