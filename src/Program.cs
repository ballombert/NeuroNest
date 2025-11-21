using Microsoft.UI.Xaml;

namespace ADHDWorkspace;

/// <summary>
/// Main entry point for ADHD Workspace application
/// Unified MAUI application with command routing and single instance enforcement
/// </summary>
public class Program
{
    private static Mutex? _mutex;
    private const string MutexName = "ADHDWorkspace_SingleInstance";

    [STAThread]
    static void Main(string[] args)
    {
        // Parse arguments
        bool verboseMode = args.Contains("--verbose") || args.Contains("-v");
        bool portableMode = args.Contains("--portable") || args.Contains("-p");
        
        // Remove flags from args
        var commandArgs = args.Where(a => !a.StartsWith("--") && !a.StartsWith("-")).ToArray();

        // Check for single instance (only for GUI mode)
        if (commandArgs.Length == 0 || commandArgs[0].ToLower() == "taskbar")
        {
            _mutex = new Mutex(true, MutexName, out bool createdNew);
            
            if (!createdNew)
            {
                // Another instance is running
                Console.WriteLine("❌ ADHDWorkspace is already running.");
                ShowToastNotification("Already Running", "ADHDWorkspace is already active in the system tray.");
                return;
            }
        }

        try
        {
            // Default behavior: launch MAUI app with MiniTaskbar
            if (commandArgs.Length == 0)
            {
                WinRT.ComWrappersSupport.InitializeComWrappers();
                Microsoft.UI.Xaml.Application.Start((p) =>
                {
                    var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                    System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                    new WinUI.App();
                });
                return;
            }

            string command = commandArgs[0].ToLower();

            switch (command)
            {
                case "setup":
                    Commands.WorkspaceSetupCommand.Execute();
                    break;

                case "autostart":
                    string subCommand = commandArgs.Length > 1 ? commandArgs[1] : "status";
                    Commands.AutoStartCommand.Execute(subCommand);
                    break;

                case "save":
                    string contextName = commandArgs.Length > 1 ? string.Join(" ", commandArgs.Skip(1)) : null;
                    Commands.SaveContextCommand.Execute(contextName);
                    break;

                default:
                    Console.WriteLine($"❌ Unknown command: {command}");
                    Console.WriteLine("Available commands: setup, autostart, save");
                    Console.WriteLine("Run without arguments to launch the GUI");
                    ShowUsage();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.ResetColor();
            
            if (verboseMode)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        finally
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }



    static void ShowToastNotification(string title, string message)
    {
        try
        {
            var notification = new Microsoft.Toolkit.Uwp.Notifications.ToastContentBuilder()
                .AddText(title)
                .AddText(message);
            
            notification.Show();
        }
        catch
        {
            // Silently fail if toast notifications not available
        }
    }

    static void ShowUsage()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         ADHD Workspace - Unified MAUI Application        ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Usage: ADHDWorkspace.exe [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Default: Launches MiniTaskbar with all background services");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        
        var commands = new[]
        {
            ("taskbar", "Launch mini taskbar (default, all-in-one interface)"),
            ("capture", "Quick capture window for ideas to Inbox"),
            ("restore", "Restore previous workspace context"),
            ("settings", "Open settings configuration UI"),
            ("setup", "Initialize complete workspace at startup"),
            ("save [name]", "Save current context snapshot"),
            ("autostart <enable|disable|status>", "Manage Windows startup")
        };

        foreach (var (cmd, desc) in commands)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"  {cmd,-35}");
            Console.ResetColor();
            Console.WriteLine($" {desc}");
        }

        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --verbose, -v          Enable debug logging");
        Console.WriteLine("  --portable, -p         Use portable mode (local data storage)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  ADHDWorkspace.exe");
        Console.WriteLine("  ADHDWorkspace.exe --verbose");
        Console.WriteLine("  ADHDWorkspace.exe save \"Feature Implementation\"");
        Console.WriteLine("  ADHDWorkspace.exe autostart enable");
        Console.WriteLine();
        Console.WriteLine("Note: MiniTaskbar includes Pomodoro, Focus Tracker, and all services.");
        Console.WriteLine();
    }
}
