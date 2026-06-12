using System;
using htmxRazor.Components.Forms.Time;
using Xunit;

namespace htmxRazor.Tests;

public class TimeListRendererTests
{
    [Fact]
    public void Times_Default_Step_30_Gives_48_Entries_From_Midnight()
    {
        var times = TimeListRenderer.Times(30, null, null);
        Assert.Equal(48, times.Count);
        Assert.Equal(new TimeOnly(0, 0), times[0]);
        Assert.Equal(new TimeOnly(0, 30), times[1]);
        Assert.Equal(new TimeOnly(23, 30), times[^1]);
    }

    [Fact]
    public void Times_Respects_Min_Max_And_Step()
    {
        var times = TimeListRenderer.Times(15, new TimeOnly(9, 0), new TimeOnly(10, 0));
        Assert.Equal(new[] { new TimeOnly(9, 0), new TimeOnly(9, 15), new TimeOnly(9, 30), new TimeOnly(9, 45), new TimeOnly(10, 0) }, times);
    }

    [Fact]
    public void Times_Invalid_Step_Falls_Back_To_30()
    {
        Assert.Equal(48, TimeListRenderer.Times(0, null, null).Count);
        Assert.Equal(48, TimeListRenderer.Times(-5, null, null).Count);
    }

    [Fact]
    public void FormatDisplay_TwelveHour_And_TwentyFourHour()
    {
        var t = new TimeOnly(9, 30);
        Assert.Equal("9:30 AM", TimeListRenderer.FormatDisplay(t, twelveHour: true, format: null));
        Assert.Equal("09:30", TimeListRenderer.FormatDisplay(t, twelveHour: false, format: null));
        Assert.Equal("9:30 AM", TimeListRenderer.FormatDisplay(new TimeOnly(9, 30), true, null));
        Assert.Equal("1:05 PM", TimeListRenderer.FormatDisplay(new TimeOnly(13, 5), true, null));
        Assert.Equal("21:00", TimeListRenderer.FormatDisplay(new TimeOnly(21, 0), false, null));
    }

    [Fact]
    public void FormatDisplay_Honors_Custom_Format()
    {
        Assert.Equal("09.30", TimeListRenderer.FormatDisplay(new TimeOnly(9, 30), true, "HH.mm"));
    }

    [Fact]
    public void RenderOptions_Emits_One_Option_Per_Time_With_Iso_And_Display()
    {
        var html = TimeListRenderer.RenderOptions(30, null, null, twelveHour: true, format: null, selected: new TimeOnly(9, 30));
        Assert.Equal(48, System.Text.RegularExpressions.Regex.Matches(html, "role=\"option\"").Count);
        Assert.Contains("data-time=\"09:30\"", html);
        Assert.Contains(">9:30 AM</button>", html);
        Assert.Contains("data-time=\"00:00\"", html);
    }

    [Fact]
    public void RenderOptions_Marks_Selected_Option()
    {
        var html = TimeListRenderer.RenderOptions(30, null, null, true, null, new TimeOnly(9, 30));
        var idx = html.IndexOf("data-time=\"09:30\"", System.StringComparison.Ordinal);
        Assert.True(idx >= 0);
        var seg = html.Substring(idx, 60);
        Assert.Contains("aria-selected=\"true\"", seg);
        Assert.Contains("rhx-time-picker__option--selected", html);
    }

    [Fact]
    public void RenderOptions_No_Selection_Has_No_AriaSelected()
    {
        var html = TimeListRenderer.RenderOptions(30, null, null, true, null, selected: null);
        Assert.DoesNotContain("aria-selected=\"true\"", html);
    }
}
