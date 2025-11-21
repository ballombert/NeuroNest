using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;

namespace ADHDWorkspace.WinUI;

/// <summary>
/// WinUI-specific application entry point
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        try
        {
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", $"WinUI App() constructor at {DateTime.Now}\n");
            InitializeComponent();
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "InitializeComponent OK\n");
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", $"ERROR in WinUI App(): {ex}\n");
        }
    }

    protected override MauiApp CreateMauiApp()
    {
        try
        {
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "CreateMauiApp() called\n");
            
            // Parse command line arguments
            var args = Environment.GetCommandLineArgs();
            bool verboseMode = args.Contains("--verbose") || args.Contains("-v");
            bool portableMode = args.Contains("--portable") || args.Contains("-p");
            
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", $"Creating MauiApp verbose={verboseMode} portable={portableMode}\n");
            var mauiApp = MauiProgram.CreateMauiApp(verboseMode, portableMode);
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", "MauiApp created successfully\n");
            return mauiApp;
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(@"C:\WORK\Perso\adhd\app_debug.txt", $"ERROR in CreateMauiApp: {ex}\n");
            throw;
        }
    }
}
