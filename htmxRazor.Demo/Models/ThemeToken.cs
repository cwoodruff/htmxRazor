namespace htmxRazor.Demo.Models;

/// <summary>
/// Defines a CSS custom property token for the theme builder.
/// </summary>
/// <param name="CssProperty">The CSS custom property name (e.g., "--rhx-color-brand-500").</param>
/// <param name="Category">Display category (e.g., "Color Palette: Brand").</param>
/// <param name="Label">Human-readable label (e.g., "Brand 500").</param>
/// <param name="InputType">HTML input type: "color", "text", "range".</param>
/// <param name="DefaultLight">Default value for light theme.</param>
/// <param name="DefaultDark">Default value for dark theme (null if same as light).</param>
public record ThemeToken(
    string CssProperty,
    string Category,
    string Label,
    string InputType,
    string DefaultLight,
    string? DefaultDark = null
);
