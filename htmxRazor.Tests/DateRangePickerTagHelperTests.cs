using System;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

public class DateRangePickerTagHelperTests : TagHelperTestBase
{
    private DateRangePickerTagHelper CreateHelper() => new(CreateUrlHelperFactory())
    {
        ViewContext = CreateViewContext(),
        Today = new DateOnly(2026, 10, 9),
    };

    [Fact]
    public async Task Renders_Wrapper_Input_Two_Hidden_Inputs_And_Range_Calendar()
    {
        var helper = CreateHelper();
        helper.StartName = "From";
        helper.EndName = "To";
        helper.StartValue = "2026-10-13";
        helper.EndValue = "2026-10-17";
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-date-range-picker"));
        Assert.True(output.Attributes.TryGetAttribute("data-rhx-date-range-picker", out _));
        var html = output.Content.GetContent();
        Assert.Contains("rhx-date-range-picker__input", html);
        Assert.Contains("name=\"From\"", html);
        Assert.Contains("name=\"To\"", html);
        Assert.Contains("data-rhx-range-start", html);
        Assert.Contains("data-rhx-range-end", html);
        Assert.Contains("value=\"2026-10-13\"", html);
        Assert.Contains("value=\"2026-10-17\"", html);
        Assert.Contains("data-rhx-range-cal", html);
        Assert.Contains("October 2026", html);
        Assert.Contains("November 2026", html);
    }

    [Fact]
    public async Task Seeds_Js_With_Range_Data_Attributes()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        helper.StartValue = "2026-10-13"; helper.EndValue = "2026-10-17";
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("2026-10-13", GetAttribute(output, "data-range-start"));
        Assert.Equal("2026-10-17", GetAttribute(output, "data-range-end"));
    }

    [Fact]
    public async Task Renders_Presets_When_Requested()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        helper.Presets = "today,last7,thismonth,last30";
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("rhx-date-range-picker__presets", html);
        Assert.Contains("data-range-preset=\"last7\"", html);
        Assert.Contains("data-range-preset=\"thismonth\"", html);
        Assert.Contains(">Last 7 days<", html);
    }

    [Fact]
    public async Task Empty_Values_Render_Today_Months_And_Empty_Hidden()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("October 2026", html);
        Assert.Contains("November 2026", html);
        Assert.Equal("", GetAttribute(output, "data-range-start"));
    }

    [Fact]
    public async Task Encodes_Start_And_End_Names()
    {
        var helper = CreateHelper();
        helper.StartName = "<x>";
        helper.EndName = "a\"b";
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("name=\"&lt;x&gt;\"", html);   // start name HTML-encoded
        Assert.DoesNotContain("name=\"<x>\"", html);
        Assert.Contains("a&quot;b", html);             // end name quote encoded
    }

    [Fact]
    public async Task Disabled_Adds_Modifier_And_Disables_Input_And_Trigger()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        helper.Disabled = true;
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.True(HasClass(output, "rhx-date-range-picker--disabled"));
        Assert.True(System.Text.RegularExpressions.Regex.Matches(html, "disabled").Count >= 2);
    }
}
