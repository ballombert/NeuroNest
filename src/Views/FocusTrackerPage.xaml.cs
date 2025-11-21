using System;
using System.Collections.Generic;
using System.Linq;
using ADHDWorkspace.Helpers;
using ADHDWorkspace.Models;
using ADHDWorkspace.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace ADHDWorkspace.Views;

public partial class FocusTrackerPage : ContentPage
{
    private readonly ILogger<FocusTrackerPage> _logger;
    private readonly FocusTrackerService _focusTrackerService;
    private readonly FocusLoggerService _focusLoggerService;
    private readonly INotificationService _notificationService;
    private readonly IWindowManagerService _windowManagerService;
    private IDispatcherTimer? _updateTimer;
    private readonly Dictionary<TaskFilter, Button> _filterButtons = new();
    private TaskFilter _currentFilter = TaskFilter.Active;

    public FocusTrackerPage(
        ILogger<FocusTrackerPage> logger,
        FocusTrackerService focusTrackerService,
        FocusLoggerService focusLoggerService,
        INotificationService notificationService,
        IWindowManagerService windowManagerService)
    {
        InitializeComponent();

        _logger = logger;
        _focusTrackerService = focusTrackerService;
        _focusLoggerService = focusLoggerService;
        _notificationService = notificationService;
        _windowManagerService = windowManagerService;

        _filterButtons[TaskFilter.All] = FilterAllButton;
        _filterButtons[TaskFilter.Active] = FilterActiveButton;
        _filterButtons[TaskFilter.Completed] = FilterCompletedButton;

        SetFilter(TaskFilter.Active, false);
        UpdateUI();
        StartUpdateTimer();
    }

    private void StartUpdateTimer()
    {
        _updateTimer = Dispatcher.CreateTimer();
        _updateTimer.Interval = TimeSpan.FromSeconds(1);
        _updateTimer.Tick += (s, e) => UpdateUI();
        _updateTimer.Start();
    }

    private void UpdateUI()
    {
        try
        {
            List<FocusSession> sessions = _currentFilter switch
            {
                TaskFilter.All => _focusTrackerService.GetAllSessions(),
                TaskFilter.Completed => _focusTrackerService.GetAllSessions().Where(s => !s.IsActive).ToList(),
                _ => _focusTrackerService.GetActiveSessions()
            };
            
            // Clear and rebuild active tasks list
            ActiveTasksContainer.Clear();

            if (sessions.Count == 0)
            {
                string emptyText = _currentFilter switch
                {
                    TaskFilter.Completed => "No completed tasks yet.",
                    TaskFilter.All => "No tasks yet. Add one above!",
                    _ => "No active tasks. Start tracking above!"
                };

                ActiveTasksContainer.Add(new Label
                {
                    Text = emptyText,
                    TextColor = GetColor("SecondaryText"),
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                });
            }
            else
            {
                foreach (var session in sessions)
                {
                    var taskCard = CreateTaskCard(session);
                    ActiveTasksContainer.Add(taskCard);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating UI");
        }
    }

    private Border CreateTaskCard(FocusSession session)
    {
        bool isTracking = session.StartTime != DateTime.MinValue && session.IsActive;
        bool isPending = session.StartTime == DateTime.MinValue && session.IsActive;
        bool isCompleted = !session.IsActive;
        
        var timeText = "";
        var reminderBadge = "";
        
        if (isTracking)
        {
            var elapsed = DateTime.Now - session.StartTime;
            var elapsedMinutes = (int)elapsed.TotalMinutes;
            var elapsedSeconds = (int)elapsed.TotalSeconds % 60;
            timeText = $"⏱️ {elapsedMinutes:D2}:{elapsedSeconds:D2}";
            
            if (session.ReminderCount > 0)
            {
                reminderBadge = $" ⚠️ {session.ReminderCount}";
            }
        }
        else if (isPending)
        {
            timeText = "Not started";
        }
        else
        {
            var elapsed = session.EndTime.HasValue && session.StartTime != DateTime.MinValue
                ? session.EndTime.Value - session.StartTime
                : TimeSpan.Zero;
            var minutes = Math.Max(1, (int)Math.Ceiling(elapsed.TotalMinutes));
            timeText = $"⏱️ {minutes} min";
        }

        var taskLabel = new Label
        {
            Text = session.TaskName,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = GetColor("PrimaryText"),
            VerticalOptions = LayoutOptions.Center
        };

        var statsLabel = new Label
        {
            Text = timeText + reminderBadge,
            FontSize = 14,
            TextColor = isTracking ? GetColor("SecondaryText") : GetColor("SecondaryText"),
            VerticalOptions = LayoutOptions.Center
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10
        };

        var textStack = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center
        };
        textStack.Add(taskLabel);
        textStack.Add(statsLabel);

        grid.Add(textStack, 0);

        int currentColumn = 1;
        if (!isCompleted)
        {
            var actionButton = new Button
            {
                Text = isTracking ? "Stop" : "Start",
                BackgroundColor = isTracking ? GetColor("AccentRed") : GetColor("AccentGreen"),
                TextColor = Colors.Black,
                CornerRadius = 8,
                WidthRequest = 80,
                HeightRequest = 40,
                VerticalOptions = LayoutOptions.Center
            };

            actionButton.CommandParameter = session.TaskName;
            actionButton.Clicked += isTracking ? OnStopTaskClicked : OnStartTrackingClicked;
            grid.Add(actionButton, currentColumn++);
        }

        var removeButton = new Button
        {
            Text = "✕",
            BackgroundColor = GetColor("SurfaceDarkAlt"),
            TextColor = GetColor("PrimaryText"),
            CornerRadius = 8,
            WidthRequest = 40,
            HeightRequest = 40,
            VerticalOptions = LayoutOptions.Center
        };

        removeButton.CommandParameter = session.TaskName;
        removeButton.Clicked += OnRemoveTaskClicked;
        grid.Add(removeButton, currentColumn);

        var border = new Border
        {
            BackgroundColor = isCompleted ? GetColor("SurfaceDark") : GetColor("SurfaceDarkAlt"),
            StrokeThickness = isTracking ? 2 : 0,
            Stroke = isTracking ? GetColor("AccentGreen") : Colors.Transparent,
            Padding = 15,
            Content = grid
        };
        border.StrokeShape = new RoundRectangle { CornerRadius = 8 };

        return border;
    }

    private async void OnAddTaskClicked(object? sender, EventArgs e)
    {
        try
        {
            string task = TaskEntry.Text?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(task))
            {
                _notificationService.ShowError("Error", "Please enter a task description.");
                return;
            }

            _focusTrackerService.CreateTask(task);
            TaskEntry.Text = "";
            
            _notificationService.ShowSuccess("Task Added", $"Click Start to begin tracking: {task}");

            if (sender is Button addButton)
            {
                await FeedbackHelper.ShowConfirmationAsync(addButton, "✓ Added", GetColor("AccentGreen"));
            }

            UpdateUI();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding task");
            _notificationService.ShowError("Error", "Failed to add task.");
        }
    }

