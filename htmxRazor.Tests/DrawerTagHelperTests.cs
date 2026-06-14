using htmxRazor.Components.Overlays;
using htmxRazor.Rendering;
using Xunit;

namespace htmxRazor.Tests;

public class DrawerTagHelperTests : TagHelperTestBase
{
    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    private DrawerTagHelper CreateHelper()
    {
        var helper = new DrawerTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    // ══════════════════════════════════════════════
    //  DrawerTagHelper — Structure
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Renders_Dialog_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.Equal("dialog", output.TagName);
    }

    [Fact]
    public async Task Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-drawer"));
    }

    [Fact]
    public async Task Has_Placement_Data_Attribute()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-placement", "end");
    }

    [Fact]
    public async Task No_Legacy_Js_Hook_Attribute()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-drawer");
    }

    [Fact]
    public async Task Default_Placement_End()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-drawer--end"));
        AssertAttribute(output, "data-rhx-placement", "end");
    }

    [Fact]
    public async Task Custom_Placement_Start()
    {
        var helper = CreateHelper();
        helper.Placement = "start";
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-drawer--start"));
        AssertAttribute(output, "data-rhx-placement", "start");
    }

    [Fact]
    public async Task Placement_Top()
    {
        var helper = CreateHelper();
        helper.Placement = "top";
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-drawer--top"));
    }

    [Fact]
    public async Task Placement_Bottom()
    {
        var helper = CreateHelper();
        helper.Placement = "bottom";
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-drawer--bottom"));
    }

    [Fact]
    public async Task Contains_Body()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-drawer__body", content);
    }

    // ══════════════════════════════════════════════
    //  Header & Label
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Label_Renders_Title()
    {
        var helper = CreateHelper();
        helper.Label = "Navigation";
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-drawer__title", content);
        Assert.Contains("Navigation", content);
    }

    [Fact]
    public async Task Label_Sets_AriaLabel()
    {
        var helper = CreateHelper();
        helper.Label = "Navigation";
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "aria-label", "Navigation");
    }

    [Fact]
    public async Task Default_Has_Header_With_Close()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-drawer__header", content);
        Assert.Contains("rhx-drawer__close", content);
    }

    [Fact]
    public async Task Close_Button_Uses_Close_Command()
    {
        var helper = CreateHelper();
        helper.Id = "nav";
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("command=\"close\"", content);
        Assert.Contains("commandfor=\"nav\"", content);
    }

    [Fact]
    public async Task NoHeader_Suppresses_Header()
    {
        var helper = CreateHelper();
        helper.NoHeader = true;
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-drawer__header", content);
    }

    [Fact]
    public async Task Label_Is_HtmlEncoded()
    {
        var helper = CreateHelper();
        helper.Label = "A & B";
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("A &amp; B", content);
    }

    // ══════════════════════════════════════════════
    //  Open / Closed
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Closed_Has_No_Open_Attribute()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "open");
    }

    [Fact]
    public async Task Open_Has_Open_Attribute()
    {
        var helper = CreateHelper();
        helper.Open = true;
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "open", "open");
    }

    [Fact]
    public async Task Open_Has_Modifier()
    {
        var helper = CreateHelper();
        helper.Open = true;
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-drawer--open"));
    }

    // ══════════════════════════════════════════════
    //  Contained
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Contained_Modifier()
    {
        var helper = CreateHelper();
        helper.Contained = true;
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-drawer--contained"));
    }

    // ══════════════════════════════════════════════
    //  Footer slot
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Footer_Slot_Rendered()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-drawer");

        var output = new Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput(
            tagName: "rhx-drawer",
            attributes: new Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttributeList(),
            getChildContentAsync: (useCachedResult, encoder) =>
            {
                var slots = SlotRenderer.FromContext(context)!;
                slots.SetHtml("footer", "<button>Close</button>");
                var content = new Microsoft.AspNetCore.Razor.TagHelpers.DefaultTagHelperContent();
                return Task.FromResult<Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContent>(content);
            });

        await helper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.Contains("rhx-drawer__footer", html);
        Assert.Contains("<button>Close</button>", html);
    }

    [Fact]
    public async Task No_Footer_Slot_No_Footer()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-drawer__footer", content);
    }

    [Fact]
    public async Task DrawerFooter_Suppresses_Output()
    {
        var footerHelper = new DrawerFooterTagHelper();
        var context = CreateContext("rhx-drawer-footer");
        SlotRenderer.CreateForContext(context);
        var output = CreateOutput("rhx-drawer-footer", childContent: "<button>OK</button>");

        await footerHelper.ProcessAsync(context, output);

        Assert.True(output.IsContentModified == false || output.Content.GetContent() == "");
    }

    [Fact]
    public async Task DrawerFooter_Registers_Slot()
    {
        var footerHelper = new DrawerFooterTagHelper();
        var context = CreateContext("rhx-drawer-footer");
        var slots = SlotRenderer.CreateForContext(context);
        var output = CreateOutput("rhx-drawer-footer", childContent: "<button>OK</button>");

        await footerHelper.ProcessAsync(context, output);

        Assert.True(slots.Has("footer"));
    }

    // ══════════════════════════════════════════════
    //  Custom CSS & htmx
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Custom_CssClass_Appended()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-drawer";
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "my-drawer"));
        Assert.True(HasClass(output, "rhx-drawer"));
    }

    [Fact]
    public async Task Renders_Htmx_Attributes()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/drawer";
        var context = CreateContext("rhx-drawer");
        var output = CreateOutput("rhx-drawer", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-get", "/api/drawer");
    }
}
