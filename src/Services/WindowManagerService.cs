namespace ADHDWorkspace.Services;

using ADHDWorkspace.Views;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

/// <summary>
/// Service for managing window instances and preventing duplicates
/// </summary>
public class WindowManagerService : IWindowManagerService
{
    private readonly ILogger<WindowManagerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Window> _openWindows = new();
    private readonly object _lock = new();

#if WINDOWS
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;
#endif

    public WindowManagerService(ILogger<WindowManagerService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> OpenWindowAsync<TPage>() where TPage : ContentPage
    {
        var pageType = typeof(TPage);

        lock (_lock)
        {
            // Check if window is already open
            if (_openWindows.TryGetValue(pageType, out var existingWindow))
            {
                _logger.LogInformation("Window {PageType} already open, bringing to foreground", pageType.Name);
                
                // Bring to foreground on dispatcher
                Application.Current?.Dispatcher.Dispatch(async () =>
                {
                    await BringWindowToForegroundAsync(existingWindow);
                });
                
                return true;
            }
        }

        // Create new window
        try
        {
            var page = _serviceProvider.GetService<TPage>();
            if (page == null)
            {
                _logger.LogError("Failed to resolve page type {PageType} from DI", pageType.Name);
                return false;
            }

            var window = new Window(page);
            ConfigureWindowSizing(window, pageType);
            
            // Track window lifecycle
            window.Destroying += (s, e) =>
            {
                lock (_lock)
                {
                    _openWindows.Remove(pageType);
                    _logger.LogInformation("Window {PageType} destroyed and untracked", pageType.Name);
                }
            };

            lock (_lock)
            {
                _openWindows[pageType] = window;
            }

            // Open window on UI thread
            await Application.Current?.Dispatcher.DispatchAsync(() =>
            {
                Application.Current?.OpenWindow(window);
                _logger.LogInformation("Window {PageType} opened", pageType.Name);
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open window {PageType}", pageType.Name);
            return false;
        }
    }

    public void CloseWindow<TPage>() where TPage : ContentPage
    {
        var pageType = typeof(TPage);

        lock (_lock)
        {
            if (_openWindows.TryGetValue(pageType, out var window))
            {
                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    Application.Current?.CloseWindow(window);
                    _logger.LogInformation("Window {PageType} closed", pageType.Name);
                });

                _openWindows.Remove(pageType);
            }
        }
    }

    public bool IsWindowOpen<TPage>() where TPage : ContentPage
    {
        var pageType = typeof(TPage);
        lock (_lock)
        {
            return _openWindows.ContainsKey(pageType);
        }
    }

    public async Task<bool> BringToForegroundAsync<TPage>() where TPage : ContentPage
    {
        var pageType = typeof(TPage);

        Window? window = null;
        lock (_lock)
        {
            _openWindows.TryGetValue(pageType, out window);
        }

        if (window != null)
        {
            await Application.Current?.Dispatcher.DispatchAsync(async () =>
            {
                await BringWindowToForegroundAsync(window);
            });
            return true;
        }

        return false;
    }

    public void UnregisterWindow(ContentPage page)
    {
        if (page == null) return;

        var pageType = page.GetType();
        
        lock (_lock)
        {
            if (_openWindows.TryGetValue(pageType, out var window))
            {
                if (window.Page == page)
                {
                    _openWindows.Remove(pageType);
                    _logger.LogInformation("Window {PageType} unregistered", pageType.Name);
                }
            }
        }
    }

    private async Task BringWindowToForegroundAsync(Window window)
    {
        try
        {
            // Try MAUI's built-in activation first
            if (window != null)
            {
                // Attempt to activate via MAUI
                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    // MAUI doesn't have a direct Activate method, so we try to set it as main page temporarily
                    // This is a workaround - the real activation happens via Windows API
                });

#if WINDOWS
                // Fallback to Windows API for reliable foreground activation
                var handler = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                if (handler != null)
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(handler);
                    
                    // Restore if minimized
                    ShowWindow(hwnd, SW_RESTORE);
                    
                    // Bring to foreground
                    SetForegroundWindow(hwnd);
                    
                    _logger.LogDebug("Window brought to foreground using Windows API");
                }
#endif
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to bring window to foreground");
        }
    }

    private static void ConfigureWindowSizing(Window window, Type pageType)
    {
        void Apply(double minWidth, double minHeight, double maxWidth, double maxHeight, double initialWidth, double initialHeight)
        {
            window.MinimumWidth = minWidth;
            window.MinimumHeight = minHeight;
            window.MaximumWidth = maxWidth;
            window.MaximumHeight = maxHeight;
            window.Width = initialWidth;
            window.Height = initialHeight;
        }

        if (pageType == typeof(SettingsPage))
        {
            Apply(500, 600, 900, 800, 720, 720);
        }
        else if (pageType == typeof(QuickCapturePage))
        {
            Apply(400, 300, 800, 600, 600, 420);
        }
        else if (pageType == typeof(FocusTrackerPage))
        {
            Apply(450, 500, 900, 900, 700, 780);
        }
        else if (pageType == typeof(RestoreContextPage))
        {
            Apply(500, 400, 850, 750, 660, 560);
        }
        else
        {
            window.MinimumWidth = 400;
            window.MinimumHeight = 300;
        }
    }
}
