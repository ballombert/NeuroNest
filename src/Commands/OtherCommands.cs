using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace ADHDWorkspace.Commands;

public static class WorkspaceSetupCommand
{
    public static void Execute()
    {
        // Now integrated - would call WorkspaceSetupService directly via DI
        Console.WriteLine("⚠️  Use 'adhd setup' or call WorkspaceSetupService directly");
    }
}

/// <summary>
/// Command for managing Windows startup auto-launch
/// </summary>
public static class AutoStartCommand
{
    private const string RegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ADHDWorkspace";

    public static void Execute(string subCommand)
    {
        switch (subCommand.ToLower())
        {
            case "enable":
                EnableAutoStart();
                break;
            case "disable":
                DisableAutoStart();
                break;
            case "status":
                ShowStatus();
                break;
            default:
                Console.WriteLine("Usage: adhd autostart <enable|disable|status>");
                break;
        }
    }

    private static void EnableAutoStart()
    {
        try
        {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Failed to get executable path");
                Console.ResetColor();
                return;
            }

            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Failed to open registry key");
                Console.ResetColor();
                return;
            }

            key.SetValue(AppName, $"\"{exePath}\"", RegistryValueKind.String);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Auto-start enabled");
            Console.WriteLine($"   {AppName} will launch at Windows startup");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Failed to enable auto-start: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static void DisableAutoStart()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Failed to open registry key");
                Console.ResetColor();
                return;
            }

            var value = key.GetValue(AppName);
            if (value == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️  Auto-start is not enabled");
                Console.ResetColor();
                return;
            }

            key.DeleteValue(AppName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Auto-start disabled");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Failed to disable auto-start: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static void ShowStatus()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
            if (key == null)
            {
                Console.WriteLine("Status: Unknown (registry key not accessible)");
                return;
            }

            var value = key.GetValue(AppName) as string;

            if (string.IsNullOrEmpty(value))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Status: ❌ Disabled");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Status: ✅ Enabled");
                Console.WriteLine($"Path: {value}");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Failed to check status: {ex.Message}");
            Console.ResetColor();
        }
    }
}

public static class OverlayCommand
{
    public static void Execute()
    {
        CommandHelper.LaunchExe("ObsidianOverlay.exe");
    }
}

public static class PomodoroCommand
{
    public static void Execute()
    {
        CommandHelper.LaunchExe("DevPomodoro.exe");
    }
}

public static class SaveContextCommand
{
    public static void Execute(string? contextName)
    {
        CommandHelper.LaunchExe("AutoSaveContext.exe", contextName ?? "");
    }
}

public static class FocusTrackerCommand
{
    public static void Execute()
    {
        CommandHelper.LaunchExe("FocusTracker.exe");
    }
}

internal static class CommandHelper
{
    public static void LaunchExe(string exeName, string args = "")
{
    try
    {
        // Try scripts folder relative to current exe
        string baseDir = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? "";
        string scriptsPath = Path.Combine(baseDir, "..", "..", "..", "scripts", exeName);
        
        // Fallback to absolute path
        if (!File.Exists(scriptsPath))
        {
            scriptsPath = Path.Combine(@"C:\WORK\Perso\adhd\scripts", exeName);
        }

        if (!File.Exists(scriptsPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Erreur: {exeName} introuvable");
            Console.WriteLine($"   Cherché dans: {scriptsPath}");
            Console.ResetColor();
            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = scriptsPath,
            Arguments = args,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(scriptsPath)
        };

        Process.Start(psi);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {exeName} lancé");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erreur lors du lancement de {exeName}: {ex.Message}");
        Console.ResetColor();
    }
    }
}
