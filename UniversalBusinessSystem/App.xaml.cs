using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using Serilog;
using MaterialDesignThemes.Wpf;
using System.Text.Json;
using System.IO;
using UniversalBusinessSystem.Core.Services;
using UniversalBusinessSystem.Data;
using UniversalBusinessSystem.ViewModels;
using UniversalBusinessSystem.Views;
using UniversalBusinessSystem.Settings;
using UniversalBusinessSystem.Services;

namespace UniversalBusinessSystem;

public partial class App : Application
{
    private IHost? _host;
    private static readonly string AppDataRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UniversalBusinessSystem");
    private static readonly string DatabaseDirectory = Path.Combine(AppDataRoot, "database");
    private static readonly string LogsDirectory = Path.Combine(AppDataRoot, "logs");
    private static readonly string ConfigDirectory = Path.Combine(AppDataRoot, "config");
    private static readonly string TempDirectory = Path.Combine(AppDataRoot, "temp");
    private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "config.json");

    protected override async void OnStartup(StartupEventArgs e)
    {
        var appSettings = EnsureAppDataStructure();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(appSettings.Logging.GetLogEventLevel())
            .WriteTo.File(
                path: Path.Combine(LogsDirectory, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Settings
                    services.AddSingleton(appSettings);

                    // Database
                    services.AddDbContext<UniversalBusinessSystemDbContext>(options =>
                        options.UseSqlite(appSettings.Database.ConnectionString));

                    // Core Services
                    services.AddSingleton<IAuthenticationService, AuthenticationService>();
                    services.AddSingleton<IModuleRegistry, ModuleRegistry>();
                    services.AddSingleton<IModuleService, ModuleService>();
                    services.AddSingleton<IUserPreferencesService, UserPreferencesService>();
                    services.AddScoped<IInventoryService, InventoryService>();
                    services.AddSingleton<DatabaseService>();

                    // ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<RegistrationViewModel>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<UserManagementViewModel>();
                    services.AddTransient<InventoryViewModel>();
                    services.AddTransient<ModuleManagementViewModel>();

                    // Views for dynamic resolution
                    services.AddTransient<UserManagementView>();
                    services.AddTransient<InventoryView>();
                    services.AddTransient<ModuleManagementView>();
                    services.AddTransient<DashboardView>();
                })
                .Build();

            // Initialize database
            var databaseService = _host.Services.GetRequiredService<DatabaseService>();
            await databaseService.InitializeDatabaseAsync().ConfigureAwait(false);

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
            MessageBox.Show($"Application failed to start: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    public static T GetService<T>() where T : class
    {
        if (Current is App app && app._host != null)
        {
            return app._host.Services.GetRequiredService<T>();
        }
        throw new InvalidOperationException("Service provider not available");
    }

    private static AppSettings EnsureAppDataStructure()
    {
        var directories = new[]
        {
            AppDataRoot,
            DatabaseDirectory,
            LogsDirectory,
            ConfigDirectory,
            TempDirectory
        };

        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        AppSettings settings;

        if (!File.Exists(ConfigFilePath))
        {
            settings = AppSettings.CreateDefault(Path.Combine(DatabaseDirectory, "UniversalBusinessSystem.db"), LogsDirectory);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        else
        {
            try
            {
                var json = File.ReadAllText(ConfigFilePath);
                settings = JsonSerializer.Deserialize<AppSettings>(json) ??
                    AppSettings.CreateDefault(Path.Combine(DatabaseDirectory, "UniversalBusinessSystem.db"), LogsDirectory);
            }
            catch
            {
                settings = AppSettings.CreateDefault(Path.Combine(DatabaseDirectory, "UniversalBusinessSystem.db"), LogsDirectory);
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);
            }
        }

        settings.NormalizePaths(DatabaseDirectory, LogsDirectory);

        return settings;
    }
}
