using System;
using System.Diagnostics;

namespace ADHDWorkspace.Services;

/// <summary>
/// Service for managing workspace setup and application launching
/// </summary>
public class WorkspaceService
{
    public static void LaunchApp(string name, string command, string args = "")
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Starting {name}...");
        Console.ResetColor();

        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = command,
                Arguments = args,
                UseShellExecute = true
            };
            Process.Start(psi);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ {name} started");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ Failed to start {name}: {ex.Message}");
            Console.ResetColor();
        }
    }
}
