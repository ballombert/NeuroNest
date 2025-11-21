using Microsoft.Maui.Controls;
using Microsoft.Extensions.Logging;
using ADHDWorkspace.Helpers;
using ADHDWorkspace.Services;
using ADHDWorkspace.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ADHDWorkspace.Views;

public partial class MiniTaskbarWindow : Window
{
    private readonly ILogger<MiniTaskbarWindow> _logger;
    private readonly IConfigurationService _configService;
    private readonly IScreenService _screenService;
    private readonly IHotkeyService _hotkeyService;
    private readonly INotificationService _notificationService;
    private readonly PomodoroService _pomodoroService;
    private readonly FocusTrackerService _focusTrackerService;
    private readonly ObsidianOverlayService _obsidianOverlayService;
    private readonly ContextService _contextService;
    private readonly IWindowManagerService _windowManagerService;

    private bool _isExpanded = false;
    private IDispatcherTimer? _updateTimer;
    private ContentPage? _mainPage;
    private double _dragStartX = 0;
    private double _dragStartY = 0;
    private bool _isApplyingFocusModeToggle = false;

    public MiniTaskbarWindow(
        ILogger<MiniTaskbarWindow> logger,
        IConfigurationService configService,
        IScreenService screenService,
        IHotkeyService hotkeyService,
        INotificationService notificationService,
        PomodoroService pomodoroService,
        FocusTrackerService focusTrackerService,
        ObsidianOverlayService obsidianOverlayService,
        ContextService contextService,
        IWindowManagerService windowManagerService)
    {
        InitializeComponent();
        
        // Get reference to the ContentPage
        _mainPage = Page as ContentPage;

        _logger = logger;
        _configService = configService;
        _screenService = screenService;
        _hotkeyService = hotkeyService;
        _notificationService = notificationService;
        _pomodoroService = pomodoroService;
        _focusTrackerService = focusTrackerService;
        _obsidianOverlayService = obsidianOverlayService;
        _contextService = contextService;
        _windowManagerService = windowManagerService;

        HandlerChanged += OnHandlerChanged;
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        if (Handler != null)
        {
            OnWindowLoaded();
        }
    }

    private void OnWindowLoaded()
    {
        RemoveTitleBar();
        PositionWindow();
        RegisterHotkeys();
        StartBackgroundServices();
        StartUpdateTimer();
        SetupHoverStates();
        InitializeFocusModeToggle();

        _logger.LogInformation("MiniTaskbar window loaded and services started");
        _notificationService.ShowSuccess("ADHD Workspace", "All systems ready!");
    }

