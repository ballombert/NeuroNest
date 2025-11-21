namespace ADHDWorkspace.Services;

/// <summary>
/// Service for displaying Windows toast notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Shows a toast notification
    /// </summary>
    void ShowToast(string title, string message, string? icon = null);
    
    /// <summary>
    /// Shows an error toast notification
    /// </summary>
    void ShowError(string title, string message);
    
    /// <summary>
    /// Shows a success toast notification
    /// </summary>
    void ShowSuccess(string title, string message);
}
