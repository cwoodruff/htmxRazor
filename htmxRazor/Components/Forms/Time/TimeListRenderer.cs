using System;
using System.Collections.Generic;
using System.Globalization;

namespace htmxRazor.Components.Forms.Time;

/// <summary>
/// Pure helper for the time picker: generates the selectable <see cref="TimeOnly"/> set from a
/// step (minutes) bounded by optional min/max, and formats a time for display. Culture-invariant
/// so output is deterministic. No HTTP/DI. Reused by the date+time picker (Milestone 3).
/// </summary>
public static class TimeListRenderer
{
    /// <summary>
    /// The selectable times from <paramref name="min"/> (default 00:00) to <paramref name="max"/>
    /// (default 23:59) inclusive, stepping by <paramref name="stepMinutes"/> (default 30 when &lt; 1).
    /// </summary>
    public static IReadOnlyList<TimeOnly> Times(int stepMinutes, TimeOnly? min, TimeOnly? max)
    {
        if (stepMinutes < 1) stepMinutes = 30;
        var startMin = min is { } mn ? (int)mn.ToTimeSpan().TotalMinutes : 0;
        var endMin = max is { } mx ? (int)mx.ToTimeSpan().TotalMinutes : 23 * 60 + 59;
        var list = new List<TimeOnly>();
        for (var m = startMin; m <= endMin; m += stepMinutes)
            list.Add(TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(m)));
        return list;
    }

    /// <summary>
    /// Formats a time for the visible display. <paramref name="format"/> (a .NET time format string)
    /// wins; otherwise 12-hour (<c>9:30 AM</c>) when <paramref name="twelveHour"/>, else 24-hour (<c>09:30</c>).
    /// Always uses <see cref="CultureInfo.InvariantCulture"/> for deterministic output.
    /// </summary>
    public static string FormatDisplay(TimeOnly t, bool twelveHour, string? format)
    {
        if (!string.IsNullOrEmpty(format)) return t.ToString(format, CultureInfo.InvariantCulture);
        return twelveHour
            ? t.ToString("h:mm tt", CultureInfo.InvariantCulture)
            : t.ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
