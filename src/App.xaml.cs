using ADHDWorkspace.Views;
using ADHDWorkspace.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ADHDWorkspace;

public partial class App : Application
{
    private ITrayIconService? _trayIcon;
    private ILogger<App>? _logger;
    private Window? _mainWindow;

    public App()
    {
        try
        {
            File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", $"App() constructor called at {DateTime.Now}\n");
            InitializeComponent();

            // Set dark theme
            UserAppTheme = AppTheme.Dark;
            File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "App initialized successfully\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", $"ERROR in App(): {ex}\n");
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", $"CreateWindow() called at {DateTime.Now}\n");
            
            // Get services from DI
            var services = Handler?.MauiContext?.Services;
            if (services == null)
            {
                File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "ERROR: Services is null\n");
                return new Window(new ContentPage { Content = new Label { Text = "Service initialization failed" } });
            }

            File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "Getting logger...\n");
            _logger = services.GetRequiredService<ILogger<App>>();
            File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "Logger obtained\n");
            
            File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "Getting tray icon service...\n");
            _trayIcon = services.GetRequiredService<ITrayIconService>();
            File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "Tray icon service obtained\n");
            
            // Setup tray icon events
            _trayIcon.OpenMiniTaskbar += OnOpenMiniTaskbar;
            _trayIcon.StartPomodoroRequested += OnStartPomodoroRequested;
            _trayIcon.OpenSettings += OnOpenSettings;
            _trayIcon.ExitRequested += OnExitRequested;
            _trayIcon.Show();
            
            _logger.LogInformation("ADHD Workspace started with tray icon");
            
            // Get MiniTaskbarWindow from DI
            _mainWindow = services.GetRequiredService<MiniTaskbarWindow>();
            return _mainWindow;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating window: {ex}");
            return new Window(new ContentPage { Content = new Label { Text = $"Error: {ex.Message}" } });
        }
    }

    private void OnOpenMiniTaskbar(object? sender, EventArgs e)
    {
        try
        {
            Dispatcher.Dispatch(() =>
            {
                if (_mainWindow != null)
                {
                    // Show the window
                    _mainWindow.Page?.DisplayAlert("Mini Taskbar", "Bringing to front...", "OK");
                    _logger?.LogInformation("Mini taskbar requested from tray");
                }
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error showing mini taskbar");
        }
    }

    private void OnStartPomodoroRequested(object? sender, EventArgs e)
    {
        try
        {
            Dispatcher.Dispatch(() =>
            {
                var services = Handler?.MauiContext?.Services;
                if (services != null)
                {
                    var pomodoroService = services.GetService<PomodoroService>();
                    if (pomodoroService != null)
                    {
                        if (pomodoroService.IsRunning)
                        {
                            pomodoroService.Stop();
                            _logger?.LogInformation("Pomodoro stopped from tray");
                        }
                        else
                        {
                            pomodoroService.Start();
                            _logger?.LogInformation("Pomodoro started from tray");
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error toggling pomodoro from tray");
        }
    }

    private void OnOpenSettings(object? sender, EventArgs e)
    {
        try
        {
            Dispatcher.Dispatch(() =>
            {
                var services = Handler?.MauiContext?.Services;
                if (services != null)
                {
                    var settingsPage = services.GetRequiredService<SettingsPage>();
                    if (_mainWindow != null)
                    {
                        _mainWindow.Page = settingsPage;
                    }
                    _logger?.LogInformation("Settings opened from tray");
                }
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error opening settings");
        }
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        _logger?.LogInformation("Application exit requested from tray");
        _trayIcon?.Dispose();
        Quit();
    }

    protected override void CleanUp()
    {
        try
        {
            var services = Handler?.MauiContext?.Services;
            if (services != null)
            {
                var focusTracker = services.GetService<FocusTrackerService>();
                var focusLogger = services.GetService<FocusLoggerService>();

                if (focusTracker != null && focusLogger != null)
                {
                    // Stop all active tasks silently
                    focusTracker.StopAllTasks(Models.CompletionStatus.InProgress);
                    
                    // Save sessions synchronously (blocking is OK during shutdown)
                    var sessions = focusTracker.GetAllSessions();
                    focusLogger.SaveSessionsAsync(sessions).Wait(TimeSpan.FromSeconds(5));
                    
                    _logger?.LogInformation("Saved focus sessions on application shutdown");
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving focus sessions on shutdown");
        }
        
        _trayIcon?.Dispose();
        base.CleanUp();
    }
}
