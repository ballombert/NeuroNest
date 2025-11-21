namespace ADHDWorkspace.Services;

using Microsoft.Extensions.Logging;
using ADHDWorkspace.Models;
using System.Text.Json;
using System.Diagnostics;

/// <summary>
/// Service for saving and restoring workspace context
/// </summary>
public class ContextService(
    ILogger<ContextService> logger,
    IConfigurationService configService)
{
    private readonly ILogger<ContextService> _logger = logger;
    private readonly IConfigurationService _configService = configService;

    /// <summary>
    /// Saves the current workspace context
    /// </summary>
    public async Task<string> SaveContextAsync(string? contextName = null)
    {
        try
        {
            var snapshot = new ContextSnapshot
            {
                Timestamp = DateTime.Now,
                Name = contextName ?? $"Auto-{DateTime.Now:yyyy-MM-dd HH:mm}",
                VSCodeFiles = await GetVSCodeOpenFilesAsync(),
                GitInfo = GetGitInfo(),
                ActiveWindows = GetActiveWindows()
            };

            var fileName = $"{DateTime.Now:yyyy-MM-dd_HHmm}_{SanitizeFileName(snapshot.Name)}.json";
            var contextPath = _configService.ResolvePath(_configService.Settings.Paths.ContextHistoryPath);
            
            Directory.CreateDirectory(contextPath);
            
            var filePath = Path.Combine(contextPath, fileName);
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);

            // Cleanup old snapshots (keep last 30)
            await CleanupOldSnapshotsAsync(contextPath);

            _logger.LogInformation("Context saved: {Name} to {File}", snapshot.Name, fileName);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save context");
            throw;
        }
    }

    /// <summary>
    /// Lists available context snapshots
    /// </summary>
    public async Task<List<ContextSnapshot>> ListSnapshotsAsync()
    {
        try
        {
            var contextPath = _configService.ResolvePath(_configService.Settings.Paths.ContextHistoryPath);
            
            if (!Directory.Exists(contextPath))
            {
                return [];
            }

            var files = Directory.GetFiles(contextPath, "*.json")
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToList();

            var snapshots = new List<ContextSnapshot>();

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var snapshot = JsonSerializer.Deserialize<ContextSnapshot>(json);
                    if (snapshot != null)
                    {
                        snapshots.Add(snapshot);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load snapshot: {File}", file);
                }
            }

            return snapshots;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list snapshots");
            return [];
        }
    }

    private async Task<List<string>> GetVSCodeOpenFilesAsync()
    {
        var files = new List<string>();
        
        try
        {
            // Try to get VS Code workspace state
            var workspacePath = _configService.Settings.Paths.WorkspacePath;
            var vscodeDir = Path.Combine(workspacePath, ".vscode");
            
            if (Directory.Exists(vscodeDir))
            {
                // This is a simplified approach - in reality would need to parse VS Code state files
                files.Add($"{workspacePath}/*.cs");
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get VS Code files");
        }

        return files;
    }

    private GitInfo? GetGitInfo()
    {
        try
        {
            var workspacePath = _configService.Settings.Paths.WorkspacePath;
            var gitDir = Path.Combine(workspacePath, ".git");
            
            if (!Directory.Exists(gitDir))
            {
                return null;
            }

            var branch = RunGitCommand("rev-parse --abbrev-ref HEAD");
            var status = RunGitCommand("status --short");

            return new GitInfo
            {
                Branch = branch?.Trim() ?? "unknown",
                HasChanges = !string.IsNullOrWhiteSpace(status)
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get Git info");
            return null;
        }
    }

    private string? RunGitCommand(string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = _configService.Settings.Paths.WorkspacePath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }

    private List<WindowInfo> GetActiveWindows()
    {
        var windows = new List<WindowInfo>();

        try
        {
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .ToList();

            foreach (var process in processes)
            {
                windows.Add(new WindowInfo
                {
                    ProcessName = process.ProcessName,
                    WindowTitle = process.MainWindowTitle
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get active windows");
        }

        return windows;
    }

    private async Task CleanupOldSnapshotsAsync(string contextPath)
    {
        try
        {
            var files = Directory.GetFiles(contextPath, "*.json")
                .OrderByDescending(f => File.GetCreationTime(f))
                .Skip(30)
                .ToList();

            foreach (var file in files)
            {
                File.Delete(file);
                _logger.LogDebug("Deleted old snapshot: {File}", Path.GetFileName(file));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup old snapshots");
        }
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
    }
}
