namespace ADHDWorkspace.Services;

using Microsoft.Extensions.Logging;

#if WINDOWS
using System.Runtime.InteropServices;
#endif

/// <summary>
/// Windows implementation of hotkey service using RegisterHotKey API
/// </summary>
public class WindowsHotkeyService(
    ILogger<WindowsHotkeyService> logger,
    INotificationService notificationService) : IHotkeyService
{
    private readonly ILogger<WindowsHotkeyService> _logger = logger;
    private readonly INotificationService _notificationService = notificationService;
    private readonly Dictionary<string, (int id, Action callback)> _registeredHotkeys = new();
    private int _nextId = 1;

#if WINDOWS
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
#endif

    public bool RegisterHotkey(string hotkeyString, Action callback)
    {
#if WINDOWS
        try
        {
            if (_registeredHotkeys.ContainsKey(hotkeyString))
            {
                _logger.LogWarning("Hotkey {Hotkey} already registered", hotkeyString);
                return true;
            }

            var (modifiers, key) = ParseHotkey(hotkeyString);
            var id = _nextId++;

            // TODO: Get window handle from MAUI window
            // For now, this is a placeholder - needs integration with MAUI window
            var hwnd = IntPtr.Zero; // Will be set when MAUI window is available

            if (hwnd == IntPtr.Zero)
            {
                _logger.LogWarning("Window handle not available, hotkey registration deferred: {Hotkey}", hotkeyString);
                _registeredHotkeys[hotkeyString] = (id, callback);
                return true; // Defer registration
            }

            bool success = RegisterHotKey(hwnd, id, modifiers, key);

            if (success)
            {
                _registeredHotkeys[hotkeyString] = (id, callback);
                _logger.LogInformation("Hotkey registered successfully: {Hotkey}", hotkeyString);
            }
            else
            {
                var errorMessage = $"Failed to register hotkey {hotkeyString}. It may be already in use by another application.";
                _logger.LogError(errorMessage);
                _notificationService.ShowError("Hotkey Registration Failed", errorMessage);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception registering hotkey: {Hotkey}", hotkeyString);
            _notificationService.ShowError("Hotkey Error", $"Failed to register {hotkeyString}: {ex.Message}");
            return false;
        }
#else
        _logger.LogWarning("Hotkey registration not supported on this platform: {Hotkey}", hotkeyString);
        return false;
#endif
    }

    public void UnregisterHotkey(string hotkeyString)
    {
#if WINDOWS
        if (_registeredHotkeys.TryGetValue(hotkeyString, out var hotkey))
        {
            var hwnd = IntPtr.Zero; // TODO: Get from MAUI window
            if (hwnd != IntPtr.Zero)
            {
                UnregisterHotKey(hwnd, hotkey.id);
            }
            _registeredHotkeys.Remove(hotkeyString);
            _logger.LogInformation("Hotkey unregistered: {Hotkey}", hotkeyString);
        }
#endif
    }

    public void UnregisterAll()
    {
#if WINDOWS
        var hwnd = IntPtr.Zero; // TODO: Get from MAUI window
        if (hwnd != IntPtr.Zero)
        {
            foreach (var (_, (id, _)) in _registeredHotkeys)
            {
                UnregisterHotKey(hwnd, id);
            }
        }
        _registeredHotkeys.Clear();
        _logger.LogInformation("All hotkeys unregistered");
#endif
    }

#if WINDOWS
    private static (uint modifiers, uint key) ParseHotkey(string hotkeyString)
    {
        uint modifiers = 0;
        uint key = 0;

        var parts = hotkeyString.Split('+', StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            switch (part.ToUpperInvariant())
            {
                case "WIN":
                case "WINDOWS":
                    modifiers |= MOD_WIN;
                    break;
                case "CTRL":
                case "CONTROL":
                    modifiers |= MOD_CONTROL;
                    break;
                case "ALT":
                    modifiers |= MOD_ALT;
                    break;
                case "SHIFT":
                    modifiers |= MOD_SHIFT;
                    break;
                default:
                    // Try to parse as VK code or letter
                    if (part.Length == 1 && char.IsLetterOrDigit(part[0]))
                    {
                        key = (uint)part.ToUpperInvariant()[0];
                    }
                    break;
            }
        }

        return (modifiers, key);
    }
#endif
}
