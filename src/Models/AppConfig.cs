namespace ADHDWorkspace.Models;

/// <summary>
/// Application settings loaded from appsettings.json
/// </summary>
public class AppSettings
{
    public LoggingSettings Logging { get; set; } = new();
    public PathSettings Paths { get; set; } = new();
    public PomodoroSettings Pomodoro { get; set; } = new();
    public FocusTrackerSettings FocusTracker { get; set; } = new();
    public HotkeySettings Hotkeys { get; set; } = new();
    public UISettings UI { get; set; } = new();
    public bool PortableMode { get; set; }
}

public class LoggingSettings
{
    public string MinimumLevel { get; set; } = "Information";
    public string FilePath { get; set; } = "C:\\Temp\\adhd-workspace-.log";
}

public class PathSettings
{
    public string WorkspacePath { get; set; } = "C:\\WORK\\Perso\\adhd";
    public string ProjectPath { get; set; } = "C:\\WORK\\Perso";
    public string ObsidianVaultPath { get; set; } = "C:\\WORK\\Perso\\adhd\\notes";
    public string InboxPath { get; set; } = "C:\\WORK\\Vault\\SyncVault\\Inbox\\Inbox.md";
    public string DailyNotesFolder { get; set; } = "C:\\WORK\\Perso\\adhd\\notes\\Daily";
    public string TemplatesFolder { get; set; } = "C:\\WORK\\Perso\\adhd\\notes\\Templates";
    public string ContextHistoryPath { get; set; } = "C:\\WORK\\Perso\\adhd\\context-history";
    public string PomodoroStateFile { get; set; } = "C:\\Temp\\pomodoro-state.txt";
    public string FocusStateFile { get; set; } = "C:\\Temp\\focus-tracker-state.txt";
    
    // Backward compatibility computed property
    public string DailyNotesPath => DailyNotesFolder;
}

public class PomodoroSettings
{
    public int FocusMinutes { get; set; } = 50;
    public int ShortBreakMinutes { get; set; } = 10;
    public int LongBreakMinutes { get; set; } = 30;
    public int CyclesBeforeLongBreak { get; set; } = 4;
}

public class FocusTrackerSettings
{
    public int CheckIntervalSeconds { get; set; } = 15;
    public int ReminderCooldownMinutes { get; set; } = 5;
    public string[] DistractionApps { get; set; } = [];
    public string[] ProductiveApps { get; set; } = [];
}

public class HotkeySettings
{
    public string QuickCapture { get; set; } = "Win+Shift+N";
    public string StartPomodoro { get; set; } = "Win+Alt+F";
    public string RestoreContext { get; set; } = "Win+Alt+R";
    public string ShowSettings { get; set; } = "Win+Alt+S";
}

public class UISettings
{
    public int TaskbarCollapsedWidth { get; set; } = 275;
    public int TaskbarCollapsedHeight { get; set; } = 75;
    public int TaskbarExpandedWidth { get; set; } = 400;
    public int TaskbarExpandedHeight { get; set; } = 450;
    public int OpacityActive { get; set; } = 100;
    public int OpacityBackground { get; set; } = 80;
    public int OpacityPomodoro { get; set; } = 50;
    public bool FocusModeEnabled { get; set; } = false;
}

/// <summary>
/// Static configuration helper for backward compatibility and defaults
/// </summary>
public static class AppConfig
{
    private static AppSettings? _current;
    
    public static AppSettings Current
    {
        get => _current ?? throw new InvalidOperationException("AppSettings not initialized. Call Initialize() first.");
        set => _current = value;
    }
    
    public static void Initialize(AppSettings settings)
    {
        _current = settings;
    }
}
