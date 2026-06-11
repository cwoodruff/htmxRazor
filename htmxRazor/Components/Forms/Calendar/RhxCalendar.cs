namespace htmxRazor.Components.Forms.Calendar;

/// <summary>
/// Public facade for rendering a calendar grid from app code. Use this in a custom
/// page handler when overriding <c>hx-get</c> on a date picker to inject app-specific
/// data (availability, disabled dates) while returning the standard grid markup.
/// </summary>
public static class RhxCalendar
{
    /// <summary>Renders the full calendar widget HTML for the given options.</summary>
    public static string RenderMonth(CalendarOptions options) => CalendarRenderer.Render(options);
}