    private void RemoveTitleBar()
    {
        try
        {
            var hwnd = GetWindowHandle();
            if (hwnd == IntPtr.Zero)
                return;

            // Get current window style
            int style = WindowsApi.GetWindowLong(hwnd, WindowsApi.GWL_STYLE);
            
            // Remove title bar, borders, and system menu
            style &= ~(WindowsApi.WS_CAPTION | WindowsApi.WS_THICKFRAME | WindowsApi.WS_SYSMENU);
            
            // Apply new style
            WindowsApi.SetWindowLong(hwnd, WindowsApi.GWL_STYLE, style);
            
            // Force window to redraw with new style
            WindowsApi.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                WindowsApi.SWP_NOMOVE | WindowsApi.SWP_NOSIZE | WindowsApi.SWP_FRAMECHANGED | WindowsApi.SWP_SHOWWINDOW);

            _logger.LogDebug("Title bar removed from Mini Taskbar window");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove title bar");
        }
    }

    private void PositionWindow()
    {
        try
        {
            var screen = _screenService.GetRightmostScreenWorkingArea();
            var settings = _configService.Settings.UI;

            // Position at top-right of rightmost screen
            X = screen.Width - settings.TaskbarCollapsedWidth;
            Y = 0;

            _logger.LogDebug("Window positioned at X={X}, Y={Y}", X, Y);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to position window");
        }
    }

    private void RegisterHotkeys()
    {
        try
        {
            var hotkeys = _configService.Settings.Hotkeys;

            _hotkeyService.RegisterHotkey(hotkeys.QuickCapture, () =>
            {
                Dispatcher.Dispatch(async () =>
                {
                    // Open Quick Capture window via WindowManagerService
                    await _windowManagerService.OpenWindowAsync<QuickCapturePage>();
                });
            });

            _hotkeyService.RegisterHotkey(hotkeys.StartPomodoro, () =>
            {
                Dispatcher.Dispatch(() => OnPomodoroTapped(this, EventArgs.Empty));
            });

            _hotkeyService.RegisterHotkey(hotkeys.RestoreContext, () =>
            {
                Dispatcher.Dispatch(() => OnRestoreContextClicked(this, EventArgs.Empty));
            });

            _hotkeyService.RegisterHotkey(hotkeys.ShowSettings, () =>
            {
                Dispatcher.Dispatch(() => OnSettingsTapped(this, EventArgs.Empty));
            });

            _logger.LogInformation("Hotkeys registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register hotkeys");
        }
    }

    private void StartBackgroundServices()
    {
        try
        {
            // Start Obsidian overlay management
            _obsidianOverlayService.Start();
            _logger.LogInformation("Obsidian overlay service started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start background services");
        }
    }

    private void StartUpdateTimer()
    {
        _updateTimer = Dispatcher.CreateTimer();
        _updateTimer.Interval = TimeSpan.FromSeconds(1);
        _updateTimer.Tick += (s, e) =>
        {
            UpdateClock();
            UpdatePomodoroDisplay();
            UpdateFocusDisplay();
        };
        _updateTimer.Start();

        _logger.LogDebug("Update timer started");
    }

    private void UpdateClock()
    {
        ClockLabel.Text = DateTime.Now.ToString("HH:mm");
    }

    private void UpdatePomodoroDisplay()
    {
        try
        {
            ResetPomodoroIndicator();
            var statePath = _configService.ResolvePath(_configService.Settings.Paths.PomodoroStateFile);

            if (!File.Exists(statePath))
            {
                return;
            }

            var state = File.ReadAllText(statePath);
            var lines = state.Split('\n');
            var mode = lines.FirstOrDefault(l => l.StartsWith("mode:"))?.Split(':')[1].Trim() ?? string.Empty;
            var remaining = lines.FirstOrDefault(l => l.StartsWith("remaining:"))?.Split(':')[1].Trim() ?? "0";

            switch (mode)
            {
                case "focus":
                    if (PomodoroIconSource != null)
                    {
                        IconGlyphs.PomodoroFocus.ApplyTo(PomodoroIconSource, Color.FromArgb("#FF5A5F"));
                    }
                    ToolTipProperties.SetText(PomodoroLabel, $"Pomodoro: {remaining} min");

                    var progress = CalculateProgress(remaining, _configService.Settings.Pomodoro.FocusMinutes);
                    PomodoroProgressBar.Progress = progress;

                    if (progress < 0.6)
                        PomodoroProgressBar.ProgressColor = Color.FromArgb("#4CAF50");
                    else if (progress < 0.9)
                        PomodoroProgressBar.ProgressColor = Color.FromArgb("#FF9800");
                    else
                        PomodoroProgressBar.ProgressColor = Color.FromArgb("#F44336");
                    
                    // Show time in progress bar overlay
                    if (PomodoroTimeLabel != null)
                    {
                        PomodoroTimeLabel.Text = $"{remaining} min";
                        PomodoroTimeLabel.IsVisible = true;
                    }
                    break;
                case "shortbreak":
                case "longbreak":
                    if (PomodoroIconSource != null)
                    {
                        IconGlyphs.PomodoroBreak.ApplyTo(PomodoroIconSource, Color.FromArgb("#FFD700"));
                    }
                    ToolTipProperties.SetText(PomodoroLabel, $"Break: {remaining} min");

                    var breakMinutes = mode == "shortbreak"
                        ? _configService.Settings.Pomodoro.ShortBreakMinutes
                        : _configService.Settings.Pomodoro.LongBreakMinutes;
                    var breakProgress = CalculateProgress(remaining, breakMinutes);
                    PomodoroProgressBar.Progress = breakProgress;
                    PomodoroProgressBar.ProgressColor = Color.FromArgb("#2196F3");
                    
                    // Show time in progress bar overlay
                    if (PomodoroTimeLabel != null)
                    {
                        PomodoroTimeLabel.Text = $"{remaining} min";
                        PomodoroTimeLabel.IsVisible = true;
                    }
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to update Pomodoro display");
        }
    }

    private void ResetPomodoroIndicator()
    {
        if (PomodoroIconSource != null)
        {
            IconGlyphs.PomodoroIdle.ApplyTo(PomodoroIconSource, Color.FromArgb("#3F3F41"));
        }
        ToolTipProperties.SetText(PomodoroLabel, "Pomodoro");

        PomodoroProgressBar.Progress = 0;
        PomodoroProgressBar.ProgressColor = Color.FromArgb("#4CAF50");
        
        // Hide time overlay
        if (PomodoroTimeLabel != null)
        {
            PomodoroTimeLabel.IsVisible = false;
        }
    }

    private void UpdateFocusDisplay()
    {
        try
        {
            ResetFocusIndicator();
            var statePath = _configService.ResolvePath(_configService.Settings.Paths.FocusStateFile);

            if (!File.Exists(statePath))
            {
                return;
            }

            var state = File.ReadAllText(statePath);
            var lines = state.Split('\n');
            var task = lines.FirstOrDefault(l => l.StartsWith("task:"))?.Split(':')[1].Trim() ?? string.Empty;
            var reminders = lines.FirstOrDefault(l => l.StartsWith("reminders:"))?.Split(':')[1].Trim() ?? "0";
            var active = lines.FirstOrDefault(l => l.StartsWith("active:"))?.Split(':')[1].Trim() ?? "false";

            if (!string.Equals(active, "true", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int reminderCount = int.TryParse(reminders, out int parsedReminders) ? parsedReminders : 0;
            
            if (FocusIconSource != null)
            {
                IconGlyphs.FocusActive.ApplyTo(FocusIconSource, Color.FromArgb("#FF4500"));
            }
            
            string tooltipText = reminderCount > 0 
                ? $"Focus: {task} ({reminderCount} reminders)" 
                : $"Focus: {task}";
            ToolTipProperties.SetText(FocusLabel, tooltipText);

            UpdateFocusTaskDetails(task, reminderCount, Color.FromArgb("#FF4500"));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to update focus display");
        }
    }

    private void ResetFocusIndicator()
    {
        if (FocusIconSource != null)
        {
            IconGlyphs.FocusIdle.ApplyTo(FocusIconSource, Color.FromArgb("#64B5F6"));
        }
        ToolTipProperties.SetText(FocusLabel, "Focus Tracker");

        if (FocusTaskLabel != null)
        {
            FocusTaskLabel.FormattedText = null;
            FocusTaskLabel.Text = string.Empty;
        }
    }

    private void UpdateFocusTaskDetails(string task, int reminderCount, Color accentColor)
    {
        if (FocusTaskLabel == null)
        {
            return;
        }

        FocusTaskLabel.Text = string.Empty;
        var textColor = FocusTaskLabel.TextColor;
        var formatted = new FormattedString();

        formatted.Spans.Add(IconGlyphs.FocusActive.ToSpan(FocusTaskLabel.FontSize, accentColor));
        formatted.Spans.Add(new Span
        {
            Text = $" Focus: {task}",
            TextColor = textColor,
            FontSize = FocusTaskLabel.FontSize
        });

        formatted.Spans.Add(new Span
        {
            Text = " (",
            TextColor = textColor,
            FontSize = FocusTaskLabel.FontSize
        });
        formatted.Spans.Add(IconGlyphs.Warning.ToSpan(FocusTaskLabel.FontSize, Color.FromArgb("#FFD700")));
        formatted.Spans.Add(new Span
        {
            Text = $" {reminderCount} reminders)",
            TextColor = textColor,
            FontSize = FocusTaskLabel.FontSize
        });

        FocusTaskLabel.FormattedText = formatted;
    }

    private double CalculateProgress(string remainingStr, int totalMinutes)
    {
        if (int.TryParse(remainingStr, out int remaining))
        {
            return 1.0 - ((double)remaining / totalMinutes);
        }
        return 0;
    }

    private async void OnClockTapped(object? sender, EventArgs e)
    {
        await ToggleExpandAsync();
    }

    private async Task ToggleExpandAsync()
    {
        _isExpanded = !_isExpanded;
        var settings = _configService.Settings.UI;
        var screen = _screenService.GetRightmostScreenWorkingArea();

        if (_isExpanded)
        {
            // Expand - maintenir l'alignement à droite
            var oldWidth = Width;
            Width = settings.TaskbarExpandedWidth;
            Height = settings.TaskbarExpandedHeight;
            
            // Ajuster X pour maintenir le bord droit au même endroit
            try
            {
                var hwnd = GetWindowHandle();
                if (hwnd != IntPtr.Zero && WindowsApi.GetWindowRect(hwnd, out var rect))
                {
                    int newX = rect.Right - (int)settings.TaskbarExpandedWidth;
                    WindowsApi.SetWindowPos(hwnd, IntPtr.Zero, newX, rect.Top, 0, 0,
                        WindowsApi.SWP_NOSIZE | WindowsApi.SWP_SHOWWINDOW);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reposition window on expand");
            }
            
                ExpandedContent.Opacity = 1;
                ExpandedContent.IsVisible = true;
        }
        else
        {
            // Collapse - maintenir l'alignement à droite
                ExpandedContent.Opacity = 0;
            ExpandedContent.IsVisible = false;
            
            try
            {
                var hwnd = GetWindowHandle();
                if (hwnd != IntPtr.Zero && WindowsApi.GetWindowRect(hwnd, out var rect))
                {
                    int newX = rect.Right - (int)settings.TaskbarCollapsedWidth;
                    WindowsApi.SetWindowPos(hwnd, IntPtr.Zero, newX, rect.Top, 0, 0,
                        WindowsApi.SWP_NOSIZE | WindowsApi.SWP_SHOWWINDOW);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reposition window on collapse");
            }
            
            Width = settings.TaskbarCollapsedWidth;
            Height = settings.TaskbarCollapsedHeight;
        }

        _logger.LogDebug("Taskbar {State} - Width: {Width}, Height: {Height}, ExpandedVisible: {Visible}", 
            _isExpanded ? "expanded" : "collapsed", Width, Height, ExpandedContent.IsVisible);
    }

    private void OnPomodoroTapped(object? sender, EventArgs e)
    {
        if (_pomodoroService.IsRunning)
        {
            _pomodoroService.Stop();
            _notificationService.ShowToast("Pomodoro Stopped", "Timer has been stopped.");
        }
        else
        {
            _pomodoroService.Start();
            _notificationService.ShowSuccess("Pomodoro Started", "Focus session begins now!");
        }
    }

    private async void OnFocusTapped(object? sender, EventArgs e)
    {
        try
        {
            await _windowManagerService.OpenWindowAsync<FocusTrackerPage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open focus tracker");
            _notificationService.ShowError("Error", "Failed to open focus tracker");
        }
    }

    private async void OnSettingsTapped(object? sender, EventArgs e)
    {
        try
        {
            await _windowManagerService.OpenWindowAsync<SettingsPage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open settings");
            _notificationService.ShowError("Error", "Failed to open settings");
        }
    }

    private void OnCloseTapped(object? sender, EventArgs e)
    {
        // Minimize to tray instead of closing
        // For now, just hide the window
        Application.Current?.CloseWindow(this);
    }

    private void OnDragBarPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (sender is not View dragBar)
            return;

        try
        {
            var hwnd = GetWindowHandle();
            if (hwnd == IntPtr.Zero)
                return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    if (WindowsApi.GetWindowRect(hwnd, out var startRect))
                    {
                        _dragStartX = startRect.Left;
                        _dragStartY = startRect.Top;
                    }
                    break;

                case GestureStatus.Running:
                    int newX = (int)(_dragStartX + e.TotalX);
                    int newY = (int)(_dragStartY + e.TotalY);
                    WindowsApi.SetWindowPos(hwnd, IntPtr.Zero, newX, newY, 0, 0, 
                        WindowsApi.SWP_NOSIZE | WindowsApi.SWP_SHOWWINDOW);
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    // Position is already set
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during window drag");
        }
    }

    private IntPtr GetWindowHandle()
    {
#if WINDOWS
        if (Handler?.PlatformView is Microsoft.UI.Xaml.Window window)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            return hwnd;
        }
#endif
        return IntPtr.Zero;
    }

    // App launcher methods
    private void OnVSCodeClicked(object? sender, EventArgs e) => LaunchApp("code", _configService.Settings.Paths.ProjectPath);
    private void OnTeamsClicked(object? sender, EventArgs e) => LaunchApp("msedge.exe", "--app=https://teams.microsoft.com");
    private void OnOutlookClicked(object? sender, EventArgs e) => LaunchApp("msedge.exe", "--app=https://outlook.office.com");
    private void OnObsidianClicked(object? sender, EventArgs e) => LaunchApp("obsidian", "obsidian://open?vault=adhd");
    private void OnTerminalClicked(object? sender, EventArgs e) => LaunchApp("wt", "");
    private void OnEdgeClicked(object? sender, EventArgs e) => LaunchApp("msedge", "");

    private async void OnSaveContextClicked(object? sender, EventArgs e)
    {
        try
        {
            string? name = await _mainPage!.DisplayPromptAsync(
                "Save Context",
                "Enter a name for this context (optional):",
                "Save",
                "Cancel",
                maxLength: 50);

            if (name != null) // User didn't cancel
            {
                var filePath = await _contextService.SaveContextAsync(name);
                _notificationService.ShowSuccess("Context Saved", $"Saved to {Path.GetFileName(filePath)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save context");
            _notificationService.ShowError("Save Failed", ex.Message);
        }
    }

    private async void OnRestoreContextClicked(object? sender, EventArgs e)
    {
        try
        {
            await _windowManagerService.OpenWindowAsync<RestoreContextPage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open restore context");
            _notificationService.ShowError("Error", "Failed to open restore context");
        }
    }

    private void InitializeFocusModeToggle()
    {
        if (FocusModeSwitch == null)
        {
            return;
        }

        _isApplyingFocusModeToggle = true;
        bool isEnabled = _configService.Settings.UI.FocusModeEnabled;
        FocusModeSwitch.IsToggled = isEnabled;
        ApplyFocusModeVisibility(isEnabled);
        _isApplyingFocusModeToggle = false;
    }

    private void ApplyFocusModeVisibility(bool isEnabled)
    {
        if (AppButtonsScrollView != null)
        {
            AppButtonsScrollView.IsVisible = !isEnabled;
        }
    }

    private async void OnFocusModeToggled(object? sender, ToggledEventArgs e)
    {
        if (_isApplyingFocusModeToggle)
        {
            return;
        }

        ApplyFocusModeVisibility(e.Value);
        _configService.Settings.UI.FocusModeEnabled = e.Value;

        try
        {
            await _configService.SaveAsync();
            var message = e.Value
                ? "Focus Mode enabled. App buttons hidden."
                : "Focus Mode disabled. App buttons restored.";
            _notificationService.ShowToast("Focus Mode", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save focus mode preference");
            _notificationService.ShowError("Focus Mode", "Unable to save preference.");
        }
    }

    private void LaunchApp(string command, string? args = null)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args ?? "",
                UseShellExecute = true
            };

            Process.Start(psi);
            _logger.LogInformation("Launched app: {Command}", command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch app: {Command}", command);
            _notificationService.ShowError("Launch Failed", $"Failed to launch {command}");
        }
    }

    private void SetupHoverStates()
    {
        // Note: MAUI Labels don't have direct hover events like WPF/WinForms
        // This method sets up the foundation for future hover interaction
        // In a future phase, we can add PointerEntered/PointerExited events
        // to create dynamic hover effects for accessibility
        _logger.LogDebug("Hover state handlers configured for indicators");
    }

    public void Cleanup()
    {
        _updateTimer?.Stop();
        _hotkeyService.UnregisterAll();
        _pomodoroService.Stop();
        _focusTrackerService.Stop();
        _obsidianOverlayService.Stop();
        
        _logger.LogInformation("MiniTaskbar window closing, services stopped");
    }
}
