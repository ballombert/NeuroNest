using System.Collections.Generic;
using ADHDWorkspace.Helpers;
using ADHDWorkspace.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace ADHDWorkspace.Views;

public partial class SettingsPage : ContentPage
{
    private readonly ILogger<SettingsPage> _logger;
    private readonly IConfigurationService _configService;
    private readonly INotificationService _notificationService;
    private readonly IHotkeyService _hotkeyService;
    private readonly IWindowManagerService _windowManagerService;
    private bool _isDirty = false;
    private readonly Dictionary<string, (Button Header, Border Section)> _sections = new();

    public SettingsPage(
        ILogger<SettingsPage> logger,
        IConfigurationService configService,
        INotificationService notificationService,
        IHotkeyService hotkeyService,
        IWindowManagerService windowManagerService)
    {
        InitializeComponent();

        _logger = logger;
        _configService = configService;
        _notificationService = notificationService;
        _hotkeyService = hotkeyService;
        _windowManagerService = windowManagerService;

        Loaded += OnPageLoaded;

        _sections["GeneralSection"] = (GeneralHeaderButton, GeneralSection);
        _sections["PomodoroSection"] = (PomodoroHeaderButton, PomodoroSection);
        _sections["PathsSection"] = (PathsHeaderButton, PathsSection);
        _sections["HotkeysSection"] = (HotkeysHeaderButton, HotkeysSection);
    }

    private void OnPageLoaded(object? sender, EventArgs e)
    {
        SetActiveSection("GeneralSection");
        LoadSettings();
        TrackChanges();
    }

    private void OnToggleSection(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string sectionKey)
        {
            return;
        }

        if (!_sections.TryGetValue(sectionKey, out var sectionPair))
        {
            return;
        }

        if (sectionPair.Section.IsVisible)
        {
            return; // already active
        }

