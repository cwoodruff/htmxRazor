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
}
