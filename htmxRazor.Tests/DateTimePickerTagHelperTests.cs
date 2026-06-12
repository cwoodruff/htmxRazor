using System;
using htmxRazor.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace htmxRazor.Tests;

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

    [Fact]
    public async Task Renders_Wrapper_Input_Trigger_And_Hidden_Iso_DateTime()
    {
        var helper = CreateHelper();
        helper.Name = "StartsAt";
        helper.Value = "2026-10-15T09:30";
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-datetime-picker"));
        Assert.True(output.Attributes.TryGetAttribute("data-rhx-datetime-picker", out _));
        var html = output.Content.GetContent();
        Assert.Contains("rhx-datetime-picker__input", html);
        Assert.Contains("rhx-datetime-picker__trigger", html);
        Assert.Contains("aria-haspopup=\"dialog\"", html);
        Assert.Contains("type=\"hidden\"", html);
        Assert.Contains("name=\"StartsAt\"", html);
        Assert.Contains("value=\"2026-10-15T09:30\"", html);
        Assert.Contains("data-rhx-dt-value", html);
    }

    [Fact]
    public async Task Popup_Has_Calendar_For_Date_And_TimeList_With_Selected_Time()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        helper.Value = "2026-10-15T09:30";
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("rhx-calendar", html);
        Assert.Contains("October 2026", html);
        Assert.Contains("data-date=\"2026-10-15\"", html);
        Assert.Contains("rhx-datetime-picker__times", html);
        Assert.Contains("data-time=\"09:30\"", html);
        Assert.Contains("rhx-time-picker__option--selected", html);
        Assert.Contains("data-rhx-dt-clear", html);
        Assert.Contains("data-rhx-dt-done", html);
    }

    [Fact]
    public async Task Empty_Value_Shows_Today_Month_No_Selection_Empty_Hidden()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("October 2026", html);
        Assert.DoesNotContain("aria-selected=\"true\"", html);
        Assert.Contains("value=\"\"", html);
    }

    [Fact]
    public async Task DateTime_Model_Binding_Produces_Iso_And_Selection()
    {
        var helper = CreateHelper();
        helper.For = Expr("StartsAt", new DateTime(2026, 10, 15, 14, 0, 0));
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.Contains("value=\"2026-10-15T14:00\"", html);
        Assert.Contains("data-time=\"14:00\"", html);
    }

    [Fact]
    public async Task Calendar_Pane_Hides_Calendar_Own_Footer()
    {
        var helper = CreateHelper();
        helper.Name = "d";
        var ctx = CreateContext("rhx-datetime-picker");
        var output = CreateOutput("rhx-datetime-picker");

        await helper.ProcessAsync(ctx, output);
        var html = output.Content.GetContent();

        Assert.DoesNotContain("data-rhx-cal-today", html);
        Assert.DoesNotContain("data-rhx-cal-clear", html);
    }
}
