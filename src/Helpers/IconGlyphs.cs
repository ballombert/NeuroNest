using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ADHDWorkspace.Helpers;

public static class IconFontFamilies
{
    public const string Regular = "FontAwesome";
    public const string Solid = "FontAwesomeSolid";
}

public readonly record struct IconGlyph(string Glyph, string FontFamily);

public static class IconGlyphs
{
    public static IconGlyph PomodoroIdle => new("\uf192", IconFontFamilies.Regular);
    public static IconGlyph PomodoroFocus => new("\uf111", IconFontFamilies.Solid);
    public static IconGlyph PomodoroBreak => new("\uf10c", IconFontFamilies.Regular);
    public static IconGlyph FocusIdle => new("\uf10c", IconFontFamilies.Regular);
    public static IconGlyph FocusActive => new("\uf111", IconFontFamilies.Solid);
    public static IconGlyph Warning => new("\uf071", IconFontFamilies.Solid);
}

public static class IconGlyphExtensions
{
    public static void ApplyTo(this IconGlyph glyph, FontImageSource target, Color color, double? size = null)
    {
        ArgumentNullException.ThrowIfNull(target);

        target.Glyph = glyph.Glyph;
        target.FontFamily = glyph.FontFamily;
        target.Color = color;

        if (size.HasValue)
        {
            target.Size = size.Value;
        }
    }

    public static Span ToSpan(this IconGlyph glyph, double fontSize, Color color)
    {
        return new Span
        {
            Text = glyph.Glyph,
            FontFamily = glyph.FontFamily,
            FontSize = fontSize,
            TextColor = color
        };
    }
}
