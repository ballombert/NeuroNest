using System;
using System.Collections.Generic;
using System.Drawing;

namespace ADHDWorkspace.Models;

/// <summary>
/// Represents a task to focus on during the day
/// </summary>
public class FocusTask
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public bool IsCompleted { get; set; }
    public int ReminderCount { get; set; }

    public override string ToString() => Title;
}

/// <summary>
/// Represents a context snapshot with workspace state
/// </summary>
public class ContextSnapshot
{
    public string Name { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<string> VSCodeFiles { get; set; } = new();
    public List<WindowInfo> ActiveWindows { get; set; } = new();
    public GitInfo? GitInfo { get; set; }
}

/// <summary>
/// Git repository information
/// </summary>
public class GitInfo
{
    public string Branch { get; set; } = string.Empty;
    public bool HasChanges { get; set; }
}

/// <summary>
/// Information about an open window
/// </summary>
public class WindowInfo
{
    public string ProcessName { get; set; } = string.Empty;
    public string WindowTitle { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// Application launcher button configuration
/// </summary>
public class AppLauncher
{
    public string Name { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public System.Drawing.Color Color { get; set; }
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// Status of a focus session
/// </summary>
public enum CompletionStatus
{
    Completed,
    InProgress,
    Abandoned
}

/// <summary>
/// Represents a focus session with timing and completion tracking
/// </summary>
public class FocusSession
{
    public string TaskName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int ReminderCount { get; set; }
    public CompletionStatus Status { get; set; }
    
    /// <summary>
    /// Calculated duration in minutes (ceiling)
    /// </summary>
    public int DurationMinutes
    {
        get
        {
            if (EndTime == null) return 0;
            var duration = (EndTime.Value - StartTime).TotalMinutes;
            return (int)Math.Ceiling(duration);
        }
    }
    
    /// <summary>
    /// Whether the session is currently active
    /// </summary>
    public bool IsActive => EndTime == null;
}
