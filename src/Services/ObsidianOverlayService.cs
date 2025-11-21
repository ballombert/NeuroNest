namespace ADHDWorkspace.Services;

using Microsoft.Extensions.Logging;
using ADHDWorkspace.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// Service for managing Obsidian overlay (opacity, positioning, always-on-top)
/// </summary>
public class ObsidianOverlayService(
    ILogger<ObsidianOverlayService> logger,
    IConfigurationService configService)
{
    private readonly ILogger<ObsidianOverlayService> _logger = logger;
    private readonly IConfigurationService _configService = configService;
    
    private CancellationTokenSource? _cts;
    private Task? _runningTask;
    private bool _isRunning = false;

#if WINDOWS
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
    private const uint LWA_ALPHA = 0x2;
#endif

    public bool IsRunning => _isRunning;

    /// <summary>
    /// Starts monitoring and managing Obsidian overlay
    /// </summary>
    public void Start()
    {
        if (_isRunning)
        {
            _logger.LogWarning("Obsidian overlay service already running");
            return;
        }

        _isRunning = true;
        _cts = new CancellationTokenSource();
        _runningTask = Task.Run(() => RunOverlayManagement(_cts.Token), _cts.Token);
        
        _logger.LogInformation("Obsidian overlay service started");
    }

    /// <summary>
    /// Stops the overlay management
    /// </summary>
    public void Stop()
    {
        if (!_isRunning) return;

        _cts?.Cancel();
        _runningTask?.Wait(TimeSpan.FromSeconds(5));
        
        _isRunning = false;
        _logger.LogInformation("Obsidian overlay service stopped");
    }

    private async Task RunOverlayManagement(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var obsidianProcess = Process.GetProcessesByName("Obsidian").FirstOrDefault();
                
                if (obsidianProcess != null && obsidianProcess.MainWindowHandle != IntPtr.Zero)
                {
                    ApplyOverlaySettings(obsidianProcess.MainWindowHandle);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Overlay management cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in overlay management loop");
        }
    }

    private void ApplyOverlaySettings(IntPtr hwnd)
    {
#if WINDOWS
        try
        {
            var settings = _configService.Settings.UI;
            
            // Determine opacity based on Pomodoro state
            int opacity = DetermineOpacity(settings);

            // Make window layered
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);

            // Set opacity
            byte alpha = (byte)(opacity * 255 / 100);
            SetLayeredWindowAttributes(hwnd, 0, alpha, LWA_ALPHA);

            // Set always on top
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

            _logger.LogDebug("Applied overlay settings: Opacity={Opacity}%", opacity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply overlay settings");
        }
#endif
    }

    private int DetermineOpacity(UISettings settings)
    {
        try
        {
            var pomodoroStatePath = _configService.ResolvePath(_configService.Settings.Paths.PomodoroStateFile);
            
            if (File.Exists(pomodoroStatePath))
            {
                var state = File.ReadAllText(pomodoroStatePath);
                
                if (state.Contains("mode:focus"))
                {
                    return settings.OpacityPomodoro; // More transparent during focus
                }
                else if (state.Contains("mode:shortbreak") || state.Contains("mode:longbreak"))
                {
                    return settings.OpacityActive; // Fully visible during breaks
                }
            }

            return settings.OpacityBackground; // Default background opacity
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to read Pomodoro state");
            return settings.OpacityBackground;
        }
    }
}
