using System;

namespace htmxRazor.Components.Forms.Calendar;

/// <summary>Which calendar view to render.</summary>
public enum CalendarView { Days, Months, Years }

/// <summary>Immutable inputs for <see cref="CalendarRenderer"/>. Pure data; no HTTP/DI.</summary>
public sealed record CalendarOptions
{
    /// <summary>Displayed year.</summary>
    public int Year { get; init; }
    /// <summary>Displayed month, 1-12.</summary>
    public int Month { get; init; }
    /// <summary>Which view to render.</summary>
    public CalendarView View { get; init; } = CalendarView.Days;
    /// <summary>The committed selection (highlighted when visible).</summary>
    public DateOnly? Selected { get; init; }
    /// <summary>Earliest selectable date (inclusive).</summary>
    public DateOnly? Min { get; init; }
    /// <summary>Latest selectable date (inclusive).</summary>
    public DateOnly? Max { get; init; }
    /// <summary>First day of the week. Default Monday.</summary>
    public DayOfWeek WeekStart { get; init; } = DayOfWeek.Monday;
    /// <summary>"Today" for highlighting — injectable for deterministic tests.</summary>
    public DateOnly Today { get; init; }
    /// <summary>Base URL the nav controls call (built-in endpoint or app override).</summary>
    public string HxGetUrl { get; init; } = "/_rhx/calendar";
    /// <summary>Id of the calendar container; nav swaps it via hx-target/outerHTML.</summary>
    public string TargetId { get; init; } = "rhx-cal";
    /// <summary>Show the footer "Today" button.</summary>
    public bool ShowToday { get; init; } = true;
    /// <summary>Show the footer "Clear" button.</summary>
    public bool ShowClear { get; init; } = true;
    /// <summary>Optional .NET date format string for day-cell display labels (null = culture short date "d").</summary>
    public string? Format { get; init; }
}
