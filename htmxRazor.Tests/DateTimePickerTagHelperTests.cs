using System;
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace htmxRazor.Tests;

// rhx-datetime-picker renders a native <input type="datetime-local"> (no JS). Binding is via
// an ISO yyyy-MM-ddTHH:mm value; step in minutes → seconds; min/max bound to whole days.
public class DateTimePickerTagHelperTests : TagHelperTestBase
{
    private DateTimePickerTagHelper CreateHelper() => new(CreateUrlHelperFactory())
    {
        ViewContext = CreateViewContext(),
        Today = new DateOnly(2026, 10, 9),
    };

    private class TestModel { public DateTime StartsAt { get; set; } }

    private static ModelExpression Expr(string prop, object? value)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), prop);
        return new ModelExpression(prop, new ModelExplorer(provider, metadata, value));
    }

    private async Task<string> RenderAsync(DateTimePickerTagHelper helper)
    {
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");
        await helper.ProcessAsync(ctx, output);
        return output.Content.GetContent();
    }

    [Fact]
    public async Task Renders_Native_DateTimeLocal_Input_With_Iso_Value()
    {
        var helper = CreateHelper();
        helper.Name = "StartsAt";
        helper.Value = "2026-10-15T09:30";
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-datetime-picker"));
        var html = output.Content.GetContent();
        Assert.Contains("type=\"datetime-local\"", html);
        Assert.Contains("rhx-datetime-picker__control", html);
        Assert.Contains("name=\"StartsAt\"", html);
        Assert.Contains("value=\"2026-10-15T09:30\"", html);
    }

    [Fact]
    public async Task Empty_Value_Has_No_Value_Attribute()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        var html = await RenderAsync(helper);
        Assert.DoesNotContain("value=\"", html);
    }

    [Fact]
    public async Task DateTime_Model_Binding_Produces_Iso()
    {
        var helper = CreateHelper();
        helper.For = Expr("StartsAt", new DateTime(2026, 10, 15, 14, 0, 0));
        var html = await RenderAsync(helper);
        Assert.Contains("value=\"2026-10-15T14:00\"", html);
    }

    [Fact]
    public async Task Min_And_Max_Bound_To_Whole_Days()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Min = "2026-10-01";
        helper.Max = "2026-10-31";
        var html = await RenderAsync(helper);
        Assert.Contains("min=\"2026-10-01T00:00\"", html);
        Assert.Contains("max=\"2026-10-31T23:59\"", html);
    }

    [Fact]
    public async Task Step_Minutes_Maps_To_Native_Seconds()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Step = 15;
        var html = await RenderAsync(helper);
        Assert.Contains("step=\"900\"", html);
    }

    [Fact]
    public async Task Disabled_Adds_Modifier_And_Native_Attribute()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Disabled = true;
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);
        Assert.True(HasClass(output, "rhx-datetime-picker--disabled"));
        Assert.Contains(" disabled", output.Content.GetContent());
    }

    [Fact]
    public async Task Label_Renders_With_For_And_AriaLabelledby()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Label = "Starts at";
        var html = await RenderAsync(helper);
        Assert.Contains(">Starts at</label>", html);
        Assert.Contains("for=\"d\"", html);
        Assert.Contains("aria-labelledby=\"d-label\"", html);
    }
}
