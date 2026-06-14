using htmxRazor.Components.Imagery;
using Xunit;

namespace htmxRazor.Tests;

public class ComparisonTagHelperTests : TagHelperTestBase
{
    private ComparisonTagHelper CreateHelper()
    {
        var helper = new ComparisonTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    private ComparisonTagHelper Configured()
    {
        var helper = CreateHelper();
        helper.Before = "before.jpg";
        helper.BeforeAlt = "Before shot";
        helper.After = "after.jpg";
        helper.AfterAlt = "After shot";
        return helper;
    }

    // ══════════════════════════════════════════════
    //  Structure
    // ══════════════════════════════════════════════

    [Fact]
    public void Renders_Div_Element()
    {
        var helper = Configured();
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);
        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Has_Block_Class()
    {
        var helper = Configured();
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);
        Assert.True(HasClass(output, "rhx-comparison"));
    }

    [Fact]
    public void Does_Not_Render_Js_Hook_Attribute()
    {
        var helper = Configured();
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);
        AssertNoAttribute(output, "data-rhx-comparison");
    }

    [Fact]
    public void Contains_Before_Image()
    {
        var helper = Configured();
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-comparison__before", content);
        Assert.Contains("src=\"before.jpg\"", content);
        Assert.Contains("alt=\"Before shot\"", content);
    }

    [Fact]
    public void Contains_After_Image()
    {
        var helper = Configured();
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-comparison__after", content);
        Assert.Contains("src=\"after.jpg\"", content);
        Assert.Contains("alt=\"After shot\"", content);
    }

    [Fact]
    public void Contains_Decorative_Handle()
    {
        var helper = Configured();
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-comparison__handle", content);
        Assert.Contains("rhx-comparison__handle-grip", content);
        Assert.Contains("rhx-comparison__handle-line", content);
        // The handle is now a decorative element; no slider role/tabindex (the native
        // resize grip drives the interaction).
        Assert.DoesNotContain("role=\"slider\"", content);
    }

    // ══════════════════════════════════════════════
    //  Position → after-panel width
    // ══════════════════════════════════════════════

    [Fact]
    public void Default_Position_50_Sizes_After()
    {
        var helper = Configured();
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);
        Assert.Contains("width: 50%", output.Content.GetContent());
    }

    [Fact]
    public void Custom_Position_Sizes_After()
    {
        var helper = Configured();
        helper.Position = 75;
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);
        Assert.Contains("width: 75%", output.Content.GetContent());
    }

    [Fact]
    public void Position_Clamped_Min()
    {
        var helper = Configured();
        helper.Position = -20;
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);
        Assert.Contains("width: 0%", output.Content.GetContent());
    }

    [Fact]
    public void Position_Clamped_Max()
    {
        var helper = Configured();
        helper.Position = 150;
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);
        Assert.Contains("width: 100%", output.Content.GetContent());
    }

    [Fact]
    public void Before_Then_After_Order()
    {
        var helper = Configured();
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);

        var html = output.Content.GetContent();
        var beforeIdx = html.IndexOf("rhx-comparison__before");
        var afterIdx = html.IndexOf("rhx-comparison__after");
        Assert.True(beforeIdx >= 0 && afterIdx >= 0);
        Assert.True(beforeIdx < afterIdx);
    }

    [Fact]
    public void Renders_Htmx_Attributes()
    {
        var helper = Configured();
        helper.HxGet = "/api/compare";
        var output = CreateOutput("rhx-comparison");
        helper.Process(CreateContext("rhx-comparison"), output);
        AssertAttribute(output, "hx-get", "/api/compare");
    }
}
