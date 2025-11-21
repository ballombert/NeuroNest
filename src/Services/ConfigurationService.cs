namespace ADHDWorkspace.Services;

using System.Text.Json;
using ADHDWorkspace.Models;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation of configuration service with JSON file persistence
/// </summary>
public class ConfigurationService(ILogger<ConfigurationService> logger) : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger = logger;
    private AppSettings _settings = new();
    private string _configPath = "config/appsettings.json";
    private string _appDirectory = AppContext.BaseDirectory;

    public AppSettings Settings => _settings;

    /// <summary>
    /// Synchronous load for startup (to avoid deadlocks)
    /// </summary>
    public void Load()
    {
        LoadInternal();
    }
    
    public async Task LoadAsync()
    {
        await Task.Run(() => LoadInternal());
    }
    
    private void LoadInternal()
    {
        try
        {
            var fullPath = Path.Combine(_appDirectory, _configPath);
            
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Configuration file not found at {Path}, using defaults", fullPath);
                _settings = new AppSettings();
                return;
            }

            // Use synchronous File.ReadAllText to avoid deadlock
            var json = File.ReadAllText(fullPath);
            _settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            }) ?? new AppSettings();

            // Update paths for portable mode
            if (_settings.PortableMode)
            {
                UpdatePathsForPortableMode();
            }

            AppConfig.Initialize(_settings);
            _logger.LogInformation("Configuration loaded successfully from {Path}", fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration, using defaults");
            _settings = new AppSettings();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            var fullPath = Path.Combine(_appDirectory, _configPath);
            var directory = Path.GetDirectoryName(fullPath);
            
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create backup
            if (File.Exists(fullPath))
            {
                var backupPath = fullPath + ".backup";
                File.Copy(fullPath, backupPath, true);
                _logger.LogDebug("Created configuration backup at {BackupPath}", backupPath);
            }

            // Save current settings
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(fullPath, json);
            _logger.LogInformation("Configuration saved successfully to {Path}", fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration");
            throw;
        }
    }

    public string ResolvePath(string path)
    {
        if (_settings.PortableMode)
        {
            // In portable mode, use relative paths from app directory
            if (path.StartsWith("C:\\Temp\\"))
            {
                return Path.Combine(_appDirectory, "data", Path.GetFileName(path));
            }
            if (path.StartsWith("C:\\WORK\\"))
            {
                return Path.Combine(_appDirectory, "workspace", path.Replace("C:\\WORK\\Perso\\adhd\\", ""));
            }
        }
        
        return path;
    }

    private void UpdatePathsForPortableMode()
    {
        var dataDir = Path.Combine(_appDirectory, "data");
        var workspaceDir = Path.Combine(_appDirectory, "workspace");

        Directory.CreateDirectory(dataDir);
        Directory.CreateDirectory(workspaceDir);

        _settings.Logging.FilePath = Path.Combine(dataDir, "adhd-workspace-.log");
        _settings.Paths.PomodoroStateFile = Path.Combine(dataDir, "pomodoro-state.txt");
        _settings.Paths.FocusStateFile = Path.Combine(dataDir, "focus-tracker-state.txt");
        _settings.Paths.ContextHistoryPath = Path.Combine(workspaceDir, "context-history");

        _logger.LogInformation("Portable mode enabled, using local directories");
    }
}
