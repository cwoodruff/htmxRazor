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
    public void Days_Nav_Arrows_Carry_HxGet_With_PrevNext_Month()
    {
        var html = CalendarRenderer.Render(DaysOpts());
        Assert.Contains("hx-get=\"/_rhx/calendar?view=days&amp;year=2026&amp;month=9", html);
        Assert.Contains("hx-get=\"/_rhx/calendar?view=days&amp;year=2026&amp;month=11", html);
        Assert.Contains("hx-target=\"#dp1-cal\"", html);
        Assert.Contains("hx-swap=\"outerHTML\"", html);
    }
}
