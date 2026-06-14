using System;
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace htmxRazor.Tests;

// rhx-time-picker renders a native <input type="time"> (no JS). The browser supplies the time
// entry UI and accessibility; binding is via an ISO HH:mm value, step in minutes → seconds.
public class TimePickerTagHelperTests : TagHelperTestBase
{
    private TimePickerTagHelper CreateHelper() => new(CreateUrlHelperFactory()) { ViewContext = CreateViewContext() };

    private class TestModel { public TimeOnly StartTime { get; set; } public DateTime Event { get; set; } }

    private static ModelExpression Expr(string prop, object? value)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), prop);
        return new ModelExpression(prop, new ModelExplorer(provider, metadata, value));
    }

    private async Task<string> RenderAsync(TimePickerTagHelper helper)
    {
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");
        await helper.ProcessAsync(ctx, output);
        return output.Content.GetContent();
    }

    [Fact]
    public async Task Renders_Native_Time_Input_With_Iso_Value()
    {
        var helper = CreateHelper();
        helper.Name = "StartTime";
        helper.Value = "09:30";
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-time-picker"));
        var html = output.Content.GetContent();
        Assert.Contains("type=\"time\"", html);
        Assert.Contains("rhx-time-picker__control", html);
        Assert.Contains("name=\"StartTime\"", html);
        Assert.Contains("value=\"09:30\"", html);
    }

    [Fact]
    public async Task Step_Minutes_Maps_To_Native_Seconds()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Step = 60;
        var html = await RenderAsync(helper);
        Assert.Contains("step=\"3600\"", html);   // 60 minutes → 3600 seconds
    }

    [Fact]
    public async Task Min_And_Max_Flow_To_Native_Attributes()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Min = "08:00";
        helper.Max = "17:00";
        var html = await RenderAsync(helper);
        Assert.Contains("min=\"08:00\"", html);
        Assert.Contains("max=\"17:00\"", html);
    }

    [Fact]
    public async Task TimeOnly_Model_Binding_Produces_Iso_Value()
    {
        var helper = CreateHelper();
        helper.For = Expr("StartTime", new TimeOnly(14, 15));
        var html = await RenderAsync(helper);
        Assert.Contains("value=\"14:15\"", html);
    }

    [Fact]
    public async Task DateTime_Model_Binding_Uses_Time_Component()
    {
        var helper = CreateHelper();
        helper.For = Expr("Event", new DateTime(2026, 10, 15, 8, 5, 0));
        var html = await RenderAsync(helper);
        Assert.Contains("value=\"08:05\"", html);
    }

    [Fact]
    public async Task Disabled_Adds_Modifier_And_Native_Attribute()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Disabled = true;
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        Assert.True(HasClass(output, "rhx-time-picker--disabled"));
        Assert.Contains(" disabled", output.Content.GetContent());
    }

    [Fact]
    public async Task Readonly_Adds_Modifier_And_Native_Attribute()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Readonly = true;
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        Assert.True(HasClass(output, "rhx-time-picker--readonly"));
        Assert.Contains(" readonly", output.Content.GetContent());
    }

    [Fact]
    public async Task Required_Adds_Native_Required()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Required = true;
        var html = await RenderAsync(helper);
        Assert.Contains(" required", html);
    }

    [Fact]
    public async Task Label_Renders_With_For_And_AriaLabelledby()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Label = "Start Time";
        var html = await RenderAsync(helper);
        Assert.Contains(">Start Time</label>", html);
        Assert.Contains("for=\"t\"", html);
        Assert.Contains("aria-labelledby=\"t-label\"", html);
    }
}
