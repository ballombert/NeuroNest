namespace ADHDWorkspace.Services;

using Microsoft.Extensions.Logging;
using ADHDWorkspace.Models;
using System.Text;

/// <summary>
/// Service for logging focus sessions to markdown files
/// </summary>
public class FocusLoggerService
{
    private readonly IConfigurationService _configService;
    private readonly ILogger<FocusLoggerService> _logger;

    public FocusLoggerService(
        IConfigurationService configService,
        ILogger<FocusLoggerService> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    /// <summary>
    /// Save focus sessions to daily note and monthly log
    /// </summary>
    public async Task SaveSessionsAsync(List<FocusSession> sessions)
    {
        if (sessions == null || sessions.Count == 0)
        {
            _logger.LogDebug("No sessions to save");
            return;
        }

        try
        {
            // Filter sessions that should be logged (completed or in-progress >= 10min)
            var sessionsToLog = sessions
                .Where(s => s.Status == CompletionStatus.Completed || 
                           (s.Status == CompletionStatus.InProgress && s.DurationMinutes >= 10))
                .ToList();

            if (sessionsToLog.Count == 0)
            {
                _logger.LogDebug("No sessions meet logging criteria");
                return;
            }

            // Save to daily note
            await SaveToDailyNoteAsync(sessionsToLog);

            // Save to monthly log
            await SaveToMonthlyLogAsync(sessionsToLog);

            _logger.LogInformation("Saved {Count} focus session(s)", sessionsToLog.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save focus sessions");
        }
    }

    private async Task SaveToDailyNoteAsync(List<FocusSession> sessions)
    {
        try
        {
            var dailyNotesPath = _configService.ResolvePath(_configService.Settings.Paths.DailyNotesPath);
            var today = DateTime.Now;
            var dailyNotePath = Path.Combine(dailyNotesPath, $"{today:yyyy-MM-dd}.md");

            // Ensure directory exists
            Directory.CreateDirectory(dailyNotesPath);

            // Read existing content or create new
            string existingContent = "";
            if (File.Exists(dailyNotePath))
            {
                existingContent = await File.ReadAllTextAsync(dailyNotePath);
            }
            else
            {
                // Create new daily note with header
                existingContent = $"# {today:yyyy-MM-dd}\n\n";
            }

            // Find or create "## ✅ Completed Tasks" section
            var tasksSectionHeader = "## ✅ Completed Tasks";
            var lines = existingContent.Split('\n').ToList();
            int insertIndex = -1;

            // Find the section
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim() == tasksSectionHeader)
                {
                    insertIndex = i + 1;
                    break;
                }
            }

            // If section doesn't exist, add it at the end
            if (insertIndex == -1)
            {
                lines.Add("");
                lines.Add(tasksSectionHeader);
                insertIndex = lines.Count;
            }

            // Generate task entries
            var taskEntries = new List<string>();
            foreach (var session in sessions)
            {
                var checkbox = session.Status == CompletionStatus.Completed ? "[x]" : "[/]";
                var emoji = session.ReminderCount > 0 ? "🔴" : "✅";
                var reminderText = session.ReminderCount > 0 ? $" {emoji} {session.ReminderCount} reminders" : "";
                var timeRange = session.EndTime.HasValue 
                    ? $"_{session.StartTime:HH:mm}-{session.EndTime.Value:HH:mm}_" 
                    : $"_{session.StartTime:HH:mm}-..._";
                
                var entry = $"- {checkbox} {session.TaskName} ⏱️ {session.DurationMinutes}min{reminderText} {timeRange}";
                taskEntries.Add(entry);
            }

            // Insert task entries
            lines.InsertRange(insertIndex, taskEntries);

            // Write back to file
            await File.WriteAllTextAsync(dailyNotePath, string.Join('\n', lines));

            _logger.LogDebug("Updated daily note: {Path}", dailyNotePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save to daily note");
        }
    }

    private async Task SaveToMonthlyLogAsync(List<FocusSession> sessions)
    {
        try
        {
            var notesPath = _configService.ResolvePath(_configService.Settings.Paths.ObsidianVaultPath);
            var today = DateTime.Now;
            var monthlyLogPath = Path.Combine(notesPath, $"focus-log-{today:yyyy-MM}.md");

            // Ensure directory exists
            Directory.CreateDirectory(notesPath);

            bool fileExists = File.Exists(monthlyLogPath);
            
            // Create or append to file
            using var writer = new StreamWriter(monthlyLogPath, append: true);

            // If new file, add header and table header
            if (!fileExists)
            {
                await writer.WriteLineAsync($"# Focus Log - {today:MMMM yyyy}");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("| Date | Task | Duration | Reminders | Status | Time |");
                await writer.WriteLineAsync("|------|------|----------|-----------|--------|------|");
            }

            // Add session entries
            foreach (var session in sessions)
            {
                var date = session.StartTime.ToString("yyyy-MM-dd");
                var status = session.Status.ToString();
                var reminders = session.ReminderCount.ToString();
                var timeRange = session.EndTime.HasValue 
                    ? $"{session.StartTime:HH:mm}-{session.EndTime.Value:HH:mm}" 
                    : $"{session.StartTime:HH:mm}-...";
                
                var row = $"| {date} | {session.TaskName} | {session.DurationMinutes}min | {reminders} | {status} | {timeRange} |";
                await writer.WriteLineAsync(row);
            }

            _logger.LogDebug("Updated monthly log: {Path}", monthlyLogPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save to monthly log");
        }
    }
}
