using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Windows.Forms;

namespace ADHDWorkspace.Services;

/// <summary>
/// Service for managing system tray icon
/// </summary>
public interface ITrayIconService : IDisposable
{
    void Show();
    void Hide();
    void ShowNotification(string title, string message);
    event EventHandler? OpenMiniTaskbar;
    event EventHandler? StartPomodoroRequested;
    event EventHandler? OpenSettings;
    event EventHandler? ExitRequested;
}

public class TrayIconService : ITrayIconService
{
    private readonly ILogger<TrayIconService> _logger;
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private System.Windows.Forms.ContextMenuStrip? _contextMenu;
    private bool _disposed;
    private Thread? _trayThread;
    private readonly ManualResetEvent _trayReadyEvent = new(false);

    public event EventHandler? OpenMiniTaskbar;
    public event EventHandler? StartPomodoroRequested;
    public event EventHandler? OpenSettings;
    public event EventHandler? ExitRequested;

    public TrayIconService(ILogger<TrayIconService> logger)
    {
        _logger = logger;
        
        try
        {
            Console.WriteLine("[TrayIcon] Constructor starting");
            
            // Start tray icon on dedicated STA thread
            _trayThread = new Thread(TrayThreadProc)
            {
                IsBackground = true,
                Name = "TrayIconThread"
            };
            _trayThread.SetApartmentState(ApartmentState.STA);
            _trayThread.Start();
            
            Console.WriteLine("[TrayIcon] Thread started, waiting for ready event...");
            // Wait for tray to initialize (with timeout)
            if (_trayReadyEvent.WaitOne(TimeSpan.FromSeconds(3)))
            {
                Console.WriteLine("[TrayIcon] Tray icon ready");
            }
            else
            {
                Console.WriteLine("[TrayIcon] WARNING: Tray icon initialization timeout");
            }
            
            _logger.LogInformation("TrayIconService constructor completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayIcon] Constructor ERROR: {ex.Message}");
            _logger.LogError(ex, "Failed to create TrayIconService");
        }
    }
    
    private void TrayThreadProc()
    {
        try
        {
            Console.WriteLine("[TrayIcon] Thread starting...");
            _logger.LogInformation("Tray thread starting...");
            System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Console.WriteLine("[TrayIcon] Initializing tray icon...");
            InitializeTrayIcon();
            _trayReadyEvent.Set();
            Console.WriteLine("[TrayIcon] Icon initialized, running message loop");
            _logger.LogInformation("Running WinForms message loop");
            System.Windows.Forms.Application.Run();
            Console.WriteLine("[TrayIcon] Message loop exited");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayIcon] ERROR: {ex.Message}");
            _logger.LogError(ex, "Tray thread failed");
        }
    }

    private void InitializeTrayIcon()
    {
        try
        {
            Console.WriteLine("[TrayIcon] Creating context menu...");
            // Create context menu
            _contextMenu = new System.Windows.Forms.ContextMenuStrip();
            _contextMenu.Items.Add("Start Pomodoro", null, (s, e) => StartPomodoroRequested?.Invoke(this, EventArgs.Empty));
            _contextMenu.Items.Add("Settings", null, (s, e) => OpenSettings?.Invoke(this, EventArgs.Empty));
            _contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            _contextMenu.Items.Add("Quit", null, (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty));

            Console.WriteLine("[TrayIcon] Getting icon...");
            var icon = GetApplicationIcon();
            Console.WriteLine($"[TrayIcon] Icon created: {icon != null}");

            // Create notify icon
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = icon,
                Text = "ADHD Workspace",
                ContextMenuStrip = _contextMenu,
                Visible = false
            };

            Console.WriteLine("[TrayIcon] NotifyIcon created, making visible...");
            _notifyIcon.Visible = true;
            Console.WriteLine("[TrayIcon] Icon should now be visible!");

            // Double-click opens mini taskbar
            _notifyIcon.DoubleClick += (s, e) => OpenMiniTaskbar?.Invoke(this, EventArgs.Empty);

            _logger.LogInformation("Tray icon initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayIcon] Init ERROR: {ex.Message}\n{ex.StackTrace}");
            _logger.LogError(ex, "Failed to initialize tray icon");
        }
    }

    private Icon GetApplicationIcon()
    {
        try
        {
            // Try to load tray icon from Resources
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Images", "trayicon.ico");
            if (File.Exists(iconPath))
            {
                _logger.LogInformation("Loading tray icon from: {IconPath}", iconPath);
                return new Icon(iconPath);
            }
            
            _logger.LogWarning("Tray icon not found at: {IconPath}", iconPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load custom icon");
        }

        // Create a simple icon programmatically as fallback
        return CreateDefaultIcon();
    }

    private Icon CreateDefaultIcon()
    {
        try
        {
            // Create a 32x32 bitmap with a colored square
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                // Red background for focus
                g.Clear(System.Drawing.Color.FromArgb(220, 38, 38)); // red-600
                
                // White "A" for ADHD
                using (var font = new System.Drawing.Font("Segoe UI", 20, System.Drawing.FontStyle.Bold))
                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
                {
                    var format = new System.Drawing.StringFormat
                    {
                        Alignment = System.Drawing.StringAlignment.Center,
                        LineAlignment = System.Drawing.StringAlignment.Center
                    };
                    g.DrawString("A", font, brush, new System.Drawing.RectangleF(0, 0, 32, 32), format);
                }
            }

            var hIcon = bitmap.GetHicon();
            var icon = Icon.FromHandle(hIcon);
            
            _logger.LogInformation("Default tray icon created");
            return icon;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create default icon");
            // Return system application icon as last resort
            return SystemIcons.Application;
        }
    }

    public void Show()
    {
        Console.WriteLine($"[TrayIcon] Show() called, icon exists: {_notifyIcon != null}");
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = true;
            Console.WriteLine("[TrayIcon] Icon set to visible");
            _logger.LogInformation("Tray icon shown");
        }
    }

    public void Hide()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _logger.LogInformation("Tray icon hidden");
        }
    }

    public void ShowNotification(string title, string message)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.ShowBalloonTip(3000, title, message, System.Windows.Forms.ToolTipIcon.Info);
            _logger.LogInformation("Notification shown: {Title}", title);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger.LogInformation("Disposing tray icon...");
            
            // Exit the message loop
            if (_trayThread != null && _trayThread.IsAlive)
            {
                System.Windows.Forms.Application.ExitThread();
            }
            
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
            _trayReadyEvent?.Dispose();
            _disposed = true;
            
            _logger.LogInformation("Tray icon disposed");
        }
        GC.SuppressFinalize(this);
    }
}
