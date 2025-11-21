using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using CommunityToolkit.Maui;
using ADHDWorkspace.Services;
using ADHDWorkspace.Views;

namespace ADHDWorkspace;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp(bool verboseMode = false, bool portableMode = false)
    {
        try
        {
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: Creating builder\n");
            var builder = MauiApp.CreateBuilder();
            
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: Configuring MAUI\n");
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FontAwesome-Free-Regular-400.otf", "FontAwesome");
                    fonts.AddFont("FontAwesome-Free-Solid-900.otf", "FontAwesomeSolid");
                });

            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: MAUI configured\n");

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(windows =>
            {
                windows.OnWindowCreated((window) =>
                {
                    // Configure window properties
                    window.ExtendsContentIntoTitleBar = false;
                    
                    // Make window appear in taskbar
                    var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                    
                    if (appWindow != null)
                    {
                        // Show in taskbar
                        appWindow.IsShownInSwitchers = true;
                        
                        // Set title
                        appWindow.Title = "ADHD Workspace";
                    }
                    
                    // Force proper window style for taskbar
                    const int GWL_EXSTYLE = -20;
                    const int GWL_STYLE = -16;
                    const int WS_EX_APPWINDOW = 0x00040000;
                    const int WS_EX_TOOLWINDOW = 0x00000080;
                    const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
                    const int WS_VISIBLE = 0x10000000;
                    
                    // Set as app window (not tool window)
                    var extendedStyle = GetWindowLong(handle, GWL_EXSTYLE);
                    extendedStyle |= WS_EX_APPWINDOW;
                    extendedStyle &= ~WS_EX_TOOLWINDOW;
                    SetWindowLong(handle, GWL_EXSTYLE, extendedStyle);
                    
                    // Set standard window style
                    var style = GetWindowLong(handle, GWL_STYLE);
                    style |= WS_VISIBLE;
                    SetWindowLong(handle, GWL_STYLE, style);
                    
                    // Force window to update and appear
                    ShowWindow(handle, 5); // SW_SHOW
                    SetForegroundWindow(handle);
                });
            });
        });
        
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
#endif

        System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: Registering Configuration Service\n");
        
        // Configuration Service (must be first)
        builder.Services.AddSingleton<IConfigurationService>(sp =>
        {
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "ConfigService factory: Creating\n");
            var configService = new ConfigurationService(
                sp.GetRequiredService<ILogger<ConfigurationService>>());
            
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "ConfigService factory: Loading (sync)\n");
            // Override portable mode if specified - use synchronous Load()
            configService.Load();
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "ConfigService factory: Loaded!\n");
            if (portableMode)
            {
                configService.Settings.PortableMode = true;
            }
            
            return configService;
        });

        System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: Initializing logging\n");
        // Initialize logging - use synchronous load to avoid deadlock during startup
        var tempConfigService = new ConfigurationService(
            LoggerFactory.Create(b => b.AddDebug()).CreateLogger<ConfigurationService>());
        System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: Loading temp config (sync)\n");
        
        // Use synchronous Load() instead of LoadAsync() to avoid deadlock
        tempConfigService.Load();
        System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: Temp config loaded!\n");
        
        if (portableMode)
        {
            tempConfigService.Settings.PortableMode = true;
        }

        System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: Initializing LoggerService\n");
        var loggerFactory = LoggerService.Initialize(tempConfigService.Settings, verboseMode);
        builder.Logging.AddProvider(new MauiLoggerProvider(loggerFactory));
        System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: Logging initialized\n");

        // Infrastructure Services
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IScreenService, ScreenService>();
        builder.Services.AddSingleton<IWindowManagerService, WindowManagerService>();
        builder.Services.AddSingleton<ITrayIconService, TrayIconService>();
        
#if WINDOWS
        builder.Services.AddSingleton<IHotkeyService, WindowsHotkeyService>();
#else
        builder.Services.AddSingleton<IHotkeyService>(sp => 
            throw new PlatformNotSupportedException("Hotkeys only supported on Windows"));
#endif

        // Business Services
        builder.Services.AddSingleton<PomodoroService>();
        builder.Services.AddSingleton<FocusTrackerService>();
        builder.Services.AddSingleton<FocusLoggerService>();
        builder.Services.AddSingleton<ObsidianOverlayService>();
        builder.Services.AddSingleton<ContextService>();
        builder.Services.AddSingleton<WorkspaceSetupService>();

        System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: Registering Views\n");
        // Views/Pages
        builder.Services.AddTransient<MiniTaskbarWindow>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<QuickCapturePage>();
        builder.Services.AddSingleton<FocusTrackerPage>();
        builder.Services.AddSingleton<RestoreContextPage>();

        System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: Building app\n");
        var app = builder.Build();
        System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp: App built successfully!\n");
        return app;
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", $"CreateMauiApp EXCEPTION: {ex.Message}\n{ex.StackTrace}\n");
            throw;
        }
    }
}

/// <summary>
/// Custom logger provider for MAUI integration
/// </summary>
public class MauiLoggerProvider(ILoggerFactory loggerFactory) : ILoggerProvider
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    public ILogger CreateLogger(string categoryName)
    {
        return _loggerFactory.CreateLogger(categoryName);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
