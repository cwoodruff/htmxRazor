using System;
using System.Globalization;
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace htmxRazor.Tests;

public class DatePickerTagHelperTests : TagHelperTestBase
{
    private DatePickerTagHelper CreateHelper() => new(CreateUrlHelperFactory())
    {
        ViewContext = CreateViewContext(),
        Today = new DateOnly(2026, 10, 9),
    };

    [Fact]
    public async Task Renders_Wrapper_Input_And_Hidden_Iso_Value()
    {
        var helper = CreateHelper();
        helper.Name = "DueDate";
        helper.Value = "2026-10-15";
        var ctx = CreateContext("rhx-date-picker");
        var output = CreateOutput("rhx-date-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-date-picker"));
        var html = output.Content.GetContent();
        Assert.Contains("rhx-date-picker__input", html);
        Assert.Contains("type=\"hidden\"", html);
        Assert.Contains("name=\"DueDate\"", html);
        Assert.Contains("value=\"2026-10-15\"", html);
        Assert.Contains("data-rhx-date-value", html);
    }

    [Fact]
    public async Task Renders_Trigger_And_Initial_Calendar_For_Value_Month()
    {
        var helper = CreateHelper();
        helper.Name = "DueDate";
        helper.Value = "2026-10-15";
        var ctx = CreateContext("rhx-date-picker");
        var output = CreateOutput("rhx-date-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("rhx-date-picker__trigger", html);
        Assert.Contains("aria-haspopup=\"dialog\"", html);
        Assert.Contains("rhx-calendar", html);
        Assert.Contains("October 2026", html);
        Assert.Contains("data-date=\"2026-10-15\" aria-selected=\"true\"", html);
    }

    [Fact]
    public async Task Empty_Value_Shows_Today_Month_And_No_Selection()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        var ctx = CreateContext("rhx-date-picker");
        var output = CreateOutput("rhx-date-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("October 2026", html);
        Assert.DoesNotContain("aria-selected=\"true\"", html);
        Assert.Contains("value=\"\"", html);
    }

    [Fact]
    public async Task WeekStart_And_MinMax_Flow_Into_Calendar()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Value = "2026-10-15";
        helper.Min = "2026-10-10";
        var ctx = CreateContext("rhx-date-picker");
        var output = CreateOutput("rhx-date-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        var idx = html.IndexOf("data-date=\"2026-10-05\"", StringComparison.Ordinal);
        Assert.True(idx >= 0);
        Assert.Contains("disabled", html.Substring(idx, 120));
    }
}
