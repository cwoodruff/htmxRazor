using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

public class RatingTagHelperTests : TagHelperTestBase
{
    private RatingTagHelper CreateHelper() => new(CreateUrlHelperFactory());

    // ── Test model ──

    private class TestModel
    {
        public int Score { get; set; }
        public double Rating { get; set; }
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
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-rating"));
    }

    [Fact]
    public void No_Legacy_Js_Hook_Attributes()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        AssertNoAttribute(output, "data-rhx-rating");
        AssertNoAttribute(output, "data-rhx-precision");
        AssertNoAttribute(output, "data-rhx-max");
        AssertNoAttribute(output, "role"); // no role="slider" on the wrapper
        AssertNoAttribute(output, "tabindex");

        var content = output.Content.GetContent();
        Assert.DoesNotContain("data-value", content);
        Assert.DoesNotContain("data-hover", content);
        Assert.DoesNotContain("aria-valuenow", content);
    }

    [Fact]
    public void Optimistic_Emits_Data_Attribute()
    {
        var helper = CreateHelper();
        helper.Optimistic = true;
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        AssertAttribute(output, "data-rhx-optimistic", "");
    }

    // ── Interactive mode: radiogroup + radios ──

    [Fact]
    public void Interactive_Renders_Radiogroup()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("role=\"radiogroup\"", content);
    }

    [Fact]
    public void Interactive_Renders_Default_5_Radios()
    {
        var helper = CreateHelper();
        helper.Name = "rating";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        var radioCount = content.Split("type=\"radio\"").Length - 1;
        Assert.Equal(5, radioCount);
        Assert.Contains("value=\"1\"", content);
        Assert.Contains("value=\"5\"", content);
        Assert.Contains("rhx-rating__input", content);
        Assert.Contains("rhx-sr-only", content);
    }

    [Fact]
    public void Interactive_Radios_In_Reverse_Order()
    {
        var helper = CreateHelper();
        helper.Name = "rating";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        // First radio emitted is value="5" (Max), last is value="1".
        Assert.True(content.IndexOf("value=\"5\"") < content.IndexOf("value=\"1\""));
    }

    [Fact]
    public void Interactive_Has_Star_Labels_With_Svg()
    {
        var helper = CreateHelper();
        helper.Name = "rating";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("<label class=\"rhx-rating__star\"", content);
        Assert.Contains("<polygon", content);
    }

    [Fact]
    public void Custom_Max_Renders_That_Many_Radios()
    {
        var helper = CreateHelper();
        helper.Name = "rating";
        helper.Max = 10;
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        var radioCount = content.Split("type=\"radio\"").Length - 1;
        Assert.Equal(10, radioCount);
        Assert.Contains("value=\"10\"", content);
    }

    [Fact]
    public void Value_Checks_Matching_Radio()
    {
        var helper = CreateHelper();
        helper.Name = "rating";
        helper.Value = "3";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("value=\"3\" checked", content);
    }

    [Fact]
    public void Required_On_Radios()
    {
        var helper = CreateHelper();
        helper.Name = "rating";
        helper.Required = true;
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("required", content);
    }

    // ── Readonly / disabled: static stars + hidden input ──

    [Fact]
    public void Readonly_Renders_Static_Stars_And_Hidden_Input()
    {
        var helper = CreateHelper();
        helper.Name = "rating";
        helper.Value = "3";
        helper.Readonly = true;
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-rating--readonly"));

        var content = output.Content.GetContent();
        Assert.DoesNotContain("type=\"radio\"", content);
        Assert.DoesNotContain("role=\"radiogroup\"", content);
        Assert.Contains("<span class=\"rhx-rating__star", content);
        Assert.Contains("type=\"hidden\"", content);
        Assert.Contains("rhx-rating__value", content);
        Assert.Contains("name=\"rating\"", content);
        Assert.Contains("value=\"3\"", content);
    }

    [Fact]
    public void Readonly_Filled_Stars_Have_Modifier()
    {
        var helper = CreateHelper();
        helper.Value = "3";
        helper.Readonly = true;
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        var filledCount = content.Split("rhx-rating__star--filled").Length - 1;
        Assert.Equal(3, filledCount);
    }

    [Fact]
    public void Readonly_Half_Star_With_Precision()
    {
        var helper = CreateHelper();
        helper.Value = "2.5";
        helper.Precision = 0.5;
        helper.Readonly = true;
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-rating__star--half", content);
    }

    [Fact]
    public void Disabled_Renders_Static_Stars()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        helper.Value = "2";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-rating--disabled"));

        var content = output.Content.GetContent();
        Assert.DoesNotContain("type=\"radio\"", content);
        Assert.Contains("<span class=\"rhx-rating__star", content);
        Assert.Contains("type=\"hidden\"", content);
    }

    // ── Label ──

    [Fact]
    public void Label_Renders()
    {
        var helper = CreateHelper();
        helper.Label = "Rating";
        helper.Name = "rating";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("<label", content);
        Assert.Contains("Rating", content);
    }

    // ── Error ──

    [Fact]
    public void Error_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Name = "rating";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        var viewContext = CreateViewContext();
        viewContext.ModelState.AddModelError("rating", "Rating is required");
        helper.ViewContext = viewContext;
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-rating--error"));
    }

    // ── Size ──

    [Fact]
    public void Small_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "small";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-rating--small"));
    }

    [Fact]
    public void Large_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "large";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-rating--large"));
    }

    // ── Model binding ──

    [Fact]
    public void For_Resolves_Name_And_Checks_Value()
    {
        var helper = CreateHelper();
        helper.For = CreateModelExpressionFor("Score", 4);
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"Score\"", content);
        Assert.Contains("value=\"4\" checked", content);
    }

    // ── htmx ──

    [Fact]
    public void Htmx_On_Interactive_Radios()
    {
        var helper = CreateHelper();
        helper.Name = "rating";
        helper.HxPost = "/rate";
        helper.HxTrigger = "change";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("hx-post=\"/rate\"", content);
        Assert.Contains("hx-trigger=\"change\"", content);
    }

    [Fact]
    public void Htmx_On_Readonly_Hidden_Input()
    {
        var helper = CreateHelper();
        helper.Name = "rating";
        helper.Readonly = true;
        helper.HxPost = "/rate";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("hx-post=\"/rate\"", content);
    }

    // ── CSS class merging ──

    [Fact]
    public void Custom_Css_Class_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-rating";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-rating"));
        Assert.True(HasClass(output, "my-rating"));
    }

    // ── ARIA label on radiogroup ──

    [Fact]
    public void Aria_Label_On_Radiogroup()
    {
        var helper = CreateHelper();
        helper.AriaLabel = "Product rating";
        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("role=\"radiogroup\"", content);
        Assert.Contains("aria-label=\"Product rating\"", content);
    }
}
