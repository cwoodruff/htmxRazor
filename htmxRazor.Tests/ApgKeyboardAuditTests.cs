using htmxRazor.Components.Actions;
using htmxRazor.Components.Forms;
using htmxRazor.Components.Navigation;
using htmxRazor.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

/// <summary>
/// APG (ARIA Authoring Practices Guide) audit tests.
/// Verifies that Tag Helpers render the required ARIA attributes
/// for tabs, tree, dropdown, and combobox patterns.
/// </summary>
public class ApgKeyboardAuditTests : TagHelperTestBase
{
    private static string HtmlContentToString(IHtmlContent content)
    {
        using var writer = new System.IO.StringWriter();
        content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
        return writer.ToString();
    }

    // ══════════════════════════════════════════════
    //  Tabs — APG Tabs Pattern
    // ══════════════════════════════════════════════

    [Fact]
    public async Task TabGroup_Renders_Tablist_Role()
    {
        var helper = new TabGroupTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("role=\"tablist\"", content);
    }

    // Tabs are now JS-free: each tab is a native radio (mutual exclusivity +
    // arrow-key navigation + Space selection are native) paired with a <label>.
    // The radiogroup lives in the role="tablist" nav; selection is conveyed by the
    // native radio's checked state rather than aria-selected/role="tab"/tabindex.

    private static (TagHelperContext, List<string>) TabCtx()
    {
        var ctx = new TagHelperContext(
            new TagHelperAttributeList(), new Dictionary<object, object>(), "tabs-test");
        var navList = new List<string>();
        ctx.Items["RhxTabsNav"] = navList;
        ctx.Items["RhxTabsGroupName"] = "rhx-tabs-test";
        ctx.Items["RhxTabsPanelMap"] = new List<(string, string)>();
        ctx.Items["RhxTabsCounter"] = new int[1];
        return (ctx, navList);
    }

    [Fact]
    public async Task Tab_Active_Is_Checked_Radio()
    {
        var helper = new TabTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Panel = "general";
        helper.Active = true;
        var (context, navList) = TabCtx();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Single(navList);
        var html = navList[0];
        Assert.Contains("type=\"radio\"", html);
        Assert.Contains(" checked", html);
    }

    [Fact]
    public async Task Tab_Inactive_Radio_Not_Checked()
    {
        var helper = new TabTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Panel = "advanced";
        helper.Active = false;
        var (context, navList) = TabCtx();
        var output = CreateOutput("rhx-tab", childContent: "Advanced");

        await helper.ProcessAsync(context, output);

        Assert.DoesNotContain(" checked", navList[0]);
    }

    [Fact]
    public async Task Tab_Label_Linked_To_Radio()
    {
        var helper = new TabTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Panel = "settings";
        var (context, navList) = TabCtx();
        var output = CreateOutput("rhx-tab", childContent: "Settings");

        await helper.ProcessAsync(context, output);

        var html = navList[0];
        Assert.Contains("id=\"rhx-tabs-test-t0\"", html);
        Assert.Contains("for=\"rhx-tabs-test-t0\"", html);
    }

    [Fact]
    public async Task Tab_Disabled_Radio_Is_Disabled()
    {
        var helper = new TabTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Panel = "disabled-tab";
        helper.Disabled = true;
        var (context, navList) = TabCtx();
        var output = CreateOutput("rhx-tab", childContent: "Disabled");

        await helper.ProcessAsync(context, output);

        Assert.Contains(" disabled", navList[0]);
    }

    // ══════════════════════════════════════════════
    //  Tree — APG Tree View Pattern
    // ══════════════════════════════════════════════

    [Fact]
    public void Tree_Has_Role_Tree()
    {
        var helper = new TreeTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        var context = CreateContext("rhx-tree");
        var output = CreateOutput("rhx-tree");

        helper.Process(context, output);

        AssertAttribute(output, "role", "tree");
    }

    [Fact]
    public void Tree_Has_AriaLabel()
    {
        var helper = new TreeTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.AriaLabel = "File explorer";
        var context = CreateContext("rhx-tree");
        var output = CreateOutput("rhx-tree");

        helper.Process(context, output);

        AssertAttribute(output, "aria-label", "File explorer");
    }

