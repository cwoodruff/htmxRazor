using htmxRazor.Components.Overlays;
using Xunit;

namespace htmxRazor.Tests;

// rhx-command-palette is now a native modal <dialog> opened by an invoker button
// (command="show-modal" commandfor). Backdrop/focus-trap/Escape/light-dismiss are the
// browser's; results are server-rendered and filtered via htmx. No JavaScript, no Cmd+K.
public class CommandPaletteTagHelperTests : TagHelperTestBase
{
    private CommandPaletteTagHelper CreateHelper()
    {
        var helper = new CommandPaletteTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    private async Task<(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output, string content)> RenderAsync(
        CommandPaletteTagHelper helper)
    {
        var context = CreateContext("rhx-command-palette");
        var output = CreateOutput("rhx-command-palette", childContent: "");
        await helper.ProcessAsync(context, output);
        return (output, output.Content.GetContent());
    }

    // ── Structure ──

    [Fact]
    public async Task Renders_Native_Dialog_With_Block_Class()
    {
        var (output, _) = await RenderAsync(CreateHelper());
        Assert.Equal("dialog", output.TagName);
        Assert.True(HasClass(output, "rhx-command-palette"));
    }

    [Fact]
    public async Task Closed_By_Default()
    {
        // A native <dialog> is closed unless it has the `open` attribute.
        var (output, _) = await RenderAsync(CreateHelper());
        AssertNoAttribute(output, "open");
    }

    [Fact]
    public async Task Light_Dismiss_And_AriaLabel()
    {
        var helper = CreateHelper();
        helper.Label = "Quick search";
        var (output, _) = await RenderAsync(helper);
        AssertAttribute(output, "closedby", "any");
        AssertAttribute(output, "aria-label", "Quick search");
    }

    [Fact]
    public async Task Has_Id_For_Invoker_Commandfor()
    {
        var helper = CreateHelper();
        helper.Id = "search";
        var (output, _) = await RenderAsync(helper);
        AssertAttribute(output, "id", "search");
    }

    // ── Inner content ──

    [Fact]
    public async Task Contains_Panel_No_Backdrop_Div()
    {
        var (_, content) = await RenderAsync(CreateHelper());
        Assert.Contains("rhx-command-palette__panel", content);
        // Native ::backdrop replaces the old backdrop element.
        Assert.DoesNotContain("rhx-command-palette__backdrop", content);
    }

    [Fact]
    public async Task Input_Is_Autofocus_Combobox()
    {
        var (_, content) = await RenderAsync(CreateHelper());
        Assert.Contains("role=\"combobox\"", content);
        Assert.Contains("aria-autocomplete=\"list\"", content);
        Assert.Contains("autocomplete=\"off\"", content);
        Assert.Contains("autofocus", content);
    }

    [Fact]
    public async Task No_Shortcut_Kbd()
    {
        // The Cmd+K shortcut is dropped, so there is no shortcut hint.
        var (_, content) = await RenderAsync(CreateHelper());
        Assert.DoesNotContain("rhx-command-palette__shortcut", content);
    }

    [Fact]
    public async Task Placeholder_Rendered()
    {
        var helper = CreateHelper();
        helper.Placeholder = "Find anything...";
        var (_, content) = await RenderAsync(helper);
        Assert.Contains("placeholder=\"Find anything...\"", content);
    }

    [Fact]
    public async Task Contains_Results_With_Listbox_Role()
    {
        var helper = CreateHelper();
        helper.Id = "cp";
        var (_, content) = await RenderAsync(helper);
        Assert.Contains("role=\"listbox\"", content);
        Assert.Contains("id=\"cp-results\"", content);
    }

    [Fact]
    public async Task Input_AriaControls_Points_To_Results()
    {
        var helper = CreateHelper();
        helper.Id = "search";
        var (_, content) = await RenderAsync(helper);
        Assert.Contains("aria-controls=\"search-results\"", content);
    }

    [Fact]
    public async Task Contains_Empty_Message()
    {
        var helper = CreateHelper();
        helper.EmptyMessage = "Nothing here";
        var (_, content) = await RenderAsync(helper);
        Assert.Contains("Nothing here", content);
        Assert.Contains("rhx-command-palette__empty", content);
    }

    [Fact]
    public async Task Contains_Search_Icon()
    {
        var (_, content) = await RenderAsync(CreateHelper());
        Assert.Contains("rhx-command-palette__search-icon", content);
    }

    // ── htmx forwarding ──

    [Fact]
    public async Task HxGet_Forwarded_To_Input()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/search";
        var (_, content) = await RenderAsync(helper);
        Assert.Contains("hx-get=\"/api/search\"", content);
    }

    [Fact]
    public async Task Debounce_In_HxTrigger()
    {
        var helper = CreateHelper();
        helper.Debounce = 500;
        var (_, content) = await RenderAsync(helper);
        Assert.Contains("hx-trigger=\"input changed delay:500ms\"", content);
    }

    [Fact]
    public async Task HxTarget_Points_To_Results()
    {
        var helper = CreateHelper();
        helper.HxGet = "/search";
        helper.Id = "mycp";
        var (_, content) = await RenderAsync(helper);
        Assert.Contains("hx-target=\"#mycp-results\"", content);
    }
}
