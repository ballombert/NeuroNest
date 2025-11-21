using System.Text;
using ADHDWorkspace.Helpers;
using ADHDWorkspace.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ADHDWorkspace.Views;

public partial class QuickCapturePage : ContentPage
{
    private readonly ILogger<QuickCapturePage> _logger;
    private readonly IConfigurationService _configService;
    private readonly INotificationService _notificationService;
    private readonly IWindowManagerService _windowManagerService;

    public QuickCapturePage(
        ILogger<QuickCapturePage> logger,
        IConfigurationService configService,
        INotificationService notificationService,
        IWindowManagerService windowManagerService)
    {
        InitializeComponent();

        _logger = logger;
        _configService = configService;
        _notificationService = notificationService;
        _windowManagerService = windowManagerService;

        // Focus on editor when loaded
        Loaded += (s, e) => CaptureEditor.Focus();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var text = CaptureEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            await DisplayAlert("Empty Content", "Please enter some text to capture.", "OK");
            return;
        }

        try
        {
            var inboxPath = _configService.ResolvePath(_configService.Settings.Paths.InboxPath);
            var directory = Path.GetDirectoryName(inboxPath);
            
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var entry = $"\n## {DateTime.Now:yyyy-MM-dd HH:mm}\n{text}\n";
            await File.AppendAllTextAsync(inboxPath, entry, Encoding.UTF8);

            _logger.LogInformation("Quick capture saved to Inbox");
            _notificationService.ShowSuccess("Captured!", "Your idea has been saved to Inbox.");

            if (sender is Button saveButton)
            {
                await FeedbackHelper.ShowConfirmationAsync(saveButton, "✓ Saved!", Color.FromArgb("#34C759"));
            }

            // Close the window
            await Navigation.PopAsync();
            Application.Current?.CloseWindow(Application.Current.Windows.First(w => w.Page == this));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save quick capture");
            await DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(CaptureEditor.Text))
        {
            bool discard = await DisplayAlert(
                "Discard Changes?",
                "You have unsaved text. Are you sure you want to discard it?",
                "Discard",
                "Keep Editing");

            if (!discard) return;
        }

        await Navigation.PopAsync();
        Application.Current?.CloseWindow(Application.Current.Windows.First(w => w.Page == this));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _windowManagerService.UnregisterWindow(this);
    }
}
