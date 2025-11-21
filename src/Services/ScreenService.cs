namespace ADHDWorkspace.Services;

using Microsoft.Extensions.Logging;

#if WINDOWS
using Windows.Graphics.Display;
using Microsoft.Maui.Controls.PlatformConfiguration;
#endif

/// <summary>
/// Implementation of screen service for Windows platform
/// </summary>
public class ScreenService(ILogger<ScreenService> logger) : IScreenService
{
    private readonly ILogger<ScreenService> _logger = logger;

    public (int X, int Y, int Width, int Height) GetRightmostScreenWorkingArea()
    {
#if WINDOWS
        try
        {
            // Get all displays
            var displays = DeviceDisplay.Current.MainDisplayInfo;
            
            // For now, use primary screen (multi-monitor support would require platform-specific code)
            var width = (int)(displays.Width / displays.Density);
            var height = (int)(displays.Height / displays.Density);
            
            _logger.LogDebug("Rightmost screen: Width={Width}, Height={Height}", width, height);
            return (0, 0, width, height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get screen info, using defaults");
            return (0, 0, 1920, 1080); // Default fallback
        }
#else
        _logger.LogWarning("Screen detection not supported on this platform");
        return (0, 0, 1920, 1080);
#endif
    }

    public (int X, int Y, int Width, int Height) GetPrimaryScreenWorkingArea()
    {
#if WINDOWS
        try
        {
            var displays = DeviceDisplay.Current.MainDisplayInfo;
            var width = (int)(displays.Width / displays.Density);
            var height = (int)(displays.Height / displays.Density);
            
            _logger.LogDebug("Primary screen: Width={Width}, Height={Height}", width, height);
            return (0, 0, width, height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get primary screen info");
            return (0, 0, 1920, 1080);
        }
#else
        return (0, 0, 1920, 1080);
#endif
    }
}
