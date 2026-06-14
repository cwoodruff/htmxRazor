using htmxRazor.Components.Actions;
using Xunit;

namespace htmxRazor.Tests;

public class DropdownTagHelperTests : TagHelperTestBase
{
    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    private DropdownTagHelper CreateDropdownHelper()
    {
        var helper = new DropdownTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    private DropdownItemTagHelper CreateItemHelper()
    {
        var helper = new DropdownItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    // ══════════════════════════════════════════════
    //  DropdownTagHelper
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Dropdown_Renders_Div_With_Block_Class()
    {
        var helper = CreateDropdownHelper();
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-dropdown"));
    }

    [Fact]
    public async Task Dropdown_Menu_Has_Popover_Attribute()
    {
        var helper = CreateDropdownHelper();
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        // Closed dropdown uses an "auto" popover for native light-dismiss.
        Assert.Contains("popover=\"auto\"", content);
    }

    [Fact]
    public async Task Dropdown_Default_Placement_Is_BottomStart()
    {
        var helper = CreateDropdownHelper();
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-placement", "bottom-start");
    }

    [Fact]
    public async Task Dropdown_Custom_Placement_Renders()
    {
        var helper = CreateDropdownHelper();
        helper.Placement = "top-end";
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-placement", "top-end");
    }

    [Fact]
    public async Task Dropdown_Open_Still_Auto_Popover_No_JsFree_Open_On_Load()
    {
        var helper = CreateDropdownHelper();
        helper.Open = true; // no-op: popovers can't be shown on load without JS
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("popover=\"auto\"", content);
        Assert.DoesNotContain("popover=\"manual\"", content);
        Assert.DoesNotContain(" open>", content);
    }

    [Fact]
    public async Task Dropdown_Closed_Uses_Auto_Popover_Without_Open()
    {
        var helper = CreateDropdownHelper();
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("popover=\"auto\"", content);
        Assert.DoesNotContain(" open>", content);
    }

    [Fact]
    public async Task Dropdown_Disabled_Adds_Modifier_Class()
    {
        var helper = CreateDropdownHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-dropdown--disabled"));
    }

    [Fact]
    public async Task Dropdown_Panel_Has_Role_Menu()
    {
        var helper = CreateDropdownHelper();
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "<button>Item</button>");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("role=\"menu\"", content);
    }

    [Fact]
    public async Task Dropdown_Panel_Sets_Anchor_Positioning_Style()
    {
        var helper = CreateDropdownHelper();
        helper.Placement = "bottom-end";
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("position-anchor:--rhx-dropdown-anchor-", content);
        Assert.Contains("position-area:bottom span-left", content);
    }

    [Fact]
    public async Task Dropdown_Panel_Has_Unique_Id()
    {
        var helper = CreateDropdownHelper();
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("id=\"rhx-dropdown-", content);
    }

    [Fact]
    public async Task Dropdown_Panel_Has_Element_Class()
    {
        var helper = CreateDropdownHelper();
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("class=\"rhx-dropdown__panel\"", content);
    }

    [Fact]
    public async Task Dropdown_AriaLabel_Applied_To_Panel()
    {
        var helper = CreateDropdownHelper();
        helper.AriaLabel = "Actions menu";
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-label=\"Actions menu\"", content);
    }

