namespace ADHDWorkspace.Services;

using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Logging;
using ADHDWorkspace.Models;

/// <summary>
/// Service for configuring and managing Serilog logging
/// </summary>
public static class LoggerService
{
    private static ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes Serilog with configuration from settings
    /// </summary>
    public static ILoggerFactory Initialize(AppSettings settings, bool verboseMode = false)
    {
        var logLevel = verboseMode 
            ? LogEventLevel.Debug 
            : ParseLogLevel(settings.Logging.MinimumLevel);

        var logPath = settings.PortableMode
            ? Path.Combine(AppContext.BaseDirectory, "data", "adhd-workspace-.log")
            : settings.Logging.FilePath;

        // Ensure log directory exists
        var logDirectory = Path.GetDirectoryName(logPath);
        if (logDirectory != null && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
            .Enrich.FromLogContext()
            .CreateLogger();

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger, dispose: true);
        });

        Log.Information("Logging initialized at {LogLevel} level", logLevel);
        Log.Information("Log file: {LogPath}", logPath);
        Log.Information("Portable mode: {PortableMode}", settings.PortableMode);

        return _loggerFactory;
    }

    /// <summary>
    /// Gets the current logger factory
    /// </summary>
    public static ILoggerFactory GetLoggerFactory()
    {
        return _loggerFactory ?? throw new InvalidOperationException("LoggerService not initialized");
    }

    /// <summary>
    /// Closes and flushes all loggers
    /// </summary>
    public static void Shutdown()
    {
        Log.CloseAndFlush();
    }

    private static LogEventLevel ParseLogLevel(string level)
    {
        return level.ToLowerInvariant() switch
        {
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}
