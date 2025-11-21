namespace ADHDWorkspace.Services;

using Microsoft.Extensions.Logging;
using ADHDWorkspace.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// Service for tracking focus and detecting distractions
/// </summary>
public class FocusTrackerService(
    ILogger<FocusTrackerService> logger,
    IConfigurationService configService,
    INotificationService notificationService)
{
    private readonly ILogger<FocusTrackerService> _logger = logger;
    private readonly IConfigurationService _configService = configService;
    private readonly INotificationService _notificationService = notificationService;
    
    private CancellationTokenSource? _cts;
    private Task? _runningTask;
    private bool _isRunning = false;
    private List<FocusSession> _activeSessions = new();
    private readonly object _sessionsLock = new();

#if WINDOWS
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
#endif

    public bool IsRunning => _isRunning;
    
    // Legacy properties for backward compatibility
    public string CurrentTask => _activeSessions.FirstOrDefault()?.TaskName ?? "";
    public int ReminderCount => _activeSessions.FirstOrDefault()?.ReminderCount ?? 0;

    /// <summary>
    /// Create a task without starting tracking (for selection)
    /// </summary>
    public void CreateTask(string taskName)
    {
        lock (_sessionsLock)
        {
            // Check if task already exists
            if (_activeSessions.Any(s => s.TaskName == taskName))
            {
                _logger.LogWarning("Task already exists: {Task}", taskName);
                return;
            }

            var session = new FocusSession
            {
                TaskName = taskName,
                StartTime = DateTime.MinValue, // Not started yet
                Status = CompletionStatus.InProgress,
                ReminderCount = 0
            };

            _activeSessions.Add(session);
            _logger.LogInformation("Created task (not started): {Task}", taskName);
        }
    }

    /// <summary>
    /// Start tracking a specific task
    /// </summary>
    public void StartTask(string taskName)
    {
        lock (_sessionsLock)
        {
            var session = _activeSessions.FirstOrDefault(s => s.TaskName == taskName);
            
            if (session == null)
            {
                // Create new session if doesn't exist
                session = new FocusSession
                {
                    TaskName = taskName,
                    StartTime = DateTime.Now,
                    Status = CompletionStatus.InProgress,
                    ReminderCount = 0
                };
                _activeSessions.Add(session);
            }
            else if (session.StartTime == DateTime.MinValue)
            {
                // Task exists but not started - start it now
                session.StartTime = DateTime.Now;
            }

            _logger.LogInformation("Started tracking task: {Task}", taskName);

            // Start monitoring if not already running
            if (!_isRunning)
            {
                _isRunning = true;
                _cts = new CancellationTokenSource();
                _runningTask = Task.Run(() => RunFocusTracking(_cts.Token), _cts.Token);
                UpdateFocusState();
            }

            _notificationService.ShowSuccess("Focus Tracker", $"Now tracking: {taskName}");
        }
    }

    /// <summary>
    /// Pause tracking a specific task (keep it in the list but stop timer)
    /// </summary>
    public void PauseTask(string taskName)
    {
        lock (_sessionsLock)
        {
            var session = _activeSessions.FirstOrDefault(s => s.TaskName == taskName && s.IsActive);
            if (session != null && session.StartTime != DateTime.MinValue)
            {
                // Calculate time spent so far and pause
                var duration = (int)Math.Ceiling((DateTime.Now - session.StartTime).TotalMinutes);
                session.EndTime = DateTime.Now;
                _logger.LogInformation("Paused task: {Task}, Duration so far: {Duration}min", taskName, duration);
                
                // Mark as in-progress so it's not logged yet
                session.Status = CompletionStatus.InProgress;
                
                // Reset for next start
                session.StartTime = DateTime.MinValue;
                session.EndTime = null;
            }

            // Stop monitoring if no more active sessions
            if (!_activeSessions.Any(s => s.IsActive && s.StartTime != DateTime.MinValue))
            {
                Stop();
            }
        }
    }

    /// <summary>
    /// Remove a task from the list
    /// </summary>
    public void RemoveTask(string taskName)
    {
        lock (_sessionsLock)
        {
            _activeSessions.RemoveAll(s => s.TaskName == taskName);
            _logger.LogInformation("Removed task: {Task}", taskName);
        }
    }

    /// <summary>
    /// Stop tracking a specific task
    /// </summary>
    public void StopTask(string taskName, CompletionStatus status)
    {
        lock (_sessionsLock)
        {
            var session = _activeSessions.FirstOrDefault(s => s.TaskName == taskName && s.IsActive);
            if (session != null)
            {
                session.EndTime = DateTime.Now;
                session.Status = status;
                _logger.LogInformation("Stopped task: {Task}, Duration: {Duration}min, Status: {Status}", 
                    taskName, session.DurationMinutes, status);
            }

            // Stop monitoring if no more active sessions
            if (!_activeSessions.Any(s => s.IsActive))
            {
                Stop();
            }
        }
    }

    /// <summary>
    /// Stop all active tasks
    /// </summary>
    public void StopAllTasks(CompletionStatus status)
    {
        lock (_sessionsLock)
        {
            foreach (var session in _activeSessions.Where(s => s.IsActive))
            {
                session.EndTime = DateTime.Now;
                session.Status = status;
                _logger.LogInformation("Stopped task: {Task}, Duration: {Duration}min, Status: {Status}", 
                    session.TaskName, session.DurationMinutes, status);
            }

            Stop();
        }
    }

    /// <summary>
    /// Get all active sessions (including not yet started)
    /// </summary>
    public List<FocusSession> GetActiveSessions()
    {
        lock (_sessionsLock)
        {
            return _activeSessions.Where(s => s.IsActive).ToList();
        }
    }

    /// <summary>
    /// Get all sessions that are currently being tracked (started)
    /// </summary>
    public List<FocusSession> GetTrackingSessions()
    {
        lock (_sessionsLock)
        {
            return _activeSessions.Where(s => s.IsActive && s.StartTime != DateTime.MinValue).ToList();
        }
    }

    /// <summary>
    /// Get all sessions (for logging)
    /// </summary>
    public List<FocusSession> GetAllSessions()
    {
        lock (_sessionsLock)
        {
            return new List<FocusSession>(_activeSessions);
        }
    }

    /// <summary>
    /// Legacy Start method for backward compatibility
    /// </summary>
    public void Start(string taskDescription = "")
    {
        if (!string.IsNullOrWhiteSpace(taskDescription))
        {
            StartTask(taskDescription);
        }
    }

    /// <summary>
    /// Stops the focus tracker
    /// </summary>
    public void Stop()
    {
        if (!_isRunning) return;

        _cts?.Cancel();
        _runningTask?.Wait(TimeSpan.FromSeconds(5));
        
        _isRunning = false;
        ClearFocusState();
        
        _logger.LogInformation("Focus tracker stopped");
    }

    /// <summary>
    /// Updates the current task description (legacy)
    /// </summary>
    public void UpdateTask(string taskDescription)
    {
        lock (_sessionsLock)
        {
            var activeSession = _activeSessions.FirstOrDefault(s => s.IsActive);
            if (activeSession != null)
            {
                activeSession.TaskName = taskDescription;
                UpdateFocusState();
                _logger.LogInformation("Focus task updated: {Task}", taskDescription);
            }
        }
    }

    private async Task RunFocusTracking(CancellationToken ct)
    {
        try
        {
            var settings = _configService.Settings.FocusTracker;
            var checkInterval = TimeSpan.FromSeconds(settings.CheckIntervalSeconds);
            var reminderCooldown = TimeSpan.FromMinutes(settings.ReminderCooldownMinutes);
            var lastReminderTime = DateTime.MinValue;

            while (!ct.IsCancellationRequested)
            {
                var activeApp = GetActiveApplicationName();
                
                if (!string.IsNullOrEmpty(activeApp))
                {
                    bool isDistraction = IsDistractionApp(activeApp, settings.DistractionApps);
                    
                    if (isDistraction)
                    {
                        var timeSinceLastReminder = DateTime.Now - lastReminderTime;
                        
                        if (timeSinceLastReminder >= reminderCooldown)
                        {
                            // Increment reminder count for all active sessions
                            lock (_sessionsLock)
                            {
                                foreach (var session in _activeSessions.Where(s => s.IsActive))
                                {
                                    session.ReminderCount++;
                                }
                            }

                            lastReminderTime = DateTime.Now;
                            UpdateFocusState();
                            
                            var tasksList = string.Join(", ", GetActiveSessions().Select(s => s.TaskName));
                            _logger.LogWarning("Distraction detected: {App}. Active tasks: {Tasks}", activeApp, tasksList);
                            
                            _notificationService.ShowToast(
                                "⚠️ Focus Reminder",
                                $"You're using {activeApp}. Back to work!");
                        }
                    }
                }

                await Task.Delay(checkInterval, ct);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Focus tracking cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in focus tracking loop");
        }
    }

    private string GetActiveApplicationName()
    {
#if WINDOWS
        try
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return "";

            GetWindowThreadProcessId(hwnd, out uint processId);
            var process = Process.GetProcessById((int)processId);
            
            return process.ProcessName.ToLowerInvariant();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get active application");
            return "";
        }
#else
        return "";
#endif
    }

    private bool IsDistractionApp(string appName, string[] distractionApps)
    {
        return distractionApps.Any(distraction =>
            appName.Contains(distraction, StringComparison.OrdinalIgnoreCase));
    }

    private void UpdateFocusState()
    {
        try
        {
            var statePath = _configService.ResolvePath(_configService.Settings.Paths.FocusStateFile);
            var activeSessions = GetActiveSessions();
            var state = $"tasks:{activeSessions.Count}\nactive:true";
            File.WriteAllText(statePath, state);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update focus state");
        }
    }

    private void ClearFocusState()
    {
        try
        {
            var statePath = _configService.ResolvePath(_configService.Settings.Paths.FocusStateFile);
            if (File.Exists(statePath))
            {
                File.Delete(statePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clear focus state");
        }
    }
}
