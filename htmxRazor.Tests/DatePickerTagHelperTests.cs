using System;
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace htmxRazor.Tests;

// rhx-date-picker renders a native <input type="date"> (no JS). The browser supplies the
// calendar popup, keyboard entry, and accessibility; binding is via an ISO yyyy-MM-dd value.
public class DatePickerTagHelperTests : TagHelperTestBase
{
    private DatePickerTagHelper CreateHelper() => new(CreateUrlHelperFactory())
    {
        ViewContext = CreateViewContext(),
        Today = new DateOnly(2026, 10, 9),
    };

    private class TestModel
    {
        public DateTime EventDate { get; set; }
    }

    private static ModelExpression CreateModelExpressionFor(string propertyName, object? value)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), propertyName);
        var explorer = new ModelExplorer(provider, metadata, value);
        return new ModelExpression(propertyName, explorer);
    }

    private async Task<string> RenderAsync(DatePickerTagHelper helper)
    {
        var ctx = CreateContext("rhx-date-picker");
        var output = CreateOutput("rhx-date-picker");
        await helper.ProcessAsync(ctx, output);
        return output.Content.GetContent();
    }

    [Fact]
    public async Task Renders_Native_Date_Input_With_Iso_Value()
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
        Assert.Contains("type=\"date\"", html);
        Assert.Contains("rhx-date-picker__control", html);
        Assert.Contains("name=\"DueDate\"", html);
        Assert.Contains("value=\"2026-10-15\"", html);
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
    public async Task Min_And_Max_Flow_To_Native_Attributes()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Min = "2026-10-10";
        helper.Max = "2026-10-31";
        var html = await RenderAsync(helper);
        Assert.Contains("min=\"2026-10-10\"", html);
        Assert.Contains("max=\"2026-10-31\"", html);
    }

    [Fact]
    public async Task Disabled_Disables_Input()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Disabled = true;
        var ctx = CreateContext("rhx-date-picker");
        var output = CreateOutput("rhx-date-picker");
        await helper.ProcessAsync(ctx, output);

        Assert.True(HasClass(output, "rhx-date-picker--disabled"));
        Assert.Contains(" disabled", output.Content.GetContent());
    }

    [Fact]
    public async Task Readonly_Adds_Native_Readonly()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Readonly = true;
        var html = await RenderAsync(helper);
        Assert.Contains(" readonly", html);
    }

    [Fact]
    public async Task Required_Adds_Native_Required()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Required = true;
        var html = await RenderAsync(helper);
        Assert.Contains(" required", html);
    }

    [Fact]
    public async Task Label_Is_Associated_With_Input()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Label = "Due date";
        var html = await RenderAsync(helper);
        Assert.Contains("rhx-date-picker__label", html);
        Assert.Contains("Due date", html);
        Assert.Contains("for=\"d\"", html);
    }

    [Fact]
    public async Task DateTime_Model_Binding_Produces_Iso_Value()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("EventDate", new DateTime(2026, 10, 15, 9, 30, 0));
        var html = await RenderAsync(helper);
        Assert.Contains("value=\"2026-10-15\"", html);   // time stripped to date
    }
}
