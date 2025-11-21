using ADHDWorkspace.Helpers;
using ADHDWorkspace.Models;
using ADHDWorkspace.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Diagnostics;

namespace ADHDWorkspace.Views;

public partial class RestoreContextPage : ContentPage
{
    private readonly ILogger<RestoreContextPage> _logger;
    private readonly ContextService _contextService;
    private readonly INotificationService _notificationService;
    private readonly IWindowManagerService _windowManagerService;
    private ContextSnapshot? _selectedSnapshot;

    public RestoreContextPage(
        ILogger<RestoreContextPage> logger,
        ContextService contextService,
        INotificationService notificationService,
        IWindowManagerService windowManagerService)
    {
        InitializeComponent();

        _logger = logger;
        _contextService = contextService;
        _notificationService = notificationService;
        _windowManagerService = windowManagerService;

        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        await LoadSnapshotsAsync();
    }

    private async Task LoadSnapshotsAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            SnapshotsCollection.IsVisible = false;

            var snapshots = await _contextService.ListSnapshotsAsync();
            SnapshotsCollection.ItemsSource = snapshots;

            LoadingIndicator.IsVisible = false;
            SnapshotsCollection.IsVisible = true;

            if (snapshots.Count == 0)
            {
                await DisplayAlert("No Snapshots", "No context snapshots found.", "OK");
            }

            _logger.LogInformation("Loaded {Count} context snapshots", snapshots.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load snapshots");
            LoadingIndicator.IsVisible = false;
            await DisplayAlert("Error", $"Failed to load snapshots: {ex.Message}", "OK");
        }
    }

    private void OnSnapshotSelected(object? sender, SelectionChangedEventArgs e)
    {
        _selectedSnapshot = e.CurrentSelection.FirstOrDefault() as ContextSnapshot;
        RestoreButton.IsEnabled = _selectedSnapshot != null;
    }

    private async void OnRestoreClicked(object? sender, EventArgs e)
    {
        if (_selectedSnapshot == null) return;

        try
        {
            bool confirm = await DisplayAlert(
                "Restore Context",
                $"Restore context: {_selectedSnapshot.Name}?\n\nThis will open files and windows from the snapshot.",
                "Restore",
                "Cancel");

            if (!confirm) return;

            // Open VS Code files (simplified - would need full implementation)
            if (_selectedSnapshot.VSCodeFiles.Any())
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "code",
                    Arguments = string.Join(" ", _selectedSnapshot.VSCodeFiles.Select(f => $"\"{f}\"")),
                    UseShellExecute = true
                };
                Process.Start(psi);
            }

            _notificationService.ShowSuccess("Context Restored", $"Restored: {_selectedSnapshot.Name}");
            _logger.LogInformation("Context restored: {Name}", _selectedSnapshot.Name);

            if (sender is Button restoreButton)
            {
                await FeedbackHelper.ShowConfirmationAsync(restoreButton, "✓ Restored!", Color.FromArgb("#34C759"));
            }

            await Navigation.PopAsync();
            Application.Current?.CloseWindow(Application.Current.Windows.First(w => w.Page == this));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore context");
            await DisplayAlert("Error", $"Failed to restore context: {ex.Message}", "OK");
        }
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
        Application.Current?.CloseWindow(Application.Current.Windows.First(w => w.Page == this));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _windowManagerService.UnregisterWindow(this);
    }
}
