using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace htmxRazor.Components.Forms.Calendar;

/// <summary>Translates a query string into a <see cref="CalendarOptions"/> and renders it.</summary>
public static class CalendarEndpoint
{
    public static string Render(IQueryCollection q, DateOnly today)
    {
        var year = ParseInt(q["year"].ToString(), today.Year);
        if (year < 1 || year > 9999) year = today.Year;
        var month = ParseInt(q["month"].ToString(), today.Month);
        if (month < 1 || month > 12) month = today.Month;

        var view = q["view"].ToString().ToLowerInvariant() switch
        {
            "months" => CalendarView.Months,
            "years" => CalendarView.Years,
            _ => CalendarView.Days,
        };

        var weekStart = Enum.TryParse<DayOfWeek>(q["week-start"], ignoreCase: true, out var ws)
            ? ws : DayOfWeek.Monday;

        var idVal = q["id"].ToString();
        var opts = new CalendarOptions
        {
            Year = year,
            Month = month,
            View = view,
            Selected = ParseDate(q["selected"].Count == 0 ? null : q["selected"].ToString()),
            Min = ParseDate(q["min"].Count == 0 ? null : q["min"].ToString()),
            Max = ParseDate(q["max"].Count == 0 ? null : q["max"].ToString()),
            WeekStart = weekStart,
            Today = today,
            TargetId = string.IsNullOrWhiteSpace(idVal) ? "rhx-cal" : idVal,
            Format = q["format"].Count == 0 ? null : q["format"].ToString(),
        };
        return CalendarRenderer.Render(opts);
    }

    private static int ParseInt(string? s, int fallback) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    private static DateOnly? ParseDate(string? s) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d : null;
}
