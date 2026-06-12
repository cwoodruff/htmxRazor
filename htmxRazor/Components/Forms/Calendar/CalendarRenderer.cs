using System;
using System.Globalization;
using System.Net;
using System.Text;

namespace htmxRazor.Components.Forms.Calendar;

/// <summary>
/// Pure renderer: turns <see cref="CalendarOptions"/> into the full calendar widget HTML
/// (header navigation + grid body + footer). Navigation controls carry hx-get URLs that
/// re-request the endpoint and swap the whole widget (outerHTML).
/// </summary>
public static class CalendarRenderer
{
    public static string Render(CalendarOptions o) => o.View switch
    {
        CalendarView.Months => RenderShell(o, RenderMonthsBody(o)),
        CalendarView.Years => RenderShell(o, RenderYearsBody(o)),
        _ => RenderShell(o, RenderDaysBody(o)),
    };

    private static string Enc(string s) => WebUtility.HtmlEncode(s);

    private static string NavUrl(CalendarOptions o, CalendarView view, int year, int month)
    {
        var sb = new StringBuilder();
        var sep = o.HxGetUrl.Contains('?') ? '&' : '?';
        sb.Append(o.HxGetUrl).Append(sep).Append("view=").Append(view.ToString().ToLowerInvariant());
        sb.Append("&year=").Append(year).Append("&month=").Append(month);
        if (o.Selected is { } s) sb.Append("&selected=").Append(s.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (o.Min is { } mn) sb.Append("&min=").Append(mn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (o.Max is { } mx) sb.Append("&max=").Append(mx.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        sb.Append("&week-start=").Append(o.WeekStart.ToString().ToLowerInvariant());
        if (!string.IsNullOrEmpty(o.Format)) sb.Append("&format=").Append(Uri.EscapeDataString(o.Format));
        sb.Append("&id=").Append(Uri.EscapeDataString(o.TargetId));
        return Enc(sb.ToString());
    }

    private static string RenderShell(CalendarOptions o, string body)
    {
        var label = new DateOnly(o.Year, o.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        var cur = new DateOnly(o.Year, o.Month, 1);
        var prev = cur.AddMonths(-1);
        var next = cur.AddMonths(1);

        var sb = new StringBuilder();
        sb.Append($"<div class=\"rhx-calendar\" id=\"{Enc(o.TargetId)}\" data-rhx-calendar>");

        sb.Append("<div class=\"rhx-calendar__header\">");
        sb.Append($"<button type=\"button\" class=\"rhx-calendar__nav\" aria-label=\"Previous month\" hx-get=\"{NavUrl(o, CalendarView.Days, prev.Year, prev.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">&#8249;</button>");
        sb.Append($"<button type=\"button\" class=\"rhx-calendar__label\" hx-get=\"{NavUrl(o, CalendarView.Months, o.Year, o.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">{Enc(label)}</button>");
        sb.Append($"<button type=\"button\" class=\"rhx-calendar__nav\" aria-label=\"Next month\" hx-get=\"{NavUrl(o, CalendarView.Days, next.Year, next.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">&#8250;</button>");
        sb.Append("</div>");

        sb.Append("<div class=\"rhx-calendar__body\">").Append(body).Append("</div>");

        if (o.ShowToday || o.ShowClear)
        {
            sb.Append("<div class=\"rhx-calendar__footer\">");
            if (o.ShowToday)
            {
                var todayDisabled = (o.Min is { } tmn && o.Today < tmn) || (o.Max is { } tmx && o.Today > tmx);
                sb.Append("<button type=\"button\" class=\"rhx-calendar__action\" data-rhx-cal-today");
                if (todayDisabled) sb.Append(" disabled");
                sb.Append(">Today</button>");
            }
            if (o.ShowClear) sb.Append("<button type=\"button\" class=\"rhx-calendar__action\" data-rhx-cal-clear>Clear</button>");
            sb.Append("</div>");
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    private static readonly string[] DayAbbrev = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };

    private static readonly string[] MonthAbbrev =
        { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

    private static string RenderDaysBody(CalendarOptions o)
    {
        var first = new DateOnly(o.Year, o.Month, 1);
        var label = first.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        var offset = ((int)first.DayOfWeek - (int)o.WeekStart + 7) % 7;
        var gridStart = first.AddDays(-offset);

        DateOnly focus = o.Selected is { } s && s.Year == o.Year && s.Month == o.Month ? s
            : (o.Today.Year == o.Year && o.Today.Month == o.Month ? o.Today : first);

        var sb = new StringBuilder();
        sb.Append($"<div class=\"rhx-calendar__grid\" role=\"grid\" aria-label=\"{Enc(label)}\" data-rhx-calendar-grid data-year=\"{o.Year}\" data-month=\"{o.Month}\">");

        sb.Append("<div class=\"rhx-calendar__weekdays\" role=\"row\">");
        for (var i = 0; i < 7; i++)
        {
            var dow = (DayOfWeek)(((int)o.WeekStart + i) % 7);
            sb.Append($"<span class=\"rhx-calendar__weekday\" role=\"columnheader\" aria-label=\"{dow}\">{DayAbbrev[(int)dow]}</span>");
        }
        sb.Append("</div>");

        for (var w = 0; w < 6; w++)
        {
            sb.Append("<div class=\"rhx-calendar__week\" role=\"row\">");
            for (var d = 0; d < 7; d++)
            {
                var date = gridStart.AddDays(w * 7 + d);
                var iso = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var muted = date.Month != o.Month;
                var isToday = date == o.Today;
                var isSelected = o.Selected is { } sel && date == sel;
                var disabled = (o.Min is { } mn && date < mn) || (o.Max is { } mx && date > mx);

                var cls = "rhx-calendar__day";
                if (muted) cls += " rhx-calendar__day--muted";
                if (isToday) cls += " rhx-calendar__day--today";
                if (isSelected) cls += " rhx-calendar__day--selected";

                var disp = date.ToString(string.IsNullOrEmpty(o.Format) ? "d" : o.Format, CultureInfo.CurrentCulture);
                sb.Append($"<button type=\"button\" class=\"{cls}\" role=\"gridcell\" data-date=\"{iso}\" data-display=\"{Enc(disp)}\" ");
                sb.Append(isSelected ? "aria-selected=\"true\" " : "");
                sb.Append(date == focus ? "tabindex=\"0\"" : "tabindex=\"-1\"");
                if (disabled) sb.Append(" disabled aria-disabled=\"true\"");
                sb.Append('>').Append(date.Day).Append("</button>");
            }
            sb.Append("</div>");
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    private static string RenderMonthsBody(CalendarOptions o)
    {
        var sb = new StringBuilder();
        sb.Append("<div class=\"rhx-calendar__months\" role=\"grid\" aria-label=\"Select month\">");
        for (var m = 1; m <= 12; m++)
        {
            var cls = "rhx-calendar__month-cell";
            if (m == o.Month) cls += " rhx-calendar__month-cell--selected";
            sb.Append($"<button type=\"button\" class=\"{cls}\" role=\"gridcell\" hx-get=\"{NavUrl(o, CalendarView.Days, o.Year, m)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">{MonthAbbrev[m - 1]}</button>");
        }
        sb.Append("</div>");
        return sb.ToString();
    }

    private static string RenderYearsBody(CalendarOptions o)
    {
        var start = o.Year - (o.Year % 10) - 1; // decade window with one leading/trailing year
        var sb = new StringBuilder();
        sb.Append("<div class=\"rhx-calendar__years\" role=\"grid\" aria-label=\"Select year\">");
        for (var i = 0; i < 12; i++)
        {
            var y = start + i;
            var cls = "rhx-calendar__year-cell";
            if (y == o.Year) cls += " rhx-calendar__year-cell--selected";
            sb.Append($"<button type=\"button\" class=\"{cls}\" role=\"gridcell\" hx-get=\"{NavUrl(o, CalendarView.Months, y, o.Month)}\" hx-target=\"#{Enc(o.TargetId)}\" hx-swap=\"outerHTML\">{y}</button>");
        }
        sb.Append("</div>");
        return sb.ToString();
    }
}
