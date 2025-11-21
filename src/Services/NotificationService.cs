namespace ADHDWorkspace.Services;

using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation of notification service using Windows toast notifications
/// </summary>
public class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    private readonly ILogger<NotificationService> _logger = logger;
    private const string AppId = "ADHDWorkspace";

    public void ShowToast(string title, string message, string? icon = null)
    {
        try
        {
            var builder = new ToastContentBuilder()
                .AddText(title, hintStyle: AdaptiveTextStyle.Header)
                .AddText(message);

            if (!string.IsNullOrEmpty(icon))
            {
                builder.AddAppLogoOverride(new Uri(icon), ToastGenericAppLogoCrop.Circle);
            }

            builder.Show();
            _logger.LogDebug("Toast notification shown: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show toast notification: {Title}", title);
        }
    }

    public void ShowError(string title, string message)
    {
        try
        {
            new ToastContentBuilder()
                .AddText("❌ " + title, hintStyle: AdaptiveTextStyle.Header)
                .AddText(message)
                .AddAudio(new Uri("ms-winsoundevent:Notification.Default"))
                .Show();

            _logger.LogDebug("Error toast shown: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show error toast: {Title}", title);
        }
    }

    public void ShowSuccess(string title, string message)
    {
        try
        {
            new ToastContentBuilder()
                .AddText("✅ " + title, hintStyle: AdaptiveTextStyle.Header)
                .AddText(message)
                .Show();

            _logger.LogDebug("Success toast shown: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show success toast: {Title}", title);
        }
    }
}
