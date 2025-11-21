namespace ADHDWorkspace.Services;

/// <summary>
/// Service for registering and managing global hotkeys
/// </summary>
public interface IHotkeyService
{
    /// <summary>
    /// Registers a global hotkey
    /// </summary>
    /// <returns>True if registration succeeded, false otherwise</returns>
    bool RegisterHotkey(string hotkeyString, Action callback);
    
    /// <summary>
    /// Unregisters a specific hotkey
    /// </summary>
    void UnregisterHotkey(string hotkeyString);
    
    /// <summary>
    /// Unregisters all hotkeys
    /// </summary>
    void UnregisterAll();
}
