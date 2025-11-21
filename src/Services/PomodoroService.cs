namespace ADHDWorkspace.Services;

using Microsoft.Extensions.Logging;
using ADHDWorkspace.Models;
using Microsoft.Win32;

/// <summary>
/// Service for managing Pomodoro timer cycles
/// </summary>
public class PomodoroService
{
    private readonly ILogger<PomodoroService> _logger;
    private readonly IConfigurationService _configService;
    private readonly INotificationService _notificationService;
    private readonly FocusTrackerService _focusTrackerService;
    private readonly FocusLoggerService _focusLoggerService;
    
    private CancellationTokenSource? _cts;
    private Task? _runningTask;
    private int _cycleCount = 0;
    private bool _isRunning = false;

    public PomodoroService(
        ILogger<PomodoroService> logger,
        IConfigurationService configService,
        INotificationService notificationService,
        FocusTrackerService focusTrackerService,
        FocusLoggerService focusLoggerService)
    {
        _logger = logger;
        _configService = configService;
        _notificationService = notificationService;
        _focusTrackerService = focusTrackerService;
        _focusLoggerService = focusLoggerService;
    }

    public bool IsRunning => _isRunning;
    public int CycleCount => _cycleCount;

    /// <summary>
    /// Starts the Pomodoro timer
    /// </summary>
    public void Start()
    {
        if (_isRunning)
        {
            _logger.LogWarning("Pomodoro already running");
            return;
        }

        _isRunning = true;
        _cts = new CancellationTokenSource();
        _runningTask = Task.Run(() => RunPomodoroLoop(_cts.Token), _cts.Token);
        
        _logger.LogInformation("Pomodoro service started");
    }

    /// <summary>
    /// Stops the Pomodoro timer
    /// </summary>
    public void Stop()
    {
        if (!_isRunning) return;

        _cts?.Cancel();
        _runningTask?.Wait(TimeSpan.FromSeconds(5));
        
        _isRunning = false;
        ClearPomodoroState();
        SetFocusAssist("Off");
        
        _logger.LogInformation("Pomodoro service stopped after {Cycles} cycles", _cycleCount);
        _notificationService.ShowSuccess("Pomodoro Complete", $"You completed {_cycleCount} cycle(s). Great work!");
    }

    private async Task RunPomodoroLoop(CancellationToken ct)
    {
        try
        {
            var settings = _configService.Settings.Pomodoro;

            while (!ct.IsCancellationRequested)
            {
                _cycleCount++;

                // Focus session
                await RunFocusSession(settings.FocusMinutes, _cycleCount, ct);
                if (ct.IsCancellationRequested) break;

                // After focus phase completes, save sessions
                await SaveFocusSessionsAsync();

                // Determine break type
                bool isLongBreak = (_cycleCount % settings.CyclesBeforeLongBreak == 0);
                int breakMinutes = isLongBreak ? settings.LongBreakMinutes : settings.ShortBreakMinutes;

                // Break session
                await RunBreakSession(breakMinutes, isLongBreak, ct);
                if (ct.IsCancellationRequested) break;

                // Short pause between cycles
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Pomodoro loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Pomodoro loop");
        }
    }

    private async Task SaveFocusSessionsAsync()
    {
        try
        {
            // Get active focus sessions
            var activeSessions = _focusTrackerService.GetActiveSessions();
            
            if (activeSessions.Count > 0)
            {
                // Stop all tasks with InProgress status
                _focusTrackerService.StopAllTasks(CompletionStatus.InProgress);
                
                // Get all sessions (now includes the stopped ones)
                var allSessions = _focusTrackerService.GetAllSessions();
                
                // Save to logs
                await _focusLoggerService.SaveSessionsAsync(allSessions);
                
                _logger.LogInformation("Saved {Count} focus sessions after Pomodoro cycle", allSessions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save focus sessions during Pomodoro");
        }
    }

    private async Task RunFocusSession(int minutes, int cycleNumber, CancellationToken ct)
    {
        _logger.LogInformation("Starting focus session {Cycle} for {Minutes} minutes", cycleNumber, minutes);
        
        SetFocusAssist("PriorityOnly");
        UpdatePomodoroState("focus", minutes);
        
        _notificationService.ShowToast(
            $"Focus Session {cycleNumber}",
            $"Time to focus for {minutes} minutes! 🎯");

        var endTime = DateTime.Now.AddMinutes(minutes);

        while (DateTime.Now < endTime && !ct.IsCancellationRequested)
        {
            var remaining = (int)Math.Ceiling((endTime - DateTime.Now).TotalMinutes);
            UpdatePomodoroState("focus", remaining);
            
            await Task.Delay(TimeSpan.FromMinutes(1), ct);
        }

        if (!ct.IsCancellationRequested)
        {
            _logger.LogInformation("Focus session {Cycle} completed", cycleNumber);
        }
    }

    private async Task RunBreakSession(int minutes, bool isLongBreak, CancellationToken ct)
    {
        string breakType = isLongBreak ? "Long Break" : "Short Break";
        _logger.LogInformation("Starting {BreakType} for {Minutes} minutes", breakType, minutes);
        
        SetFocusAssist("Off");
        string mode = isLongBreak ? "longbreak" : "shortbreak";
        UpdatePomodoroState(mode, minutes);
        
        _notificationService.ShowToast(
            breakType,
            $"Take a break for {minutes} minutes. Stand up, stretch, hydrate! ☕");

        var endTime = DateTime.Now.AddMinutes(minutes);

        while (DateTime.Now < endTime && !ct.IsCancellationRequested)
        {
            var remaining = (int)Math.Ceiling((endTime - DateTime.Now).TotalMinutes);
            UpdatePomodoroState(mode, remaining);
            
            await Task.Delay(TimeSpan.FromMinutes(1), ct);
        }

        if (!ct.IsCancellationRequested)
        {
            _logger.LogInformation("{BreakType} completed", breakType);
        }
    }

    private void SetFocusAssist(string mode)
    {
        try
        {
            string registryPath = @"Software\Microsoft\Windows\CurrentVersion\QuietHours";
            int modeValue = mode switch
            {
                "Off" => 0,
                "PriorityOnly" => 1,
                "AlarmsOnly" => 2,
                _ => 0
            };

            using var key = Registry.CurrentUser.OpenSubKey(registryPath, true);
            if (key != null)
            {
                key.SetValue("Profile", modeValue, RegistryValueKind.DWord);
                _logger.LogDebug("Focus Assist set to: {Mode}", mode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set Focus Assist");
        }
    }

    private void UpdatePomodoroState(string mode, int remainingMinutes)
    {
        try
        {
            var statePath = _configService.ResolvePath(_configService.Settings.Paths.PomodoroStateFile);
            var state = $"mode:{mode}\nremaining:{remainingMinutes}";
            File.WriteAllText(statePath, state);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update Pomodoro state");
        }
    }

    private void ClearPomodoroState()
    {
        try
        {
            var statePath = _configService.ResolvePath(_configService.Settings.Paths.PomodoroStateFile);
            if (File.Exists(statePath))
            {
                File.Delete(statePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clear Pomodoro state");
        }
    }
}
