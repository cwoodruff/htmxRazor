using htmxRazor.Components.Feedback;
using Xunit;

namespace htmxRazor.Tests;

public class CalloutTagHelperTests : TagHelperTestBase
{
    private CalloutTagHelper CreateHelper()
    {
        var helper = new CalloutTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    // ──────────────────────────────────────────────
    //  Default rendering
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Renders_Div_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public async Task Renders_Block_Class_And_Default_Variant()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-callout"));
        Assert.True(HasClass(output, "rhx-callout--neutral"));
    }

    [Fact]
    public async Task Renders_Role_Alert()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "role", "alert");
    }

    [Fact]
    public async Task Does_Not_Render_Js_Hook_Attribute()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-callout");
    }

    [Fact]
    public async Task Renders_Icon_Span()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-callout__icon", content);
        Assert.Contains("aria-hidden=\"true\"", content);
        Assert.Contains("<svg", content);
    }

    [Fact]
    public async Task Renders_Content_Wrapper()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Test message");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-callout__content", content);
        Assert.Contains("Test message", content);
    }

    [Fact]
    public async Task Default_Has_No_Close_Button()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-callout__close", content);
    }

    [Fact]
    public async Task Default_Not_Hidden()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "hidden");
    }

    [Fact]
    public async Task Default_No_Duration_Attribute()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-duration");
    }

    // ──────────────────────────────────────────────
    //  Variants
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("neutral", "rhx-callout--neutral")]
    [InlineData("brand", "rhx-callout--brand")]
    [InlineData("success", "rhx-callout--success")]
    [InlineData("warning", "rhx-callout--warning")]
    [InlineData("danger", "rhx-callout--danger")]
    public async Task Renders_Correct_Variant_Class(string variant, string expectedClass)
    {
        var helper = CreateHelper();
        helper.Variant = variant;
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, expectedClass));
    }

    [Fact]
    public async Task Variant_Is_Case_Insensitive()
    {
        var helper = CreateHelper();
        helper.Variant = "Success";
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-callout--success"));
    }

    // ──────────────────────────────────────────────
    //  Variant-appropriate icons
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("neutral")]
    [InlineData("brand")]
    public async Task Neutral_And_Brand_Use_Info_Circle_Icon(string variant)
    {
        var helper = CreateHelper();
        helper.Variant = variant;
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        // Info circle has a circle + two lines for "i"
        Assert.Contains("<circle cx=\"12\" cy=\"12\" r=\"10\"", content);
    }

    [Fact]
    public async Task Success_Uses_Check_Circle_Icon()
    {
        var helper = CreateHelper();
        helper.Variant = "success";
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("<polyline", content);
    }

    [Fact]
    public async Task Warning_Uses_Exclamation_Triangle_Icon()
    {
        var helper = CreateHelper();
        helper.Variant = "warning";
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        // Exclamation triangle has a distinct path for the triangle shape
        Assert.Contains("L1.82 18", content);
    }

    [Fact]
    public async Task Danger_Uses_Exclamation_Circle_Icon()
    {
        var helper = CreateHelper();
        helper.Variant = "danger";
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        // Exclamation circle has a circle + lines at y=8/12 and y=16 (different from info which has y=16/12 and y=8)
        Assert.Contains("x1=\"12\" y1=\"8\" x2=\"12\" y2=\"12\"", content);
        Assert.Contains("x1=\"12\" y1=\"16\"", content);
    }

    // ──────────────────────────────────────────────
    //  Close button
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Closable_Renders_Css_Dismiss_Control()
    {
        var helper = CreateHelper();
        helper.Closable = true;
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        // CSS-only dismiss: a focusable checkbox + a label styled as the close button.
        Assert.Contains("rhx-callout__dismiss", content);
        Assert.Contains("type=\"checkbox\"", content);
        Assert.Contains("aria-label=\"Close\"", content);
        Assert.Contains("rhx-callout__close", content);
    }

    // ──────────────────────────────────────────────
    //  Open/Hidden state
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Open_False_Sets_Hidden()
    {
        var helper = CreateHelper();
        helper.Open = false;
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hidden", "hidden");
    }

    // ──────────────────────────────────────────────
    //  Duration
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Duration_Sets_AutoDismiss_Animation()
    {
        var helper = CreateHelper();
        helper.Duration = 5000;
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var style = output.Attributes["style"]?.Value?.ToString() ?? "";
        Assert.Contains("rhx-callout-auto-dismiss", style);
        Assert.Contains("5000ms", style);
    }

    [Fact]
    public async Task Duration_Zero_No_Animation()
    {
        var helper = CreateHelper();
        helper.Duration = 0;
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-duration");
        var style = output.Attributes["style"]?.Value?.ToString() ?? "";
        Assert.DoesNotContain("auto-dismiss", style);
    }

    // ──────────────────────────────────────────────
    //  Custom icon
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Custom_Icon_Overrides_Default()
    {
        var helper = CreateHelper();
        helper.Variant = "success";
        helper.Icon = "exclamation-triangle";
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        // Should use exclamation triangle, not check circle
        Assert.Contains("L1.82 18", content);
        Assert.DoesNotContain("<polyline", content);
    }

    // ──────────────────────────────────────────────
    //  Custom CSS class
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Custom_CssClass_Appended()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-alert";
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "my-alert"));
        Assert.True(HasClass(output, "rhx-callout"));
    }

    // ──────────────────────────────────────────────
    //  htmx attributes
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HxGet_Renders()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/status";
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-get", "/api/status");
    }

    [Fact]
    public async Task Null_Htmx_Attributes_Not_Rendered()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-callout");
        var output = CreateOutput("rhx-callout", childContent: "Hello");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "hx-get");
        AssertNoAttribute(output, "hx-post");
    }
}
