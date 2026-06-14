using System;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

// rhx-date-range-picker renders a pair of native <input type="date"> controls (start + end),
// no JS. Each carries its own name/value; the end's min follows the chosen start to keep order.
public class DateRangePickerTagHelperTests : TagHelperTestBase
{
    private DateRangePickerTagHelper CreateHelper() => new(CreateUrlHelperFactory())
    {
        ViewContext = CreateViewContext(),
        Today = new DateOnly(2026, 10, 9),
    };

    private async Task<string> RenderAsync(DateRangePickerTagHelper helper)
    {
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");
        await helper.ProcessAsync(ctx, output);
        return output.Content.GetContent();
    }

    [Fact]
    public async Task Renders_Two_Native_Date_Inputs_With_Values()
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
        var html = output.Content.GetContent();
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(html, "type=\"date\"").Count);
        Assert.Contains("rhx-date-range-picker__start", html);
        Assert.Contains("rhx-date-range-picker__end", html);
        Assert.Contains("name=\"From\"", html);
        Assert.Contains("name=\"To\"", html);
        Assert.Contains("value=\"2026-10-13\"", html);
        Assert.Contains("value=\"2026-10-17\"", html);
    }

    [Fact]
    public async Task End_Min_Follows_Start_Value_To_Keep_Order()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        helper.StartValue = "2026-10-13";
        var html = await RenderAsync(helper);
        // The end input's min is the chosen start so an earlier end can't be picked.
        Assert.Contains("min=\"2026-10-13\"", html);
    }

    [Fact]
    public async Task Min_And_Max_Flow_To_Both_Inputs()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        helper.Min = "2026-10-05"; helper.Max = "2026-10-25";
        var html = await RenderAsync(helper);
        Assert.Contains("min=\"2026-10-05\"", html);
        Assert.Contains("max=\"2026-10-25\"", html);
    }

    [Fact]
    public async Task Empty_Values_Render_No_Value_Attributes()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        var html = await RenderAsync(helper);
        Assert.DoesNotContain("value=\"2026", html);
    }

    [Fact]
    public async Task Encodes_Start_And_End_Names()
    {
        var helper = CreateHelper();
        helper.StartName = "<x>";
        helper.EndName = "a\"b";
        var html = await RenderAsync(helper);
        Assert.Contains("name=\"&lt;x&gt;\"", html);
        Assert.DoesNotContain("name=\"<x>\"", html);
        Assert.Contains("a&quot;b", html);
    }

    [Fact]
    public async Task Group_Has_Accessible_Label()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        helper.Label = "Date range";
        var html = await RenderAsync(helper);
        Assert.Contains("role=\"group\"", html);
        Assert.Contains("rhx-date-range-picker__label", html);
        Assert.Contains("Date range", html);
    }

    [Fact]
    public async Task Disabled_Adds_Modifier_And_Disables_Both_Inputs()
    {
        var helper = CreateHelper();
        helper.StartName = "From"; helper.EndName = "To";
        helper.Disabled = true;
        var ctx = CreateContext("rhx-date-range-picker");
        var output = CreateOutput("rhx-date-range-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.True(HasClass(output, "rhx-date-range-picker--disabled"));
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(html, " disabled").Count);
    }
}
