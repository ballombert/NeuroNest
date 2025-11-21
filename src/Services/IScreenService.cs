namespace ADHDWorkspace.Services;

/// <summary>
/// Service for detecting and managing screen positions
/// </summary>
public interface IScreenService
{
    /// <summary>
    /// Gets the rightmost screen's working area
    /// </summary>
    (int X, int Y, int Width, int Height) GetRightmostScreenWorkingArea();
    
    /// <summary>
    /// Gets the primary screen's working area
    /// </summary>
    (int X, int Y, int Width, int Height) GetPrimaryScreenWorkingArea();
}
