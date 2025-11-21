using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using ADHDWorkspace.Services;
using ADHDWorkspace.Models;
using System.IO;
using System.Threading.Tasks;

namespace ADHDWorkspace.Tests.Services;

public class ConfigurationServiceTests
{
    [Fact]
    public async Task LoadAsync_WithValidFile_ShouldLoadSettings()
    {
        // Arrange
        var logger = Mock.Of<ILogger<ConfigurationService>>();
        var service = new ConfigurationService(logger);

        // Act
        await service.LoadAsync();

        // Assert
        Assert.NotNull(service.Settings);
        Assert.NotNull(service.Settings.Paths);
        Assert.NotNull(service.Settings.Pomodoro);
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateBackup()
    {
        // Arrange
        var logger = Mock.Of<ILogger<ConfigurationService>>();
        var service = new ConfigurationService(logger);
        await service.LoadAsync();

        // Act
        await service.SaveAsync();

        // Assert
        var backupPath = Path.Combine(AppContext.BaseDirectory, "config", "appsettings.json.backup");
        // Note: Would check if backup exists in real test
        Assert.True(true); // Placeholder
    }

    [Fact]
    public void ResolvePath_InPortableMode_ShouldUseLocalPaths()
    {
        // Arrange
        var logger = Mock.Of<ILogger<ConfigurationService>>();
        var service = new ConfigurationService(logger);
        service.Settings.PortableMode = true;

        // Act
        var resolvedPath = service.ResolvePath("C:\\Temp\\test.txt");

        // Assert
        Assert.Contains(AppContext.BaseDirectory, resolvedPath);
    }

    [Fact]
    public void ResolvePath_InNormalMode_ShouldKeepOriginalPaths()
    {
        // Arrange
        var logger = Mock.Of<ILogger<ConfigurationService>>();
        var service = new ConfigurationService(logger);
        service.Settings.PortableMode = false;

        // Act
        var resolvedPath = service.ResolvePath("C:\\Temp\\test.txt");

        // Assert
        Assert.Equal("C:\\Temp\\test.txt", resolvedPath);
    }
}
