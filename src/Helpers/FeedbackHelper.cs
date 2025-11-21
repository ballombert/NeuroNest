namespace ADHDWorkspace.Helpers;

using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

/// <summary>
/// Centralized helper for lightweight confirmation feedback animations.
/// </summary>
public static class FeedbackHelper
{
    public static async Task ShowConfirmationAsync(Button button, string successText, Color successColor, int durationMs = 600)
    {
        if (button == null)
        {
            return;
        }

        var originalText = button.Text;
        var originalBackground = button.BackgroundColor;
        var originalTextColor = button.TextColor;

        try
        {
            button.Text = successText;
            button.BackgroundColor = successColor;
            button.TextColor = Colors.Black;

            await button.ScaleTo(1.05, 80, Easing.CubicOut);
            await button.ScaleTo(1.0, 80, Easing.CubicIn);
            await Task.Delay(durationMs);
        }
        finally
        {
            button.Text = originalText;
            button.BackgroundColor = originalBackground;
            button.TextColor = originalTextColor;
        }
    }
}
