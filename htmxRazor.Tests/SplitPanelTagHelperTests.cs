using htmxRazor.Components.Organization;
using htmxRazor.Rendering;
using Xunit;

namespace htmxRazor.Tests;

public class SplitPanelTagHelperTests : TagHelperTestBase
{
    private SplitPanelTagHelper CreateHelper()
    {
        var helper = new SplitPanelTagHelper(CreateUrlHelperFactory());
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
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public async Task Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-split-panel"));
    }

    [Fact]
    public async Task Emits_No_Js_Hook_Attributes()
    {
        var helper = CreateHelper();
        helper.Position = 30;
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-split-panel");
        AssertNoAttribute(output, "data-rhx-position");
    }

    [Fact]
    public async Task Default_Position_50_Sizes_Start_Panel()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("flex-basis: 50%", content);
        Assert.Contains("width: 50%", content);
    }

    [Fact]
    public async Task Custom_Position_Sizes_Start_Panel()
    {
        var helper = CreateHelper();
        helper.Position = 30;
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("flex-basis: 30%", content);
        Assert.Contains("width: 30%", content);
    }

    [Fact]
    public async Task Contains_Start_And_End_Panels()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-split-panel__start", content);
        Assert.Contains("rhx-split-panel__end", content);
    }

    [Fact]
    public async Task Contains_Divider_With_Separator_Role()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-split-panel__divider", content);
        Assert.Contains("role=\"separator\"", content);
        Assert.Contains("rhx-split-panel__divider-handle", content);
    }

    [Fact]
    public async Task Default_Horizontal_Orientation()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.Contains("aria-orientation=\"vertical\"", output.Content.GetContent());
        Assert.False(HasClass(output, "rhx-split-panel--vertical"));
    }

    // ══════════════════════════════════════════════
    //  Vertical
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Vertical_Modifier_And_Orientation_And_Height()
    {
        var helper = CreateHelper();
        helper.Vertical = true;
        helper.Position = 40;
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-split-panel--vertical"));
        var content = output.Content.GetContent();
        Assert.Contains("aria-orientation=\"horizontal\"", content);
        Assert.Contains("height: 40%", content);
    }

    // ══════════════════════════════════════════════
    //  Disabled
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Disabled_Modifier()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-split-panel--disabled"));
    }

    // ══════════════════════════════════════════════
    //  Custom CSS & htmx
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Custom_CssClass_Appended()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-split";
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "my-split"));
        Assert.True(HasClass(output, "rhx-split-panel"));
    }

    [Fact]
    public async Task Renders_Htmx_Attributes()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/panel";
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-get", "/api/panel");
    }

    // ══════════════════════════════════════════════
    //  Slot children
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Slot_Content_Rendered_In_Order()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-split-panel");

        var output = new Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput(
            tagName: "rhx-split-panel",
            attributes: new Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttributeList(),
            getChildContentAsync: (useCachedResult, encoder) =>
            {
                var slots = SlotRenderer.FromContext(context)!;
                slots.SetHtml("start", "<p>Start</p>");
                slots.SetHtml("end", "<p>End</p>");
                var content = new Microsoft.AspNetCore.Razor.TagHelpers.DefaultTagHelperContent();
                return Task.FromResult<Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContent>(content);
            });

        await helper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.Contains("<p>Start</p>", html);
        Assert.Contains("<p>End</p>", html);
        var startIdx = html.IndexOf("rhx-split-panel__start");
        var dividerIdx = html.IndexOf("rhx-split-panel__divider");
        var endIdx = html.IndexOf("rhx-split-panel__end");
        Assert.True(startIdx < dividerIdx, "Start should appear before divider");
        Assert.True(dividerIdx < endIdx, "Divider should appear before end");
    }

    [Fact]
    public async Task Start_Registers_Slot()
    {
        var startHelper = new SplitPanelStartTagHelper();
        var context = CreateContext("rhx-split-start");
        var slots = SlotRenderer.CreateForContext(context);
        var output = CreateOutput("rhx-split-start", childContent: "Sidebar");

        await startHelper.ProcessAsync(context, output);

        Assert.True(slots.Has("start"));
    }

    [Fact]
    public async Task End_Registers_Slot()
    {
        var endHelper = new SplitPanelEndTagHelper();
        var context = CreateContext("rhx-split-end");
        var slots = SlotRenderer.CreateForContext(context);
        var output = CreateOutput("rhx-split-end", childContent: "Main");

        await endHelper.ProcessAsync(context, output);

        Assert.True(slots.Has("end"));
    }

    // ══════════════════════════════════════════════
    //  Position clamping
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Position_Clamped_To_0()
    {
        var helper = CreateHelper();
        helper.Position = -10;
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.Contains("flex-basis: 0%", output.Content.GetContent());
    }

    [Fact]
    public async Task Position_Clamped_To_100()
    {
        var helper = CreateHelper();
        helper.Position = 150;
        var context = CreateContext("rhx-split-panel");
        var output = CreateOutput("rhx-split-panel", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.Contains("flex-basis: 100%", output.Content.GetContent());
    }
}
