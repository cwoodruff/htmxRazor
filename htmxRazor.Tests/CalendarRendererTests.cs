using System;
using htmxRazor.Components.Forms.Calendar;
using Xunit;

namespace htmxRazor.Tests;

public class CalendarRendererTests
{
    private static CalendarOptions DaysOpts() => new()
    {
        Year = 2026, Month = 10, View = CalendarView.Days,
        Selected = new DateOnly(2026, 10, 15),
        Today = new DateOnly(2026, 10, 9),
        WeekStart = DayOfWeek.Monday,
        HxGetUrl = "/_rhx/calendar", TargetId = "dp1-cal",
    };

    [Fact]
    public void Days_Renders_Grid_With_Header_And_42_Day_Cells()
    {
        var html = CalendarRenderer.Render(DaysOpts());
        Assert.Contains("data-rhx-calendar", html);
        Assert.Contains("id=\"dp1-cal\"", html);
        Assert.Contains("role=\"grid\"", html);
        Assert.Contains("October 2026", html);
        Assert.Equal(42, System.Text.RegularExpressions.Regex.Matches(html, "role=\"gridcell\"").Count);
    }

    [Fact]
    public void Days_Marks_Today_Selected_And_Muted()
    {
        var html = CalendarRenderer.Render(DaysOpts());
        Assert.Contains("data-date=\"2026-10-09\"", html);
        Assert.Contains("rhx-calendar__day--today", html);
        Assert.Contains("data-date=\"2026-10-15\" ", html);
        Assert.Contains("rhx-calendar__day--selected", html);
        Assert.Contains("aria-selected=\"true\"", html);
        Assert.Contains("data-date=\"2026-09-28\"", html);
        Assert.Contains("rhx-calendar__day--muted", html);
    }

    [Fact]
    public void Days_Weekday_Header_Starts_Monday()
    {
        var html = CalendarRenderer.Render(DaysOpts());
        var moIdx = html.IndexOf(">Mo<", StringComparison.Ordinal);
        var suIdx = html.IndexOf(">Su<", StringComparison.Ordinal);
        Assert.True(moIdx >= 0 && suIdx > moIdx, "Mo should appear before Su for Monday-start.");
    }

    [Fact]
    public void Days_Disables_Out_Of_Range_Days()
    {
        var opts = DaysOpts() with { Min = new DateOnly(2026, 10, 10), Max = new DateOnly(2026, 10, 20) };
        var html = CalendarRenderer.Render(opts);
        var idx = html.IndexOf("data-date=\"2026-10-05\"", StringComparison.Ordinal);
        Assert.True(idx >= 0);
        var cell = html.Substring(idx, 120);
        Assert.Contains("disabled", cell);
        Assert.Contains("aria-disabled=\"true\"", cell);
    }

    [Fact]
    public void Days_Weekday_Header_Starts_Sunday_When_Configured()
    {
        var html = CalendarRenderer.Render(DaysOpts() with { WeekStart = DayOfWeek.Sunday });
        var suIdx = html.IndexOf(">Su<", StringComparison.Ordinal);
        var moIdx = html.IndexOf(">Mo<", StringComparison.Ordinal);
        Assert.True(suIdx >= 0 && moIdx > suIdx, "Su should appear before Mo for Sunday-start.");
    }

    [Fact]
    public void Days_Nav_Arrows_Carry_HxGet_With_PrevNext_Month()
    {
        var html = CalendarRenderer.Render(DaysOpts());
        Assert.Contains("hx-get=\"/_rhx/calendar?view=days&amp;year=2026&amp;month=9", html);
        Assert.Contains("hx-get=\"/_rhx/calendar?view=days&amp;year=2026&amp;month=11", html);
        Assert.Contains("hx-target=\"#dp1-cal\"", html);
        Assert.Contains("hx-swap=\"outerHTML\"", html);
    }

