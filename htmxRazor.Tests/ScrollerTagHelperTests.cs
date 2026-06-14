using htmxRazor.Components.Organization;
using Xunit;

namespace htmxRazor.Tests;

public class ScrollerTagHelperTests : TagHelperTestBase
{
    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    private ScrollerTagHelper CreateHelper()
    {
        var helper = new ScrollerTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    // ══════════════════════════════════════════════
    //  Structure
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Renders_Div_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public async Task Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-scroller"));
    }

    [Fact]
    public async Task Has_Scrollable_Scope_Attribute()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-scrollable", "");
    }

    [Fact]
    public async Task Contains_Viewport_With_Hook()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-scroller__viewport", content);
        Assert.Contains("data-rhx-scroll-viewport", content);
    }

    [Fact]
    public async Task Contains_Prev_Button()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-scroller__btn--prev", content);
        Assert.Contains("data-rhx-scroll-prev", content);
    }

    [Fact]
    public async Task Contains_Next_Button()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-scroller__btn--next", content);
        Assert.Contains("data-rhx-scroll-next", content);
    }

    [Fact]
    public async Task Buttons_Have_AriaLabels()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-label=\"Scroll back\"", content);
        Assert.Contains("aria-label=\"Scroll forward\"", content);
    }

    [Fact]
    public async Task Buttons_Are_Type_Button()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        var count = content.Split("type=\"button\"").Length - 1;
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task Renders_No_Custom_Scroller_Script_Hook()
    {
        // The legacy per-component JS hook must be gone; only the shared shim hooks remain.
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.False(output.Attributes.ContainsName("data-rhx-scroller"));
        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-scroller__shadow", content);
    }

    // ══════════════════════════════════════════════
    //  Orientation
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Default_Orientation_Horizontal()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-scroller--horizontal"));
        AssertAttribute(output, "data-rhx-orientation", "horizontal");
    }

    [Fact]
    public async Task Orientation_Vertical()
    {
        var helper = CreateHelper();
        helper.Orientation = "vertical";
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-scroller--vertical"));
        AssertAttribute(output, "data-rhx-orientation", "vertical");
    }

    [Fact]
    public async Task Vertical_Uses_Vertical_Aria_Labels()
    {
        var helper = CreateHelper();
        helper.Orientation = "vertical";
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-label=\"Scroll up\"", content);
        Assert.Contains("aria-label=\"Scroll down\"", content);
    }

    [Fact]
    public async Task Orientation_Both()
    {
        var helper = CreateHelper();
        helper.Orientation = "both";
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-scroller--both"));
        AssertAttribute(output, "data-rhx-orientation", "both");
    }

    [Fact]
    public async Task Orientation_Case_Insensitive()
    {
        var helper = CreateHelper();
        helper.Orientation = "Vertical";
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-scroller--vertical"));
        AssertAttribute(output, "data-rhx-orientation", "vertical");
    }

    // ══════════════════════════════════════════════
    //  Content ordering
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Prev_Before_Viewport_Before_Next()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        var prevIdx = html.IndexOf("data-rhx-scroll-prev");
        var viewportIdx = html.IndexOf("data-rhx-scroll-viewport");
        var nextIdx = html.IndexOf("data-rhx-scroll-next");
        Assert.True(prevIdx < viewportIdx);
        Assert.True(viewportIdx < nextIdx);
    }

    [Fact]
    public async Task Child_Content_Inside_Viewport()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "inner-content");

        await helper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        // Child content lands after the viewport opening tag.
        var viewportIdx = html.IndexOf("data-rhx-scroll-viewport");
        var contentIdx = html.IndexOf("inner-content");
        Assert.True(contentIdx > viewportIdx, "child content should be inside the viewport");
    }

    // ══════════════════════════════════════════════
    //  Custom CSS
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Custom_CssClass_Appended()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-scroller";
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "my-scroller"));
        Assert.True(HasClass(output, "rhx-scroller"));
    }

    // ══════════════════════════════════════════════
    //  htmx support
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Renders_Htmx_Attributes()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/items";
        helper.HxTrigger = "revealed";
        helper.HxTarget = "find .rhx-scroller__viewport";
        var context = CreateContext("rhx-scroller");
        var output = CreateOutput("rhx-scroller", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-get", "/api/items");
        AssertAttribute(output, "hx-trigger", "revealed");
        AssertAttribute(output, "hx-target", "find .rhx-scroller__viewport");
    }
}
