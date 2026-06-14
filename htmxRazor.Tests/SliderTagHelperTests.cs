using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

public class SliderTagHelperTests : TagHelperTestBase
{
    private SliderTagHelper CreateHelper() => new(CreateUrlHelperFactory());

    // ── Test model ──

    private class TestModel
    {
        public int Volume { get; set; }
        public double Brightness { get; set; }
    }

    private static ModelExpression CreateModelExpressionFor(string propertyName, object? value = null)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), propertyName);
        var explorer = new ModelExplorer(provider, metadata, value);
        return new ModelExpression(propertyName, explorer);
    }

    // ── Element rendering ──

    [Fact]
    public void Renders_Div_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-slider"));
    }

    // ── Native range input ──

    [Fact]
    public void Has_Native_Range_Input()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("type=\"range\"", content);
        Assert.Contains("rhx-slider__input", content);
    }

    [Fact]
    public void Has_No_Fill_Bar()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-slider__fill", content);
    }

    [Fact]
    public void Has_No_Js_Hooks()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        AssertNoAttribute(output, "data-rhx-slider");
    }

    [Fact]
    public void Default_Min_Max_Step()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("min=\"0\"", content);
        Assert.Contains("max=\"100\"", content);
        Assert.Contains("step=\"1\"", content);
    }

    [Fact]
    public void Custom_Min_Max_Step()
    {
        var helper = CreateHelper();
        helper.Min = "10";
        helper.Max = "200";
        helper.Step = "5";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("min=\"10\"", content);
        Assert.Contains("max=\"200\"", content);
        Assert.Contains("step=\"5\"", content);
    }

    // ── Name, Id, Value ──

    [Fact]
    public void Name_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Name = "volume";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"volume\"", content);
    }

    [Fact]
    public void Value_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Value = "75";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("value=\"75\"", content);
    }

    // ── Show value ──

    [Fact]
    public void No_Value_Output_By_Default()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("<output", content);
        Assert.DoesNotContain("rhx-slider__value", content);
    }

    [Fact]
    public void Show_Value_Renders_Static_Output()
    {
        var helper = CreateHelper();
        helper.ShowValue = true;
        helper.Value = "50";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-slider__value", content);
        Assert.Contains(">50</output>", content);
    }

    // ── Label ──

    [Fact]
    public void Label_Renders()
    {
        var helper = CreateHelper();
        helper.Label = "Volume";
        helper.Name = "volume";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("<label", content);
        Assert.Contains("rhx-slider__label", content);
        Assert.Contains("Volume", content);
    }

    // ── Hint ──

    [Fact]
    public void Hint_Renders()
    {
        var helper = CreateHelper();
        helper.Hint = "Adjust volume level";
        helper.Name = "volume";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-slider__hint", content);
        Assert.Contains("Adjust volume level", content);
    }

    // ── Error ──

    [Fact]
    public void Error_Adds_Modifier_And_Message()
    {
        var helper = CreateHelper();
        helper.Name = "volume";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        var viewContext = CreateViewContext();
        viewContext.ModelState.AddModelError("volume", "Volume is required");
        helper.ViewContext = viewContext;
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-slider--error"));
        var content = output.Content.GetContent();
        Assert.Contains("Volume is required", content);
        Assert.Contains("aria-invalid=\"true\"", content);
    }

    // ── States ──

    [Fact]
    public void Disabled_Adds_Modifier_And_Attribute()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-slider--disabled"));
        var content = output.Content.GetContent();
        Assert.Contains(" disabled", content);
    }

    [Fact]
    public void Required_Sets_Attributes()
    {
        var helper = CreateHelper();
        helper.Required = true;
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains(" required", content);
        Assert.Contains("aria-required=\"true\"", content);
    }

    // ── Size ──

    [Fact]
    public void Small_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "small";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-slider--small"));
    }

    [Fact]
    public void Large_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "large";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-slider--large"));
    }

    // ── Model binding ──

    [Fact]
    public void For_Resolves_Name_And_Value()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Volume", 75);
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"Volume\"", content);
        Assert.Contains("value=\"75\"", content);
    }

    // ── htmx ──

    [Fact]
    public void Htmx_On_Native_Input()
    {
        var helper = CreateHelper();
        helper.HxPost = "/settings";
        helper.HxTrigger = "change";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("hx-post=\"/settings\"", content);
        Assert.Contains("hx-trigger=\"change\"", content);
    }

    // ── CSS class merging ──

    [Fact]
    public void Custom_Css_Class_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-slider";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-slider"));
        Assert.True(HasClass(output, "my-slider"));
    }

    // ── ARIA label ──

    [Fact]
    public void Aria_Label_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.AriaLabel = "Volume control";
        var context = CreateContext("rhx-slider");
        var output = CreateOutput("rhx-slider");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-label=\"Volume control\"", content);
    }
}