    [Fact]
    public void Months_View_Renders_12_Month_Buttons_With_HxGet_To_Days()
    {
        var o = DaysOpts() with { View = CalendarView.Months };
        var html = CalendarRenderer.Render(o);
        Assert.Equal(12, System.Text.RegularExpressions.Regex.Matches(html, "class=\"rhx-calendar__month-cell").Count);
        Assert.Contains(">Jan<", html);
        Assert.Contains(">Dec<", html);
        Assert.Contains("hx-get=\"/_rhx/calendar?view=days&amp;year=2026&amp;month=10", html);
        Assert.Contains("rhx-calendar__month-cell--selected", html);
    }

    [Fact]
    public void Years_View_Renders_Decade_Of_Year_Buttons()
    {
        var o = DaysOpts() with { View = CalendarView.Years };
        var html = CalendarRenderer.Render(o);
        Assert.Contains(">2026<", html);
        Assert.Contains("rhx-calendar__year-cell--selected", html);
        Assert.Contains("hx-get=\"/_rhx/calendar?view=months&amp;year=2026", html);
    }

    [Fact]
    public void Endpoint_Parses_Query_And_Renders_Days_By_Default()
    {
        var q = new Microsoft.AspNetCore.Http.QueryCollection(
            new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["year"] = "2026", ["month"] = "10", ["selected"] = "2026-10-15",
                ["min"] = "2026-10-01", ["max"] = "2026-10-31",
                ["week-start"] = "monday", ["id"] = "dp1-cal",
                ["format"] = "yyyy-MM-dd",
            });

        var html = CalendarEndpoint.Render(q, today: new DateOnly(2026, 10, 9));

        Assert.Contains("id=\"dp1-cal\"", html);
        Assert.Contains("October 2026", html);
        Assert.Contains("data-date=\"2026-10-15\" data-display=\"2026-10-15\" aria-selected=\"true\"", html);
    }

    [Fact]
    public void Endpoint_Defaults_To_Today_Month_When_Year_Month_Missing()
    {
        var q = new Microsoft.AspNetCore.Http.QueryCollection(
            new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
        var html = CalendarEndpoint.Render(q, today: new DateOnly(2026, 3, 4));
        Assert.Contains("March 2026", html);
    }

    [Fact]
    public void Endpoint_Degrades_Gracefully_On_Garbage_Query()
    {
        var q = new Microsoft.AspNetCore.Http.QueryCollection(
            new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["year"] = "0", ["month"] = "abc", ["selected"] = "notadate",
                ["min"] = "", ["week-start"] = "garbage",
            });

        // Must not throw, and falls back to today's month.
        var html = CalendarEndpoint.Render(q, today: new DateOnly(2026, 5, 7));
        Assert.Contains("May 2026", html);
    }

    [Fact]
    public void Days_Cells_Carry_Server_Formatted_Display()
    {
        var html = CalendarRenderer.Render(DaysOpts() with { Format = "yyyy-MM-dd" });
        Assert.Contains("data-date=\"2026-10-15\" data-display=\"2026-10-15\"", html);
    }

    [Fact]
    public void Nav_Url_Includes_Format_When_Set()
    {
        var html = CalendarRenderer.Render(DaysOpts() with { Format = "yyyy-MM-dd" });
        Assert.Contains("format=yyyy-MM-dd", html);
    }

    [Fact]
    public void Today_Action_Disabled_When_Today_Out_Of_Range()
    {
        // Today is 2026-10-09 (from DaysOpts); set Max earlier so today is out of range.
        var html = CalendarRenderer.Render(DaysOpts() with { Max = new DateOnly(2026, 10, 1) });
        var idx = html.IndexOf("data-rhx-cal-today", StringComparison.Ordinal);
        Assert.True(idx >= 0);
        Assert.Contains("disabled", html.Substring(idx, 40));
    }

    [Fact]
    public void RhxCalendar_RenderMonth_Produces_Same_Grid_As_Renderer()
    {
        var opts = DaysOpts();
        Assert.Equal(CalendarRenderer.Render(opts), htmxRazor.Components.Forms.Calendar.RhxCalendar.RenderMonth(opts));
    }
}
