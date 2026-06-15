using htmxRazor.Components.Overlays;
using Xunit;

namespace htmxRazor.Tests;

public class CommandItemTagHelperTests : TagHelperTestBase
{
    private CommandItemTagHelper CreateHelper()
    {
        var helper = new CommandItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    // ══════════════════════════════════════════════
    //  Structure
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Renders_Button_When_No_Href()
    {
        // No href ⇒ a native <button> (htmx-actionable); with href ⇒ an <a>.
        var helper = CreateHelper();
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Home");

        await helper.ProcessAsync(context, output);

        Assert.Equal("button", output.TagName);
        AssertAttribute(output, "role", "option");
        AssertAttribute(output, "type", "button");
    }

    [Fact]
    public async Task Has_Item_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Home");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-command-palette__item"));
    }

    [Fact]
    public async Task Href_Renders_Anchor()
    {
        // A command item with href navigates natively (no JS) — it's a real <a>.
        var helper = CreateHelper();
        helper.Href = "/settings";
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Settings");

        await helper.ProcessAsync(context, output);

        Assert.Equal("a", output.TagName);
        AssertAttribute(output, "href", "/settings");
    }

    // ══════════════════════════════════════════════
    //  Properties
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Value_Sets_DataAttribute()
    {
        var helper = CreateHelper();
        helper.Value = "settings";
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Settings");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-value", "settings");
    }

    [Fact]
    public async Task Icon_Renders_Svg()
    {
        var helper = CreateHelper();
        helper.Icon = "settings";
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Settings");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-command-palette__item-icon", content);
        Assert.Contains("<svg", content);
    }

    [Fact]
    public async Task Description_Rendered()
    {
        var helper = CreateHelper();
        helper.Description = "Manage preferences";
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Settings");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("Manage preferences", content);
        Assert.Contains("rhx-command-palette__item-description", content);
    }

    [Fact]
    public async Task No_Description_No_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Home");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-command-palette__item-description", content);
    }

    [Fact]
    public async Task Shortcut_Rendered_As_Kbd()
    {
        var helper = CreateHelper();
        helper.ShortcutHint = "⌘,";
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Settings");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-command-palette__item-shortcut", content);
        Assert.Contains("<kbd", content);
    }

    [Fact]
    public async Task No_Shortcut_No_Kbd()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Home");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-command-palette__item-shortcut", content);
    }

    // ══════════════════════════════════════════════
    //  Disabled
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Disabled_Sets_AriaDisabled()
    {
        var helper = CreateHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Locked");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "aria-disabled", "true");
        Assert.True(HasClass(output, "rhx-command-palette__item--disabled"));
    }

    // ══════════════════════════════════════════════
    //  Content structure
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Contains_Label_And_Content_Wrappers()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-command-item");
        var output = CreateOutput("rhx-command-item", childContent: "Home Page");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-command-palette__item-content", content);
        Assert.Contains("rhx-command-palette__item-label", content);
        Assert.Contains("Home Page", content);
    }
}