        SetActiveSection(sectionKey);
    }

    private void SetActiveSection(string sectionKey)
    {
        foreach (var entry in _sections)
        {
            bool isActive = entry.Key == sectionKey;
            entry.Value.Section.IsVisible = isActive;
            entry.Value.Header.BackgroundColor = isActive
                ? GetColor("AccentCyan")
                : GetColor("SurfaceDarkAlt");
            entry.Value.Header.TextColor = isActive ? Colors.Black : GetColor("PrimaryText");
        }
    }

    private void LoadSettings()
    {
        try
        {
            var settings = _configService.Settings;

            // General
            PortableModeSwitch.IsToggled = settings.PortableMode;
            LogLevelPicker.SelectedItem = settings.Logging.MinimumLevel;

            // Pomodoro
            FocusMinutesEntry.Text = settings.Pomodoro.FocusMinutes.ToString();
            ShortBreakEntry.Text = settings.Pomodoro.ShortBreakMinutes.ToString();
            LongBreakEntry.Text = settings.Pomodoro.LongBreakMinutes.ToString();

            // Paths
            InboxPathEntry.Text = settings.Paths.InboxPath;
            DailyNotesPathEntry.Text = settings.Paths.DailyNotesFolder;
            TemplatesPathEntry.Text = settings.Paths.TemplatesFolder;

            // Hotkeys
            QuickCaptureHotkeyEntry.Text = settings.Hotkeys.QuickCapture;
            StartPomodoroHotkeyEntry.Text = settings.Hotkeys.StartPomodoro;
            RestoreContextHotkeyEntry.Text = settings.Hotkeys.RestoreContext;
            ShowSettingsHotkeyEntry.Text = settings.Hotkeys.ShowSettings;

            _logger.LogInformation("Settings loaded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
            DisplayAlert("Error", "Failed to load settings", "OK");
        }
    }

    private void TrackChanges()
    {
        // Track changes to mark form as dirty
        PortableModeSwitch.Toggled += MarkDirty;
        LogLevelPicker.SelectedIndexChanged += MarkDirty;
        FocusMinutesEntry.TextChanged += MarkDirty;
        ShortBreakEntry.TextChanged += MarkDirty;
        LongBreakEntry.TextChanged += MarkDirty;
        InboxPathEntry.TextChanged += MarkDirty;
        DailyNotesPathEntry.TextChanged += MarkDirty;
        TemplatesPathEntry.TextChanged += MarkDirty;
        QuickCaptureHotkeyEntry.TextChanged += MarkDirty;
        StartPomodoroHotkeyEntry.TextChanged += MarkDirty;
        RestoreContextHotkeyEntry.TextChanged += MarkDirty;
        ShowSettingsHotkeyEntry.TextChanged += MarkDirty;
    }

    private void MarkDirty(object? sender, EventArgs e)
    {
        _isDirty = true;
    }

    private void OnPortableModeToggled(object? sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            _notificationService.ShowToast(
                "Portable Mode",
                "Application will use local directories for data storage.");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            var settings = _configService.Settings;

            // Validate and save General
            settings.PortableMode = PortableModeSwitch.IsToggled;
            settings.Logging.MinimumLevel = LogLevelPicker.SelectedItem?.ToString() ?? "Information";

            // Validate and save Pomodoro
            if (int.TryParse(FocusMinutesEntry.Text, out int focusMin) && focusMin > 0)
                settings.Pomodoro.FocusMinutes = focusMin;
            else
            {
                await DisplayAlert("Invalid Input", "Focus minutes must be a positive number", "OK");
                return;
            }

            if (int.TryParse(ShortBreakEntry.Text, out int shortBreak) && shortBreak > 0)
                settings.Pomodoro.ShortBreakMinutes = shortBreak;
            else
            {
                await DisplayAlert("Invalid Input", "Short break minutes must be a positive number", "OK");
                return;
            }

            if (int.TryParse(LongBreakEntry.Text, out int longBreak) && longBreak > 0)
                settings.Pomodoro.LongBreakMinutes = longBreak;
            else
            {
                await DisplayAlert("Invalid Input", "Long break minutes must be a positive number", "OK");
                return;
            }

            // Validate and save Paths
            if (!string.IsNullOrWhiteSpace(InboxPathEntry.Text))
                settings.Paths.InboxPath = InboxPathEntry.Text;
            if (!string.IsNullOrWhiteSpace(DailyNotesPathEntry.Text))
                settings.Paths.DailyNotesFolder = DailyNotesPathEntry.Text;
            if (!string.IsNullOrWhiteSpace(TemplatesPathEntry.Text))
                settings.Paths.TemplatesFolder = TemplatesPathEntry.Text;

            // Validate and save Hotkeys
            settings.Hotkeys.QuickCapture = QuickCaptureHotkeyEntry.Text ?? "Win+Shift+N";
            settings.Hotkeys.StartPomodoro = StartPomodoroHotkeyEntry.Text ?? "Win+Alt+F";
            settings.Hotkeys.RestoreContext = RestoreContextHotkeyEntry.Text ?? "Win+Alt+R";
            settings.Hotkeys.ShowSettings = ShowSettingsHotkeyEntry.Text ?? "Win+Alt+S";

            // Test hotkeys registration
            bool hotkeysFailed = false;
            var testHotkeys = new[]
            {
                settings.Hotkeys.QuickCapture,
                settings.Hotkeys.StartPomodoro,
                settings.Hotkeys.RestoreContext,
                settings.Hotkeys.ShowSettings
            };

            foreach (var hotkey in testHotkeys)
            {
                if (!_hotkeyService.RegisterHotkey(hotkey, () => { }))
                {
                    hotkeysFailed = true;
                    break;
                }
            }

            if (hotkeysFailed)
            {
                // Toast notification already shown by HotkeyService
                return;
            }

            // Save to file
            await _configService.SaveAsync();

            _isDirty = false;
            _notificationService.ShowSuccess("Settings Saved", "Your settings have been saved successfully.");

            if (sender is Button saveButton)
            {
                await FeedbackHelper.ShowConfirmationAsync(saveButton, "✓ Saved!", GetColor("AccentGreen"));
            }

            _logger.LogInformation("Settings saved successfully");

            await Navigation.PopAsync();
            Application.Current?.CloseWindow(Application.Current.Windows.First(w => w.Page == this));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            await DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        if (_isDirty)
        {
            bool discard = await DisplayAlert(
                "Unsaved Changes",
                "You have unsaved changes. Discard them?",
                "Discard",
                "Keep Editing");

            if (!discard) return;
        }

        // Find and close the window containing this page
        var window = Application.Current?.Windows.FirstOrDefault(w => w.Page == this);
        if (window != null)
        {
            Application.Current?.CloseWindow(window);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (_isDirty)
        {
            Dispatcher.Dispatch(async () =>
            {
                bool discard = await DisplayAlert(
                    "Unsaved Changes",
                    "You have unsaved changes. Discard them?",
                    "Discard",
                    "Keep Editing");

                if (discard)
                {
                    await Navigation.PopAsync();
                }
            });

            return true; // Prevent default back behavior
        }

        return base.OnBackButtonPressed();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _windowManagerService.UnregisterWindow(this);
    }

    private static Color GetColor(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var resource) == true && resource is Color color)
        {
            return color;
        }

        return Colors.White;
    }
}
