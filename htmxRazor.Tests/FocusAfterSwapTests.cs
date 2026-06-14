using htmxRazor.Components.Overlays;
using Xunit;

namespace htmxRazor.Tests;

public class FocusAfterSwapTests : TagHelperTestBase
{
    // ══════════════════════════════════════════════
    //  Base class — FocusAfterSwap attribute rendering
    // ══════════════════════════════════════════════

    [Fact]
    public async Task FocusAfterSwap_Renders_DataAttribute_On_Button()
    {
        var helper = new htmxRazor.Components.Actions.ButtonTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.FocusAfterSwap = "#search-input";
        var context = CreateContext("rhx-button");
        var output = CreateOutput("rhx-button", childContent: "Click");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-focus-after-swap", "#search-input");
    }

    [Fact]
    public async Task FocusAfterSwap_Not_Rendered_When_Null()
    {
        var helper = new htmxRazor.Components.Actions.ButtonTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        var context = CreateContext("rhx-button");
        var output = CreateOutput("rhx-button", childContent: "Click");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-focus-after-swap");
    }

    [Fact]
    public async Task FocusAfterSwap_Special_Value_First()
    {
        var helper = new htmxRazor.Components.Actions.ButtonTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.FocusAfterSwap = "first";
        var context = CreateContext("rhx-button");
        var output = CreateOutput("rhx-button", childContent: "Click");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-focus-after-swap", "first");
    }

    [Fact]
    public async Task FocusAfterSwap_Special_Value_Self()
    {
        var helper = new htmxRazor.Components.Actions.ButtonTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.FocusAfterSwap = "self";
        var context = CreateContext("rhx-button");
        var output = CreateOutput("rhx-button", childContent: "Click");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-focus-after-swap", "self");
    }

    // ══════════════════════════════════════════════
    //  Dialog — defaults to "first"
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Dialog_Has_Default_FocusAfterSwap()
    {
        var helper = new DialogTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        var context = CreateContext("rhx-dialog");
        var output = CreateOutput("rhx-dialog", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-focus-after-swap", "first");
    }

    [Fact]
    public async Task Dialog_Custom_FocusAfterSwap()
    {
        var helper = new DialogTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.FocusAfterSwap = ".my-input";
        var context = CreateContext("rhx-dialog");
        var output = CreateOutput("rhx-dialog", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-focus-after-swap", ".my-input");
    }

    [Fact]
    public async Task Dialog_FocusAfterSwap_None_Not_Rendered()
    {
        var helper = new DialogTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.FocusAfterSwap = "none";
        var context = CreateContext("rhx-dialog");
        var output = CreateOutput("rhx-dialog", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-focus-after-swap");
    }

    // ══════════════════════════════════════════════
    //  Drawer — defaults to "first"
    // ══════════════════════════════════════════════

    // Drawer focus-after-swap tests removed: the drawer is now a native modal <dialog>
    // (opened via an invoker command), so the browser traps and restores focus itself —
    // the JS-era data-rhx-focus-after-swap hook no longer applies to it.
}
