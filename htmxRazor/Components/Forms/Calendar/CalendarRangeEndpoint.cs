using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace htmxRazor.Components.Forms.Calendar;

/// <summary>Translates a query string into <see cref="CalendarRangeOptions"/> and renders the dual-month widget.</summary>
public static class CalendarRangeEndpoint
{
    public static string Render(IQueryCollection q, DateOnly today)
    {
        var year = ParseInt(q["year"].ToString(), today.Year);
        if (year < 1 || year > 9999) year = today.Year;
        var month = ParseInt(q["month"].ToString(), today.Month);
        if (month < 1 || month > 12) month = today.Month;

        var weekStart = Enum.TryParse<DayOfWeek>(q["week-start"], ignoreCase: true, out var ws) ? ws : DayOfWeek.Monday;

        var opts = new CalendarRangeOptions
        {
            Year = year,
            Month = month,
            Min = ParseDate(q["min"]),
            Max = ParseDate(q["max"]),
            WeekStart = weekStart,
            Today = today,
            Format = q["format"].Count == 0 ? null : q["format"].ToString(),
            TargetId = string.IsNullOrWhiteSpace(q["id"]) ? "rhx-range-cal" : q["id"].ToString(),
        };
        return CalendarRangeRenderer.Render(opts);
    }

    private static int ParseInt(string? s, int fallback) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    private static DateOnly? ParseDate(string? s) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
}
