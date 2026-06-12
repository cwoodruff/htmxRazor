using System;
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace htmxRazor.Tests;

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

    [Fact]
    public async Task Renders_Wrapper_Input_Trigger_And_Hidden_Iso_Value()
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
        Assert.True(output.Attributes.TryGetAttribute("data-rhx-time-picker", out _));
        Assert.Contains("rhx-time-picker__input", html);
        Assert.Contains("rhx-time-picker__trigger", html);
        Assert.Contains("aria-haspopup=\"listbox\"", html);
        Assert.Contains("type=\"hidden\"", html);
        Assert.Contains("name=\"StartTime\"", html);
        Assert.Contains("value=\"09:30\"", html);
        Assert.Contains("data-rhx-time-value", html);
    }

    [Fact]
    public async Task Renders_Listbox_With_Options_And_Selected()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Value = "09:30";
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("rhx-time-picker__listbox", html);
        Assert.Contains("role=\"listbox\"", html);
        Assert.Contains("data-time=\"09:30\"", html);
        Assert.Contains("aria-selected=\"true\"", html);
        Assert.Contains(">9:30 AM</button>", html);
        Assert.Contains("value=\"9:30 AM\"", html);
    }

    [Fact]
    public async Task TwentyFourHour_Mode_Uses_24h_Display()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Value = "21:00";
        helper.TwelveHour = false;
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();
        Assert.Contains(">21:00</button>", html);
        Assert.Contains("value=\"21:00\"", html);
    }

    [Fact]
    public async Task Step_Controls_Option_Count()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Step = 60;
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();
        Assert.Equal(24, System.Text.RegularExpressions.Regex.Matches(html, "role=\"option\"").Count);
    }

    [Fact]
    public async Task TimeOnly_Model_Binding_Produces_Iso_Hidden_Value()
    {
        var helper = CreateHelper();
        helper.For = Expr("StartTime", new TimeOnly(14, 15));
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        Assert.Contains("value=\"14:15\"", output.Content.GetContent());
    }

    [Fact]
    public async Task DateTime_Model_Binding_Uses_Time_Component()
    {
        var helper = CreateHelper();
        helper.For = Expr("Event", new DateTime(2026, 10, 15, 8, 5, 0));
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        Assert.Contains("value=\"08:05\"", output.Content.GetContent());
    }

    [Fact]
    public async Task Disabled_Adds_Modifier_And_Disables_Input_And_Trigger()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Disabled = true;
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.True(HasClass(output, "rhx-time-picker--disabled"));
        // both the visible input and the trigger button carry disabled
        Assert.True(System.Text.RegularExpressions.Regex.Matches(html, "disabled").Count >= 2);
    }

    [Fact]
    public async Task Readonly_Adds_Modifier_And_Readonly_Attribute()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Readonly = true;
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.True(HasClass(output, "rhx-time-picker--readonly"));
        Assert.Contains("readonly", html);
    }

    [Fact]
    public async Task Label_Renders_With_For_And_AriaLabelledby()
    {
        var helper = CreateHelper();
        helper.Name = "t";
        helper.Label = "Start Time";
        var ctx = CreateContext("rhx-time-picker");
        var output = CreateOutput("rhx-time-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains(">Start Time</label>", html);
        Assert.Contains("for=\"t-input\"", html);          // label points at the input id
        Assert.Contains("aria-labelledby=\"t-label\"", html); // input references the label id
    }
}
