using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

public class NumberInputTagHelperTests : TagHelperTestBase
{
    private NumberInputTagHelper CreateHelper() => new(CreateUrlHelperFactory());

    // ── Element rendering ──

    [Fact]
    public void Renders_Div_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-number-input"));
    }

    [Fact]
    public void Renders_Native_Number_Input()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-number-input__native", content);
        Assert.Contains("type=\"number\"", content);
    }

    // ── Structure ──

    [Fact]
    public void Contains_Control_Wrapper()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-number-input__control", content);
    }

    [Fact]
    public void Contains_Native_Number_Input()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-number-input__native", content);
        Assert.Contains("type=\"number\"", content);
    }

    [Fact]
    public void Uses_Native_Spinners_No_Custom_Buttons()
    {
        // The browser's own number spinners replace the old custom +/- buttons (no JS).
        var helper = CreateHelper();
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-number-input__decrement", content);
        Assert.DoesNotContain("rhx-number-input__increment", content);
    }

    // ── No steppers ──

    [Fact]
    public void NoSteppers_Adds_Modifier()
    {
        // --no-steppers hides the browser's native spinners via CSS.
        var helper = CreateHelper();
        helper.NoSteppers = true;
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-number-input--no-steppers"));
    }

    // ── Name, Value ──

    [Fact]
    public void Name_And_Value_Set_On_Native()
    {
        var helper = CreateHelper();
        helper.Name = "quantity";
        helper.Value = "5";
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"quantity\"", content);
        Assert.Contains("value=\"5\"", content);
    }

    // ── Min, Max, Step ──

    [Fact]
    public void Min_Max_Step_Set_Attributes()
    {
        var helper = CreateHelper();
        helper.Min = "0";
        helper.Max = "99";
        helper.Step = "1";
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("min=\"0\"", content);
        Assert.Contains("max=\"99\"", content);
        Assert.Contains("step=\"1\"", content);
    }

    // ── Label ──

    [Fact]
    public void Label_Renders_Label_Element()
    {
        var helper = CreateHelper();
        helper.Label = "Quantity";
        helper.Name = "quantity";
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("<label", content);
        Assert.Contains("rhx-number-input__label", content);
        Assert.Contains("Quantity", content);
    }

    // ── Hint ──

    [Fact]
    public void Hint_Renders_Hint_Span()
    {
        var helper = CreateHelper();
        helper.Hint = "Enter a value between 0 and 99";
        helper.Name = "qty";
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-number-input__hint", content);
        Assert.Contains("Enter a value between 0 and 99", content);
    }

    // ── Required ──

    [Fact]
    public void Required_Sets_Attributes()
    {
        var helper = CreateHelper();
        helper.Required = true;
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains(" required", content);
        Assert.Contains("aria-required=\"true\"", content);
    }

    // ── Disabled ──

    [Fact]
    public void Disabled_Adds_Modifier_And_Disables_Steppers()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-number-input--disabled"));
        var content = output.Content.GetContent();
        Assert.Contains(" disabled", content);
    }

    // ── Readonly ──

    [Fact]
    public void Readonly_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Readonly = true;
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-number-input--readonly"));
        var content = output.Content.GetContent();
        Assert.Contains(" readonly", content);
    }

    // ── Size ──

    [Fact]
    public void Default_Size_No_Modifier()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.False(HasClass(output, "rhx-number-input--medium"));
    }

    [Fact]
    public void Small_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "small";
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-number-input--small"));
    }

    // ── Error state ──

    [Fact]
    public void Error_Adds_Modifier_And_Renders_Message()
    {
        var helper = CreateHelper();
        helper.Name = "quantity";
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        var viewContext = CreateViewContext();
        viewContext.ModelState.AddModelError("quantity", "Must be positive");
        helper.ViewContext = viewContext;
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-number-input--error"));
        var content = output.Content.GetContent();
        Assert.Contains("Must be positive", content);
        Assert.Contains("aria-invalid=\"true\"", content);
    }

    // ── CSS class merging ──

    [Fact]
    public void Custom_Css_Class_Is_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-number";
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-number-input"));
        Assert.True(HasClass(output, "my-number"));
    }

    // ── htmx ──

    [Fact]
    public void Htmx_Attributes_On_Native_Input()
    {
        var helper = CreateHelper();
        helper.HxPost = "/update";
        helper.HxTrigger = "change";
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("hx-post=\"/update\"", content);
        Assert.Contains("hx-trigger=\"change\"", content);
    }

    // ── Model binding ──

    [Fact]
    public void For_Resolves_Name_And_Id()
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), "Quantity");
        var explorer = new ModelExplorer(provider, metadata, 10);

        var helper = CreateHelper();
        helper.For = new ModelExpression("Quantity", explorer);
        var context = CreateContext("rhx-number-input");
        var output = CreateOutput("rhx-number-input");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"Quantity\"", content);
        Assert.Contains("id=\"Quantity\"", content);
        Assert.Contains("value=\"10\"", content);
    }

    private class TestModel
    {
        public int Quantity { get; set; }
    }
}
