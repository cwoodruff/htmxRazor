using htmxRazor.Components.Utilities;
using Xunit;

namespace htmxRazor.Tests;

public class PopoverTagHelperTests : TagHelperTestBase
{
    private PopoverTagHelper CreateHelper()
    {
        return new PopoverTagHelper { ViewContext = CreateViewContext() };
    }

    private static async Task<TagHelperOutputAccess> RenderAsync(PopoverTagHelper helper)
    {
        var ctx = CreateContext("rhx-popover");
        var output = CreateOutput("rhx-popover", childContent: "Content");
        await helper.ProcessAsync(ctx, output);
        return new TagHelperOutputAccess(output);
    }

    private sealed class TagHelperOutputAccess
    {
        public Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput Output { get; }
        public string Content { get; }
        public string Style { get; }
        public TagHelperOutputAccess(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput o)
        {
            Output = o;
            Content = o.Content.GetContent();
            Style = o.Attributes.TryGetAttribute("style", out var s) ? s.Value?.ToString() ?? "" : "";
        }
    }

    // ── Structure ──

    [Fact]
    public async Task Renders_Div_Element()
    {
        var output = CreateOutput("rhx-popover", childContent: "<p>Content</p>");
        await CreateHelper().ProcessAsync(CreateContext("rhx-popover"), output);
        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public async Task Has_Block_Class()
    {
        var output = CreateOutput("rhx-popover", childContent: "Content");
        await CreateHelper().ProcessAsync(CreateContext("rhx-popover"), output);
        Assert.True(HasClass(output, "rhx-popover"));
    }

    [Fact]
    public async Task Is_Native_Popover()
    {
        var output = CreateOutput("rhx-popover", childContent: "Content");
        await CreateHelper().ProcessAsync(CreateContext("rhx-popover"), output);
        AssertAttribute(output, "popover", "auto");
        // No JS-era hooks.
        AssertNoAttribute(output, "data-rhx-popover");
        AssertNoAttribute(output, "data-rhx-trigger");
        AssertNoAttribute(output, "hidden");
        AssertNoAttribute(output, "aria-hidden");
    }

    [Fact]
    public async Task Has_Generated_Id()
    {
        var output = CreateOutput("rhx-popover", childContent: "Content");
        await CreateHelper().ProcessAsync(CreateContext("rhx-popover"), output);
        var id = GetAttribute(output, "id");
        Assert.NotNull(id);
        Assert.StartsWith("rhx-popover-", id);
    }

    [Fact]
    public async Task Custom_Id_Overrides_Generated()
    {
        var helper = CreateHelper();
        helper.Id = "my-popover";
        var output = CreateOutput("rhx-popover", childContent: "Content");
        await helper.ProcessAsync(CreateContext("rhx-popover"), output);
        AssertAttribute(output, "id", "my-popover");
        // The anchor name follows the id so the trigger can position against it.
        var style = GetAttribute(output, "style") ?? "";
        Assert.Contains("position-anchor:--my-popover", style);
    }

    [Fact]
    public async Task Custom_Class_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "custom-pop";
        var output = CreateOutput("rhx-popover", childContent: "Content");
        await helper.ProcessAsync(CreateContext("rhx-popover"), output);
        Assert.True(HasClass(output, "rhx-popover"));
        Assert.True(HasClass(output, "custom-pop"));
    }

    // ── Content / arrow ──

    [Fact]
    public async Task Content_Wrapped_In_Content_Div()
    {
        var r = await RenderAsync(CreateHelper());
        Assert.Contains("rhx-popover__content", r.Content);
    }

    [Fact]
    public async Task Arrow_Rendered_By_Default()
    {
        var r = await RenderAsync(CreateHelper());
        Assert.Contains("rhx-popover__arrow", r.Content);
    }

    [Fact]
    public async Task Arrow_Disabled()
    {
        var helper = CreateHelper();
        helper.Arrow = false;
        var r = await RenderAsync(helper);
        Assert.DoesNotContain("rhx-popover__arrow", r.Content);
    }

    // ── Placement → position-area ──

    [Fact]
    public async Task Default_Placement_Bottom()
    {
        var r = await RenderAsync(CreateHelper());
        Assert.Contains("position-area:bottom", r.Style);
    }

    [Fact]
    public async Task Custom_Placement_Maps_To_Position_Area()
    {
        var helper = CreateHelper();
        helper.Placement = "top-start";
        var r = await RenderAsync(helper);
        Assert.Contains("position-area:top span-right", r.Style);
    }

    // ── htmx ──

    [Fact]
    public async Task Renders_Htmx_Attributes()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/card";
        var output = CreateOutput("rhx-popover", childContent: "Content");
        await helper.ProcessAsync(CreateContext("rhx-popover"), output);
        AssertAttribute(output, "hx-get", "/api/card");
    }
}