    private async void OnStartTrackingClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is string taskName)
            {
                _focusTrackerService.StartTask(taskName);
                await FeedbackHelper.ShowConfirmationAsync(button, "✓ Started", GetColor("AccentGreen"));
                UpdateUI();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting tracking");
            _notificationService.ShowError("Error", "Failed to start tracking.");
        }
    }

    private async void OnRemoveTaskClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is string taskName)
            {
                bool confirm = await DisplayAlert(
                    "Remove Task?",
                    $"Remove task: {taskName}?",
                    "Remove",
                    "Cancel");

                if (confirm)
                {
                    _focusTrackerService.RemoveTask(taskName);
                    UpdateUI();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing task");
            _notificationService.ShowError("Error", "Failed to remove task.");
        }
    }

    private async void OnStopTaskClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is string taskName)
            {
                bool isCompleted = await DisplayAlert(
                    "Task Completed?",
                    $"Did you complete: {taskName}?",
                    "Yes - Completed",
                    "No - In Progress");

                var status = isCompleted ? CompletionStatus.Completed : CompletionStatus.InProgress;

                var session = _focusTrackerService.GetActiveSessions()
                    .FirstOrDefault(s => s.TaskName == taskName);

                if (session != null && !isCompleted)
                {
                    var duration = (DateTime.Now - session.StartTime).TotalMinutes;
                    if (duration < 10)
                    {
                        status = CompletionStatus.Abandoned;
                    }
                }

                _focusTrackerService.StopTask(taskName, status);

                var allSessions = _focusTrackerService.GetAllSessions();
                await _focusLoggerService.SaveSessionsAsync(allSessions);

                _notificationService.ShowSuccess("Task Stopped", $"Logged: {taskName}");

                if (sender is Button stopButton)
                {
                    await FeedbackHelper.ShowConfirmationAsync(stopButton, "✓ Logged", GetColor("AccentGreen"));
                }
                
                UpdateUI();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping task");
            _notificationService.ShowError("Error", "Failed to stop task.");
        }
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        _updateTimer?.Stop();
        
        var window = Application.Current?.Windows.FirstOrDefault(w => w.Page == this);
        if (window != null)
        {
            Application.Current?.CloseWindow(window);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _updateTimer?.Stop();
        _windowManagerService.UnregisterWindow(this);
    }

    private void OnFilterButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string parameter)
        {
            return;
        }

        if (!Enum.TryParse<TaskFilter>(parameter, out var filter))
        {
            return;
        }

        if (filter == _currentFilter)
        {
            return;
        }

        SetFilter(filter, true);
    }

    private void SetFilter(TaskFilter filter, bool refresh)
    {
        _currentFilter = filter;

        foreach (var kvp in _filterButtons)
        {
            bool isActive = kvp.Key == filter;
            kvp.Value.BackgroundColor = isActive ? GetColor("AccentCyan") : GetColor("SurfaceDarkAlt");
            kvp.Value.TextColor = isActive ? Colors.Black : GetColor("PrimaryText");
        }

        if (refresh)
        {
            UpdateUI();
        }
    }

    private static Color GetColor(string resourceKey)
    {
        if (Application.Current?.Resources.TryGetValue(resourceKey, out var value) == true && value is Color color)
        {
            return color;
        }

        return Colors.White;
    }
}

internal enum TaskFilter
{
    All,
    Active,
    Completed
}