    [Fact]
    public async Task TreeItem_Branch_Has_Role_Treeitem_And_AriaExpanded()
    {
        var helper = new TreeItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Label = "Documents";
        helper.Expanded = false;
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "<div>child</div>");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "role", "treeitem");
        AssertAttribute(output, "aria-expanded", "false");
    }

    [Fact]
    public async Task TreeItem_Branch_Expanded_Has_AriaExpanded_True()
    {
        var helper = new TreeItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Label = "Documents";
        helper.Expanded = true;
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "<div>child</div>");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "aria-expanded", "true");
    }

    [Fact]
    public async Task TreeItem_Leaf_Has_No_AriaExpanded()
    {
        var helper = new TreeItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "readme.txt");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "role", "treeitem");
        AssertNoAttribute(output, "aria-expanded");
    }

    [Fact]
    public async Task TreeItem_Children_Group_Has_Role_Group()
    {
        var helper = new TreeItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Label = "Folder";
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "<div>child</div>");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("role=\"group\"", content);
    }

    [Fact]
    public async Task TreeItem_Disabled_Has_AriaDisabled()
    {
        var helper = new TreeItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Disabled = true;
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "locked.txt");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "aria-disabled", "true");
    }

    [Fact]
    public async Task TreeItem_Has_Tabindex()
    {
        var helper = new TreeItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "file.txt");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "tabindex", "-1");
    }

    // ══════════════════════════════════════════════
    //  Dropdown — APG Menu Button Pattern
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Dropdown_Panel_Has_Role_Menu()
    {
        var helper = new DropdownTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("role=\"menu\"", content);
    }

    [Fact]
    public async Task Dropdown_Panel_Is_Popover_When_Closed()
    {
        var helper = new DropdownTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        // The menu is a native popover (auto): the browser shows/hides it; no aria-hidden hook.
        var content = output.Content.GetContent();
        Assert.Contains("popover=\"auto\"", content);
        Assert.DoesNotContain("aria-hidden", content);
    }

    [Fact]
    public async Task Dropdown_Panel_Is_Auto_Popover_Even_When_Open_Requested()
    {
        var helper = new DropdownTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Open = true; // no-op: a popover can't be shown on load without JS
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("popover=\"auto\"", content);
    }

    [Fact]
    public async Task Dropdown_Trigger_Has_AriaHaspopup_Menu()
    {
        var triggerHelper = new DropdownTriggerTagHelper();
        var context = CreateContext("rhx-dropdown-trigger");
        SlotRenderer.CreateForContext(context);
        context.Items[typeof(DropdownTagHelper)] = new DropdownTagHelper(CreateUrlHelperFactory());
        context.Items["DropdownPanelId"] = "test-panel";
        var output = CreateOutput("rhx-dropdown-trigger", childContent: "<button>Actions</button>");

        await triggerHelper.ProcessAsync(context, output);

        var slots = SlotRenderer.FromContext(context)!;
        var triggerHtml = HtmlContentToString(slots.Get("trigger")!);
        Assert.Contains("aria-haspopup=\"menu\"", triggerHtml);
    }

    [Fact]
    public async Task Dropdown_Trigger_Has_Popovertarget()
    {
        var triggerHelper = new DropdownTriggerTagHelper();
        var context = CreateContext("rhx-dropdown-trigger");
        SlotRenderer.CreateForContext(context);
        context.Items[typeof(DropdownTagHelper)] = new DropdownTagHelper(CreateUrlHelperFactory());
        context.Items["DropdownPanelId"] = "test-panel";
        var output = CreateOutput("rhx-dropdown-trigger", childContent: "<button>Actions</button>");

        await triggerHelper.ProcessAsync(context, output);

        var slots = SlotRenderer.FromContext(context)!;
        var triggerHtml = HtmlContentToString(slots.Get("trigger")!);
        // The Popover API invoker drives open/close; aria-expanded is no longer hand-managed.
        Assert.Contains("popovertarget", triggerHtml);
    }

    [Fact]
    public async Task Dropdown_Trigger_Has_AriaControls()
    {
        var triggerHelper = new DropdownTriggerTagHelper();
        var context = CreateContext("rhx-dropdown-trigger");
        SlotRenderer.CreateForContext(context);
        context.Items[typeof(DropdownTagHelper)] = new DropdownTagHelper(CreateUrlHelperFactory());
        context.Items["DropdownPanelId"] = "my-menu";
        var output = CreateOutput("rhx-dropdown-trigger", childContent: "<button>Actions</button>");

        await triggerHelper.ProcessAsync(context, output);

        var slots = SlotRenderer.FromContext(context)!;
        var triggerHtml = HtmlContentToString(slots.Get("trigger")!);
        Assert.Contains("aria-controls=\"my-menu\"", triggerHtml);
    }

    [Fact]
    public async Task Dropdown_Item_Has_Tabindex_Minus1()
    {
        var helper = new DropdownItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Edit");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "tabindex", "-1");
    }

    [Fact]
    public async Task Dropdown_Link_Item_Has_Tabindex_Minus1()
    {
        var helper = new DropdownItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Href = "/settings";
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Settings");

        await helper.ProcessAsync(context, output);

        Assert.Equal("a", output.TagName);
        AssertAttribute(output, "tabindex", "-1");
        AssertAttribute(output, "role", "menuitem");
    }

    [Fact]
    public async Task Dropdown_Checkbox_Item_Has_Correct_Role_And_State()
    {
        var helper = new DropdownItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Type = "checkbox";
        helper.Checked = true;
        helper.Value = "col-name";
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Name");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "role", "menuitemcheckbox");
        AssertAttribute(output, "aria-checked", "true");
        AssertAttribute(output, "data-value", "col-name");
    }

    [Fact]
    public async Task Dropdown_Panel_Has_AriaLabel()
    {
        var helper = new DropdownTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.AriaLabel = "Actions menu";
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-label=\"Actions menu\"", content);
    }

    // ══════════════════════════════════════════════
    //  Combobox — APG Combobox Pattern
    // ══════════════════════════════════════════════

    // Combobox and select are now native elements: <input list>+<datalist> and <select>.
    // The browser supplies the APG combobox/listbox roles, expansion, and keyboard support,
    // so the audit verifies the native bindings rather than hand-rolled ARIA.

    [Fact]
    public async Task Combobox_Input_Is_Bound_To_Datalist()
    {
        var helper = new ComboboxTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Name = "city";
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("list=\"city-list\"", content);
        Assert.Contains("<datalist id=\"city-list\"", content);
    }

    [Fact]
    public async Task Combobox_Has_Autocomplete_Off()
    {
        var helper = new ComboboxTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Name = "city";
        var context = CreateContext("rhx-combobox");
        var output = CreateOutput("rhx-combobox", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("autocomplete=\"off\"", content);
    }

    [Fact]
    public async Task Select_Renders_Native_Select_With_Label_Binding()
    {
        var helper = new SelectTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        helper.Name = "country";
        helper.Label = "Country";
        var context = CreateContext("rhx-select");
        var output = CreateOutput("rhx-select", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("<select", content);
        Assert.Contains("id=\"country\"", content);
        Assert.Contains("for=\"country\"", content);
    }
}
