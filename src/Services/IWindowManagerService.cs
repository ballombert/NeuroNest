namespace ADHDWorkspace.Services;

/// <summary>
/// Service for managing window instances and preventing duplicates
/// </summary>
public interface IWindowManagerService
{
    /// <summary>
    /// Opens a window for the specified page type. If already open, brings it to foreground.
    /// </summary>
    /// <typeparam name="TPage">The ContentPage type to open</typeparam>
    /// <returns>True if window was opened or brought to foreground successfully</returns>
    Task<bool> OpenWindowAsync<TPage>() where TPage : ContentPage;

    /// <summary>
    /// Closes the window for the specified page type if it's open
    /// </summary>
    /// <typeparam name="TPage">The ContentPage type to close</typeparam>
    void CloseWindow<TPage>() where TPage : ContentPage;

    /// <summary>
    /// Checks if a window for the specified page type is currently open
    /// </summary>
    /// <typeparam name="TPage">The ContentPage type to check</typeparam>
    /// <returns>True if the window is open</returns>
    bool IsWindowOpen<TPage>() where TPage : ContentPage;

    /// <summary>
    /// Brings the window for the specified page type to foreground if it's open
    /// </summary>
    /// <typeparam name="TPage">The ContentPage type to bring to foreground</typeparam>
    /// <returns>True if window was brought to foreground successfully</returns>
    Task<bool> BringToForegroundAsync<TPage>() where TPage : ContentPage;

    /// <summary>
    /// Unregisters a window from tracking when it's closing
    /// </summary>
    /// <param name="page">The page instance to unregister</param>
    void UnregisterWindow(ContentPage page);
}
