///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   Information("Running tasks...");
   Information("Target: {0}", target);
   Information("Configuration: {0}", configuration);
});

Teardown(ctx =>
{
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories($"./bin/{configuration}");
    CleanDirectories($"./obj/{configuration}");
});

Task("GenerateTrayIcon")
    .Description("Generate tray icon from logo using Python script")
    .Does(() =>
{
    var logoPath = File("./Resources/Images/Logo.png");
    var iconPath = File("./Resources/Images/trayicon.ico");
    var scriptPath = File("./scripts/convert_logo_to_tray_icon.py");
    
    if (!FileExists(logoPath))
    {
        throw new Exception($"Logo file not found: {logoPath}");
    }
    
    if (!FileExists(scriptPath))
    {
        throw new Exception($"Python script not found: {scriptPath}");
    }
    
    Information("Checking Python availability...");
    var pythonCheck = StartProcess("python", new ProcessSettings 
    {
        Arguments = "--version",
        RedirectStandardOutput = true
    });
    
    if (pythonCheck != 0)
    {
        throw new Exception("Python not found. Please install Python and ensure it's in PATH.");
    }
    
    Information("Generating tray icon from {0}...", logoPath);
    var exitCode = StartProcess("python", new ProcessSettings 
    {
        Arguments = MakeAbsolute(scriptPath).FullPath,
        WorkingDirectory = MakeAbsolute(Directory(".")).FullPath
    });
    
    if (exitCode != 0)
    {
        throw new Exception($"Python script failed with exit code {exitCode}. Ensure 'pip install pillow' has been run.");
    }
    
    if (!FileExists(iconPath))
    {
        throw new Exception($"Icon file was not created: {iconPath}");
    }
    
    Information("Tray icon generated successfully at {0}", iconPath);
});

Task("Restore")
    .IsDependentOn("GenerateTrayIcon")
    .Does(() =>
{
    DotNetRestore("./ADHDWorkspace.sln");
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetBuild("./ADHDWorkspace.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
        NoRestore = true
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./tests/ADHDWorkspace.Tests.csproj", new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
        NoRestore = true
    });
});

Task("Default")
    .IsDependentOn("Build");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);
