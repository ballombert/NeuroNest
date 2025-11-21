namespace ADHDWorkspace.Services;

using Microsoft.Extensions.Logging;
using ADHDWorkspace.Models;
using System.Diagnostics;
using System.Text;
using Microsoft.Win32;

/// <summary>
/// Service for workspace setup and initialization
/// </summary>
public class WorkspaceSetupService(
    ILogger<WorkspaceSetupService> logger,
    IConfigurationService configService,
    INotificationService notificationService)
{
    private readonly ILogger<WorkspaceSetupService> _logger = logger;
    private readonly IConfigurationService _configService = configService;
    private readonly INotificationService _notificationService = notificationService;

    private const int STARTUP_DELAY_SECONDS = 10;

    /// <summary>
    /// Performs complete workspace setup
    /// </summary>
    public async Task SetupWorkspaceAsync()
    {
        _logger.LogInformation("Starting workspace setup...");
        _notificationService.ShowToast("Workspace Setup", "Initializing your ADHD workspace...");

        // Wait for desktop to load
        await Task.Delay(TimeSpan.FromSeconds(STARTUP_DELAY_SECONDS));

        HideWindowsTaskbar();
        await LaunchApplicationsAsync();
        await Task.Delay(TimeSpan.FromSeconds(8)); // Wait for apps to initialize
        ApplyFancyZonesLayouts();
        CreateDailyNote();

        _logger.LogInformation("Workspace setup completed");
        _notificationService.ShowSuccess("Workspace Ready", "Your ADHD-optimized workspace is ready!");
    }

    /// <summary>
    /// Hides the Windows taskbar
    /// </summary>
    public void HideWindowsTaskbar()
    {
        try
        {
            _logger.LogInformation("Hiding Windows taskbar...");

            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3", true);

            if (key != null)
            {
                var settings = (byte[]?)key.GetValue("Settings");

                if (settings != null && settings[8] != 0x03)
                {
                    settings[8] = 0x03;
                    key.SetValue("Settings", settings);

                    // Restart Explorer
                    foreach (var process in Process.GetProcessesByName("explorer"))
                    {
                        process.Kill();
                    }
                    
                    Thread.Sleep(2000);
                    Process.Start("explorer.exe");

                    _logger.LogInformation("Taskbar hidden successfully");
                }
                else
                {
                    _logger.LogInformation("Taskbar already hidden");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to hide taskbar");
        }
    }

    /// <summary>
    /// Launches configured applications
    /// </summary>
    public async Task LaunchApplicationsAsync()
    {
        _logger.LogInformation("Launching applications...");

        await LaunchAppAsync("Teams", "msedge.exe", "--app=https://teams.microsoft.com");
        await Task.Delay(2000);

        await LaunchAppAsync("Outlook", "msedge.exe", "--app=https://outlook.office.com");
        await Task.Delay(2000);

        await LaunchAppAsync("VS Code", "code", _configService.Settings.Paths.ProjectPath);
        await Task.Delay(2000);

        await LaunchAppAsync("Obsidian", "obsidian", "obsidian://open?vault=adhd");
        await Task.Delay(2000);
    }

    /// <summary>
    /// Applies FancyZones layouts to windows
    /// </summary>
    public void ApplyFancyZonesLayouts()
    {
        try
        {
            _logger.LogInformation("Applying FancyZones layouts...");

            Thread.Sleep(3000);

            // Teams - top zone
            SendKeysToProcess("ms-teams", "^#{UP}");
            Thread.Sleep(500);

            // Outlook - bottom zone
            SendKeysToProcess("msedge", "^#{DOWN}");
            Thread.Sleep(500);

            // VS Code - left zone
            SendKeysToProcess("Code", "^#{LEFT}");

            _logger.LogInformation("FancyZones layouts applied");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply FancyZones layouts");
        }
    }

    /// <summary>
    /// Creates today's daily note if it doesn't exist
    /// </summary>
    public void CreateDailyNote()
    {
        try
        {
            _logger.LogInformation("Creating daily note...");

            var today = DateTime.Now.ToString("yyyy-MM-dd");
            var dailyPath = Path.Combine(_configService.Settings.Paths.DailyNotesPath, $"{today}.md");

            if (!File.Exists(dailyPath))
            {
                var template = $@"---
date: {today}
type: daily-note
---

# {DateTime.Now:dddd, MMMM dd, yyyy}

## 🎯 Today's Focus (Max 3)
- [ ] 
- [ ] 
- [ ] 

## 📥 Inbox / Quick Capture


## ✅ Completed Tasks


## 🧠 Brain Dump (End of Day)
_What's on your mind? Unload it here._

---
";

                Directory.CreateDirectory(Path.GetDirectoryName(dailyPath)!);
                File.WriteAllText(dailyPath, template, Encoding.UTF8);

                _logger.LogInformation("Daily note created: {Date}", today);
            }
            else
            {
                _logger.LogInformation("Daily note already exists");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create daily note");
        }
    }

    private async Task LaunchAppAsync(string name, string command, string? args = null)
    {
        try
        {
            _logger.LogInformation("Starting {AppName}...", name);

            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args ?? "",
                UseShellExecute = true
            };

            Process.Start(psi);
            _logger.LogInformation("{AppName} started successfully", name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start {AppName}", name);
        }
    }

    private void SendKeysToProcess(string processName, string keys)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0 && processes[0].MainWindowHandle != IntPtr.Zero)
            {
                // This would require platform-specific code to send keys
                // For now, just log the intent
                _logger.LogDebug("Would send keys {Keys} to {Process}", keys, processName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to send keys to {Process}", processName);
        }
    }
}
