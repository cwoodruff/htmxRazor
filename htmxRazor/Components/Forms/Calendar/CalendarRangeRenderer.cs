using System;
using System.Globalization;
using System.Net;
using System.Text;

namespace htmxRazor.Components.Forms.Calendar;

/// <summary>Immutable inputs for <see cref="CalendarRangeRenderer"/>. Pure data.</summary>
public sealed record CalendarRangeOptions
{
    /// <summary>Year of the LEFT month.</summary>
    public int Year { get; init; }
    /// <summary>Month (1-12) of the LEFT month; the right month is this + 1.</summary>
    public int Month { get; init; }
    public DateOnly? Min { get; init; }
    public DateOnly? Max { get; init; }
    public DayOfWeek WeekStart { get; init; } = DayOfWeek.Monday;
    public DateOnly Today { get; init; }
    public string HxGetUrl { get; init; } = "/_rhx/calendar-range";
    public string TargetId { get; init; } = "rhx-range-cal";
    public string? Format { get; init; }
}

/// <summary>
/// Renders a two-month range calendar: a single shared header (one prev/next pair + the two
/// month captions) over two plain day-grids. Range highlighting is applied client-side by the JS;
/// the grids carry no range/selected classes. Nav re-requests <see cref="CalendarRangeOptions.HxGetUrl"/>.
/// </summary>
public static class CalendarRangeRenderer
{
    private static string Enc(string s) => WebUtility.HtmlEncode(s);

    private static string NavUrl(CalendarRangeOptions o, int year, int month)
    {
        var sb = new StringBuilder();
        var sep = o.HxGetUrl.Contains('?') ? '&' : '?';
        sb.Append(o.HxGetUrl).Append(sep).Append("year=").Append(year).Append("&month=").Append(month);
        if (o.Min is { } mn) sb.Append("&min=").Append(mn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (o.Max is { } mx) sb.Append("&max=").Append(mx.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        sb.Append("&week-start=").Append(o.WeekStart.ToString().ToLowerInvariant());
        if (!string.IsNullOrEmpty(o.Format)) sb.Append("&format=").Append(Uri.EscapeDataString(o.Format));
        sb.Append("&id=").Append(Uri.EscapeDataString(o.TargetId));
        return Enc(sb.ToString());
    }

    private static CalendarOptions MonthOpts(CalendarRangeOptions o, int year, int month) => new()
    {
        Year = year, Month = month, View = CalendarView.Days,
        Selected = null, Min = o.Min, Max = o.Max, WeekStart = o.WeekStart, Today = o.Today, Format = o.Format,
        TargetId = o.TargetId, HxGetUrl = o.HxGetUrl,
    };

    public static string Render(CalendarRangeOptions o)
    {
        var left = new DateOnly(o.Year, o.Month, 1);
        var right = left.AddMonths(1);
        var prev = left.AddMonths(-1);
        var next = left.AddMonths(1);
        var leftLabel = left.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        var rightLabel = right.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        var sb = new StringBuilder();
        sb.Append($"<div class=\"rhx-date-range-picker__cal\" id=\"{Enc(o.TargetId)}\" data-rhx-range-cal>");

        sb.Append("<div class=\"rhx-date-range-picker__cal-header\">");
        sb.Append($"<button type=\"button\" class=\"rhx-calendar__nav\" aria-label=\"Previous month\" hx-get=\"{NavUrl(o, prev.Year, prev.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">&#8249;</button>");
        sb.Append($"<span class=\"rhx-date-range-picker__cal-caption\">{Enc(leftLabel)}</span>");
        sb.Append($"<span class=\"rhx-date-range-picker__cal-caption\">{Enc(rightLabel)}</span>");
        sb.Append($"<button type=\"button\" class=\"rhx-calendar__nav\" aria-label=\"Next month\" hx-get=\"{NavUrl(o, next.Year, next.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">&#8250;</button>");
        sb.Append("</div>");

        sb.Append("<div class=\"rhx-date-range-picker__months\">");
        sb.Append("<div class=\"rhx-date-range-picker__month\">").Append(CalendarRenderer.RenderDaysBody(MonthOpts(o, left.Year, left.Month))).Append("</div>");
        sb.Append("<div class=\"rhx-date-range-picker__month\">").Append(CalendarRenderer.RenderDaysBody(MonthOpts(o, right.Year, right.Month))).Append("</div>");
        sb.Append("</div>");

        sb.Append("</div>");
        return sb.ToString();
    }
}