    [Fact]
    public async Task Dropdown_Custom_CssClass_Appended()
    {
        var helper = CreateDropdownHelper();
        helper.CssClass = "my-dropdown";
        var context = CreateContext("rhx-dropdown");
        var output = CreateOutput("rhx-dropdown", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "my-dropdown"));
        Assert.True(HasClass(output, "rhx-dropdown"));
    }

    // ══════════════════════════════════════════════
    //  DropdownItemTagHelper
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Item_Renders_Button_By_Default()
    {
        var helper = CreateItemHelper();
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Edit");

        await helper.ProcessAsync(context, output);

        Assert.Equal("button", output.TagName);
        AssertAttribute(output, "type", "button");
    }

    [Fact]
    public async Task Item_Has_Element_Class()
    {
        var helper = CreateItemHelper();
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Edit");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-dropdown__item"));
    }

    [Fact]
    public async Task Item_Has_Menuitem_Role()
    {
        var helper = CreateItemHelper();
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Edit");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "role", "menuitem");
    }

    [Fact]
    public async Task Item_Wraps_Content_In_Label()
    {
        var helper = CreateItemHelper();
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Edit");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("<span class=\"rhx-dropdown__item-label\">", content);
        Assert.Contains("Edit", content);
    }

    [Fact]
    public async Task Item_Disabled_Sets_Disabled_Attribute()
    {
        var helper = CreateItemHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Edit");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "disabled", "disabled");
        Assert.True(HasClass(output, "rhx-dropdown__item--disabled"));
    }

    [Fact]
    public async Task Item_Href_Renders_As_Anchor()
    {
        var helper = CreateItemHelper();
        helper.Href = "/settings";
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Settings");

        await helper.ProcessAsync(context, output);

        Assert.Equal("a", output.TagName);
        AssertAttribute(output, "href", "/settings");
        AssertNoAttribute(output, "type");
    }

    [Fact]
    public async Task Item_Disabled_Link_Uses_AriaDisabled()
    {
        var helper = CreateItemHelper();
        helper.Href = "/settings";
        helper.Disabled = true;
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Settings");

        await helper.ProcessAsync(context, output);

        Assert.Equal("a", output.TagName);
        AssertAttribute(output, "aria-disabled", "true");
        AssertAttribute(output, "tabindex", "-1");
        AssertNoAttribute(output, "disabled");
    }

    [Fact]
    public async Task Item_Value_Sets_DataValue()
    {
        var helper = CreateItemHelper();
        helper.Value = "edit-action";
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Edit");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-value", "edit-action");
    }

    [Fact]
    public async Task Item_AriaLabel_Sets_Attribute()
    {
        var helper = CreateItemHelper();
        helper.AriaLabel = "Edit this item";
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Edit");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "aria-label", "Edit this item");
    }

    // ── Checkbox item tests ──

    [Fact]
    public async Task Checkbox_Item_Has_MenuitemCheckbox_Role()
    {
        var helper = CreateItemHelper();
        helper.Type = "checkbox";
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Name");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "role", "menuitemcheckbox");
    }

    [Fact]
    public async Task Checkbox_Checked_Has_AriaChecked_True()
    {
        var helper = CreateItemHelper();
        helper.Type = "checkbox";
        helper.Checked = true;
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Name");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "aria-checked", "true");
    }

    [Fact]
    public async Task Checkbox_Unchecked_Has_AriaChecked_False()
    {
        var helper = CreateItemHelper();
        helper.Type = "checkbox";
        helper.Checked = false;
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Name");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "aria-checked", "false");
    }

    [Fact]
    public async Task Checkbox_Has_Checkbox_Modifier_Class()
    {
        var helper = CreateItemHelper();
        helper.Type = "checkbox";
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Name");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-dropdown__item--checkbox"));
    }

    [Fact]
    public async Task Checkbox_Checked_Has_Checked_Modifier()
    {
        var helper = CreateItemHelper();
        helper.Type = "checkbox";
        helper.Checked = true;
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Name");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-dropdown__item--checked"));
    }

    [Fact]
    public async Task Checkbox_Prepends_Check_Mark_Span()
    {
        var helper = CreateItemHelper();
        helper.Type = "checkbox";
        helper.Checked = true;
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Name");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-dropdown__item-check", content);
        Assert.Contains("aria-hidden=\"true\"", content);
        // Check mark should appear before label
        var checkPos = content.IndexOf("rhx-dropdown__item-check");
        var labelPos = content.IndexOf("rhx-dropdown__item-label");
        Assert.True(checkPos < labelPos);
    }

    [Fact]
    public async Task Unchecked_Checkbox_Has_Empty_Check_Mark()
    {
        var helper = CreateItemHelper();
        helper.Type = "checkbox";
        helper.Checked = false;
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Name");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-dropdown__item-check", content);
        // Should not contain the check mark character
        Assert.DoesNotContain("&#10003;", content);
    }

    // ── htmx on items ──

    [Fact]
    public async Task Item_HxPost_Renders()
    {
        var helper = CreateItemHelper();
        helper.HxPost = "/api/action";
        helper.HxTarget = "#result";
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Run");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-post", "/api/action");
        AssertAttribute(output, "hx-target", "#result");
    }

    [Fact]
    public async Task Item_Custom_CssClass_Appended()
    {
        var helper = CreateItemHelper();
        helper.CssClass = "danger-item";
        var context = CreateContext("rhx-dropdown-item");
        var output = CreateOutput("rhx-dropdown-item", childContent: "Delete");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "danger-item"));
        Assert.True(HasClass(output, "rhx-dropdown__item"));
    }

    // ══════════════════════════════════════════════
    //  DropdownDividerTagHelper
    // ══════════════════════════════════════════════

    [Fact]
    public void Divider_Renders_Div()
    {
        var helper = new DropdownDividerTagHelper();
        var context = CreateContext("rhx-dropdown-divider");
        var output = CreateOutput("rhx-dropdown-divider");

        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Divider_Has_Divider_Class()
    {
        var helper = new DropdownDividerTagHelper();
        var context = CreateContext("rhx-dropdown-divider");
        var output = CreateOutput("rhx-dropdown-divider");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-dropdown__divider"));
    }

    [Fact]
    public void Divider_Has_Separator_Role()
    {
        var helper = new DropdownDividerTagHelper();
        var context = CreateContext("rhx-dropdown-divider");
        var output = CreateOutput("rhx-dropdown-divider");

        helper.Process(context, output);

        AssertAttribute(output, "role", "separator");
    }

    // ══════════════════════════════════════════════
    //  Interaction Contract (documentation)
    // ══════════════════════════════════════════════
    //
    // The dropdown is JavaScript-free and relies on the native Popover API.
    // The following behaviors are provided by the browser and verified via
    // Playwright (out of scope for xUnit):
    //
    // Trigger (popovertarget invoker):
    //   Click   → Toggle the menu popover open/close
    //
    // Menu (popover="auto"):
    //   Escape        → Close the menu
    //   Click outside → Close the menu (light-dismiss)
    //   Tab           → Move focus between menu items
    //   Enter/Space   → Activate the focused item (links/buttons)
    //
    // Dropped vs. the previous JS implementation:
    //   - APG arrow-key navigation between items (Tab still works).
    //   - "Stay open on select" / interactive checkbox toggling (the Popover
    //     API light-dismisses on outside click; checkbox items render their
    //     initial state but no longer toggle without JS).
}
