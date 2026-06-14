using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

public class TextareaTagHelperTests : TagHelperTestBase
{
    private TextareaTagHelper CreateHelper() => new(CreateUrlHelperFactory());

    // ── Element rendering ──

    [Fact]
    public void Renders_Div_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-textarea"));
    }

    [Fact]
    public void Renders_Native_Textarea()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.Contains("rhx-textarea__native", output.Content.GetContent());
    }

    // ── Structure ──

    [Fact]
    public void Contains_Control_Wrapper()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-textarea__control", content);
    }

    [Fact]
    public void Contains_Native_Textarea()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-textarea__native", content);
        Assert.Contains("<textarea", content);
        Assert.Contains("</textarea>", content);
    }

    [Fact]
    public void Default_Rows_Is_3()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rows=\"3\"", content);
    }

    // ── Name, Value ──

    [Fact]
    public void Name_Sets_Name_Attribute()
    {
        var helper = CreateHelper();
        helper.Name = "notes";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"notes\"", content);
    }

    [Fact]
    public void Value_Renders_As_Textarea_Content()
    {
        var helper = CreateHelper();
        helper.Value = "Hello world";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains(">Hello world</textarea>", content);
    }

    [Fact]
    public void Value_Is_Html_Encoded()
    {
        var helper = CreateHelper();
        helper.Value = "<script>alert(1)</script>";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("<script>", content);
        Assert.Contains("&lt;script&gt;", content);
    }

    // ── Placeholder ──

    [Fact]
    public void Placeholder_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Placeholder = "Enter notes...";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("placeholder=\"Enter notes...\"", content);
    }

    // ── Rows ──

    [Fact]
    public void Custom_Rows()
    {
        var helper = CreateHelper();
        helper.Rows = 8;
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rows=\"8\"", content);
    }

    // ── Resize ──

    [Fact]
    public void Default_Resize_No_Modifier()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        // Default is "vertical", no modifier added
        Assert.False(HasClass(output, "rhx-textarea--resize-vertical"));
    }

    [Fact]
    public void Resize_None_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Resize = "none";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-textarea--resize-none"));
    }

    [Fact]
    public void Resize_Auto_Adds_Modifier_And_Data_Attribute()
    {
        var helper = CreateHelper();
        helper.Resize = "auto";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-textarea--resize-auto"));
        var content = output.Content.GetContent();
        Assert.Contains("data-rhx-autosize", content);
    }

    // ── Label ──

    [Fact]
    public void Label_Renders_Label_Element()
    {
        var helper = CreateHelper();
        helper.Label = "Bio";
        helper.Name = "bio";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("<label", content);
        Assert.Contains("rhx-textarea__label", content);
        Assert.Contains("Bio", content);
    }

    // ── Hint ──

    [Fact]
    public void Hint_Renders_Hint_Span()
    {
        var helper = CreateHelper();
        helper.Hint = "Max 500 characters";
        helper.Name = "bio";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-textarea__hint", content);
        Assert.Contains("Max 500 characters", content);
    }

    // ── Constraints ──

    [Fact]
    public void Minlength_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Minlength = 10;
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("minlength=\"10\"", content);
    }

    [Fact]
    public void Maxlength_Sets_Attribute()
    {
        var helper = CreateHelper();
        helper.Maxlength = 500;
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("maxlength=\"500\"", content);
    }

    // ── Required ──

    [Fact]
    public void Required_Sets_Attributes()
    {
        var helper = CreateHelper();
        helper.Required = true;
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains(" required", content);
        Assert.Contains("aria-required=\"true\"", content);
    }

    // ── Disabled ──

    [Fact]
    public void Disabled_Adds_Modifier_And_Attribute()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-textarea--disabled"));
        var content = output.Content.GetContent();
        Assert.Contains(" disabled", content);
    }

    // ── Readonly ──

    [Fact]
    public void Readonly_Adds_Modifier_And_Attribute()
    {
        var helper = CreateHelper();
        helper.Readonly = true;
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-textarea--readonly"));
        var content = output.Content.GetContent();
        Assert.Contains(" readonly", content);
    }

    // ── Size ──

    [Fact]
    public void Small_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "small";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-textarea--small"));
    }

    [Fact]
    public void Large_Size_Adds_Modifier()
    {
        var helper = CreateHelper();
        helper.Size = "large";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-textarea--large"));
    }

    // ── Error state ──

    [Fact]
    public void Error_Adds_Modifier_And_Renders_Message()
    {
        var helper = CreateHelper();
        helper.Name = "bio";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        var viewContext = CreateViewContext();
        viewContext.ModelState.AddModelError("bio", "Bio is too long");
        helper.ViewContext = viewContext;
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-textarea--error"));
        var content = output.Content.GetContent();
        Assert.Contains("Bio is too long", content);
        Assert.Contains("aria-invalid=\"true\"", content);
    }

    // ── CSS class merging ──

    [Fact]
    public void Custom_Css_Class_Is_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-textarea";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-textarea"));
        Assert.True(HasClass(output, "my-textarea"));
    }

    // ── htmx ──

    [Fact]
    public void Htmx_Attributes_On_Native_Textarea()
    {
        var helper = CreateHelper();
        helper.HxPost = "/save";
        helper.HxSwap = "none";
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("hx-post=\"/save\"", content);
        Assert.Contains("hx-swap=\"none\"", content);
    }

    // ── Model binding ──

    [Fact]
    public void For_Resolves_Name_Id_And_Value()
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForProperty(typeof(TestModel), "Bio");
        var explorer = new ModelExplorer(provider, metadata, "My biography");

        var helper = CreateHelper();
        helper.For = new ModelExpression("Bio", explorer);
        var context = CreateContext("rhx-textarea");
        var output = CreateOutput("rhx-textarea");

        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("name=\"Bio\"", content);
        Assert.Contains("id=\"Bio\"", content);
        Assert.Contains(">My biography</textarea>", content);
    }

    private class TestModel
    {
        public string? Bio { get; set; }
    }
}
