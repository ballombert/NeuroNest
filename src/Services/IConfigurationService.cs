namespace ADHDWorkspace.Services;

using ADHDWorkspace.Models;

/// <summary>
/// Service for loading and saving application configuration
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Current application settings
    /// </summary>
    AppSettings Settings { get; }
    
    /// <summary>
    /// Loads settings from appsettings.json
    /// </summary>
    Task LoadAsync();
    
    /// <summary>
    /// Saves current settings to appsettings.json with automatic backup
    /// </summary>
    Task SaveAsync();
    
    /// <summary>
    /// Resolves paths based on portable mode
    /// </summary>
    string ResolvePath(string path);
}
