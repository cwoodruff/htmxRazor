using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

// rhx-color-picker renders a native <input type="color"> (no JS). The browser supplies the
// color-selection UI; binding is via a hex #rrggbb value.
public class ColorPickerTagHelperTests : TagHelperTestBase
{
    private ColorPickerTagHelper CreateHelper() => new(CreateUrlHelperFactory());

    private class TestModel
    {
        public string ThemeColor { get; set; } = "#3b82f6";
    }

    private static ModelExpression CreateModelExpressionFor(string propertyName, object? value = null)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), propertyName);
        var explorer = new ModelExplorer(provider, metadata, value);
        return new ModelExpression(propertyName, explorer);
    }

    private string Render(ColorPickerTagHelper helper)
    {
        var context = CreateContext("rhx-color-picker");
        var output = CreateOutput("rhx-color-picker");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);
        return output.Content.GetContent();
    }

    [Fact]
    public void Renders_Div_Wrapper_With_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-color-picker");
        var output = CreateOutput("rhx-color-picker");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-color-picker"));
    }

    [Fact]
    public void Renders_Native_Color_Input()
    {
        var helper = CreateHelper();
        helper.Name = "color";
        helper.Value = "#3b82f6";
        var html = Render(helper);
        Assert.Contains("type=\"color\"", html);
        Assert.Contains("rhx-color-picker__control", html);
        Assert.Contains("name=\"color\"", html);
        Assert.Contains("value=\"#3b82f6\"", html);
    }

    [Fact]
    public void Default_Value_Is_Black()
    {
        var helper = CreateHelper();
        helper.Name = "color";
        var html = Render(helper);
        Assert.Contains("value=\"#000000\"", html);
    }

    [Fact]
    public void Non_Hex_Value_Falls_Back_To_Black()
    {
        // Native color inputs are hex-only; a non-#rrggbb value is normalized to black.
        var helper = CreateHelper();
        helper.Name = "color";
        helper.Value = "rgb(255,0,0)";
        var html = Render(helper);
        Assert.Contains("value=\"#000000\"", html);
    }

    [Fact]
    public void Label_Renders_For_The_Input()
    {
        var helper = CreateHelper();
        helper.Label = "Brand Color";
        helper.Name = "color";
        var html = Render(helper);
        Assert.Contains("<label", html);
        Assert.Contains("Brand Color", html);
        Assert.Contains("for=\"color\"", html);
    }

    [Fact]
    public void Error_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Name = "color";
        var context = CreateContext("rhx-color-picker");
        var output = CreateOutput("rhx-color-picker");
        var vc = CreateViewContext();
        vc.ModelState.AddModelError("color", "Invalid color");
        helper.ViewContext = vc;
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-color-picker--error"));
        Assert.Contains("aria-invalid=\"true\"", output.Content.GetContent());
    }

    [Fact]
    public void Disabled_Adds_Modifier_And_Native_Attribute()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-color-picker");
        var output = CreateOutput("rhx-color-picker");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-color-picker--disabled"));
        Assert.Contains(" disabled", output.Content.GetContent());
    }

    [Fact]
    public void Required_Adds_Native_Required()
    {
        var helper = CreateHelper();
        helper.Name = "color";
        helper.Required = true;
        var html = Render(helper);
        Assert.Contains(" required", html);
    }

    [Fact]
    public void Small_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "small";
        var context = CreateContext("rhx-color-picker");
        var output = CreateOutput("rhx-color-picker");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);
        Assert.True(HasClass(output, "rhx-color-picker--small"));
    }

    [Fact]
    public void For_Resolves_Name_And_Value()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("ThemeColor", "#3b82f6");
        var html = Render(helper);
        Assert.Contains("name=\"ThemeColor\"", html);
        Assert.Contains("value=\"#3b82f6\"", html);
    }

    [Fact]
    public void Htmx_On_Color_Input()
    {
        var helper = CreateHelper();
        helper.HxPost = "/color";
        helper.HxTrigger = "change";
        var html = Render(helper);
        Assert.Contains("hx-post=\"/color\"", html);
        Assert.Contains("hx-trigger=\"change\"", html);
    }

    [Fact]
    public void Custom_Css_Class_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-picker";
        var context = CreateContext("rhx-color-picker");
        var output = CreateOutput("rhx-color-picker");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);
        Assert.True(HasClass(output, "rhx-color-picker"));
        Assert.True(HasClass(output, "my-picker"));
    }
}
