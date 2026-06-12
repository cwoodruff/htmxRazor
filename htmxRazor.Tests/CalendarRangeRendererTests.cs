using System;
using htmxRazor.Components.Forms.Calendar;
using Xunit;

namespace htmxRazor.Tests;

public class CalendarRangeRendererTests
{
    private static CalendarRangeOptions Opts() => new()
    {
        Year = 2026, Month = 10,
        Today = new DateOnly(2026, 10, 9),
        WeekStart = DayOfWeek.Monday,
        HxGetUrl = "/_rhx/calendar-range", TargetId = "rp1-cal",
    };

    [Fact]
    public void Renders_Two_Month_Grids_With_Captions()
    {
        var html = CalendarRangeRenderer.Render(Opts());
        Assert.Contains("id=\"rp1-cal\"", html);
        Assert.Contains("data-rhx-range-cal", html);
        Assert.Contains("October 2026", html);
        Assert.Contains("November 2026", html);
        Assert.Equal(84, System.Text.RegularExpressions.Regex.Matches(html, "role=\"gridcell\"").Count);
    }

    [Fact]
    public void Nav_Moves_Both_Months_By_One()
    {
        var html = CalendarRangeRenderer.Render(Opts());
        Assert.Contains("hx-get=\"/_rhx/calendar-range?year=2026&amp;month=9", html);
        Assert.Contains("hx-get=\"/_rhx/calendar-range?year=2026&amp;month=11", html);
        Assert.Contains("hx-target=\"#rp1-cal\"", html);
        Assert.Contains("hx-swap=\"outerHTML\"", html);
    }

    [Fact]
    public void Grids_Are_Plain_No_Range_Classes_Server_Side()
    {
        var html = CalendarRangeRenderer.Render(Opts());
        Assert.DoesNotContain("rhx-calendar__day--range", html);
        Assert.DoesNotContain("rhx-calendar__day--selected", html);
    }

    [Fact]
    public void Year_Boundary_December_Pairs_With_Next_January()
    {
        var html = CalendarRangeRenderer.Render(Opts() with { Year = 2026, Month = 12 });
        Assert.Contains("December 2026", html);
        Assert.Contains("January 2027", html);
        Assert.Contains("hx-get=\"/_rhx/calendar-range?year=2027&amp;month=1", html);
        Assert.Contains("hx-get=\"/_rhx/calendar-range?year=2026&amp;month=11", html);
    }

    [Fact]
    public void Day_Cells_Carry_Data_Date_And_Data_Display()
    {
        var html = CalendarRangeRenderer.Render(Opts() with { Format = "yyyy-MM-dd" });
        Assert.Contains("data-date=\"2026-10-01\"", html);
        // with Format yyyy-MM-dd, the data-display is the ISO date (culture-independent assertion)
        Assert.Contains("data-date=\"2026-10-01\" data-display=\"2026-10-01\"", html);
    }

    [Fact]
    public void Min_Max_Disables_Days_Outside_Range_In_The_Grids()
    {
        var html = CalendarRangeRenderer.Render(Opts() with
        {
            Min = new DateOnly(2026, 10, 15),
            Max = new DateOnly(2026, 11, 15),
        });
        // Oct 1 is before Min -> disabled
        var idx = html.IndexOf("data-date=\"2026-10-01\"", System.StringComparison.Ordinal);
        Assert.True(idx >= 0);
        Assert.Contains("disabled", html.Substring(idx, 120));
        // Nov 30 is after Max -> disabled
        idx = html.IndexOf("data-date=\"2026-11-30\"", System.StringComparison.Ordinal);
        Assert.True(idx >= 0);
        Assert.Contains("disabled", html.Substring(idx, 120));
    }
}
