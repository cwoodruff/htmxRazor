using htmxRazor.Components.Navigation;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

public class TabTagHelperTests : TagHelperTestBase
{
    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    private TabGroupTagHelper CreateGroupHelper()
    {
        var helper = new TabGroupTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    private TabTagHelper CreateTabHelper()
    {
        var helper = new TabTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    private TabPanelTagHelper CreatePanelHelper()
    {
        var helper = new TabPanelTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    /// <summary>
    /// Creates a context with the shared tab state pre-registered (simulating the
    /// parent <c>rhx-tab-group</c>). Returns the context and the nav HTML list.
    /// </summary>
    private (TagHelperContext context, List<string> navList) CreateTabContext(
        string tagName = "rhx-tab", string groupName = "rhx-tabs-test")
    {
        var context = CreateContext(tagName);
        var navList = new List<string>();
        context.Items["RhxTabsNav"] = navList;
        context.Items["RhxTabsGroupName"] = groupName;
        context.Items["RhxTabsPanelMap"] = new List<(string, string)>();
        context.Items["RhxTabsCounter"] = new int[1];
        return (context, navList);
    }

    // ══════════════════════════════════════════════
    //  TabGroupTagHelper
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Group_Renders_Div_With_Block_Class()
    {
        var helper = CreateGroupHelper();
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-tab-group"));
    }

    [Fact]
    public async Task Group_Has_Id()
    {
        var helper = CreateGroupHelper();
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.NotNull(output.Attributes["id"]);
    }

    [Fact]
    public async Task Group_Default_Placement_Has_No_Modifier()
    {
        var helper = CreateGroupHelper();
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        // No modifier class for default top placement
        Assert.False(HasClass(output, "rhx-tab-group--top"));
    }

    [Fact]
    public async Task Group_Bottom_Placement_Adds_Modifier()
    {
        var helper = CreateGroupHelper();
        helper.Placement = "bottom";
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-tab-group--bottom"));
    }

    [Fact]
    public async Task Group_Start_Placement_Adds_Modifier()
    {
        var helper = CreateGroupHelper();
        helper.Placement = "start";
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-tab-group--start"));
    }

    [Fact]
    public async Task Group_End_Placement_Adds_Modifier()
    {
        var helper = CreateGroupHelper();
        helper.Placement = "end";
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-tab-group--end"));
    }

    [Fact]
    public async Task Group_Nav_Has_Tablist_Role()
    {
        var helper = CreateGroupHelper();
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("role=\"tablist\"", content);
    }

    [Fact]
    public async Task Group_Nav_Has_Element_Class()
    {
        var helper = CreateGroupHelper();
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("class=\"rhx-tab-group__nav\"", content);
    }

    [Fact]
    public async Task Group_Body_Has_Element_Class()
    {
        var helper = CreateGroupHelper();
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "panel content");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("class=\"rhx-tab-group__body\"", content);
        Assert.Contains("panel content", content);
    }

    [Fact]
    public async Task Group_AriaLabel_On_Nav()
    {
        var helper = CreateGroupHelper();
        helper.AriaLabel = "Settings tabs";
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("aria-label=\"Settings tabs\"", content);
    }

    [Fact]
    public async Task Group_Renders_Visibility_Style()
    {
        var helper = CreateGroupHelper();
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("<style>", content);
        Assert.Contains(".rhx-tab-panel{display:none}", content);
    }

    [Fact]
    public async Task Group_Custom_CssClass_Appended()
    {
        var helper = CreateGroupHelper();
        helper.CssClass = "my-tabs";
        var context = CreateContext("rhx-tab-group");
        var output = CreateOutput("rhx-tab-group", childContent: "");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "my-tabs"));
        Assert.True(HasClass(output, "rhx-tab-group"));
    }

    // ══════════════════════════════════════════════
    //  TabTagHelper
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Tab_Suppresses_Output()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        var (context, _) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Null(output.TagName);
    }

    [Fact]
    public async Task Tab_Registers_Radio_And_Label_In_Nav_List()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Single(navList);
        Assert.Contains("<input type=\"radio\"", navList[0]);
        Assert.Contains("<label", navList[0]);
        Assert.Contains("</label>", navList[0]);
    }

    [Fact]
    public async Task Tab_Label_Has_Block_Class()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Contains("class=\"rhx-tab\"", navList[0]);
    }

    [Fact]
    public async Task Tab_Radio_Has_Sr_Only_Class()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Contains("rhx-tab__radio", navList[0]);
        Assert.Contains("rhx-sr-only", navList[0]);
    }

    [Fact]
    public async Task Tab_Radio_Uses_Group_Name()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        var (context, navList) = CreateTabContext(groupName: "rhx-tabs-abc");
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Contains("name=\"rhx-tabs-abc\"", navList[0]);
    }

    [Fact]
    public async Task Tab_Radio_Value_Is_Panel()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Contains("value=\"general\"", navList[0]);
    }

    [Fact]
    public async Task Tab_Label_For_Matches_Radio_Id()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        var (context, navList) = CreateTabContext(groupName: "rhx-tabs-abc");
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Contains("id=\"rhx-tabs-abc-t0\"", navList[0]);
        Assert.Contains("for=\"rhx-tabs-abc-t0\"", navList[0]);
    }

    [Fact]
    public async Task Active_Tab_Radio_Is_Checked()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        helper.Active = true;
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Contains(" checked", navList[0]);
    }

    [Fact]
    public async Task Inactive_Tab_Radio_Not_Checked()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.DoesNotContain(" checked", navList[0]);
    }

    [Fact]
    public async Task Active_Tab_Has_Active_Modifier()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        helper.Active = true;
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Contains("rhx-tab--active", navList[0]);
    }

    [Fact]
    public async Task Disabled_Tab_Radio_Disabled_And_Modifier()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        helper.Disabled = true;
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Contains(" disabled", navList[0]);
        Assert.Contains("rhx-tab--disabled", navList[0]);
    }

    [Fact]
    public async Task Tab_Label_Wrapper_Present()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Contains("<span class=\"rhx-tab__label\">", navList[0]);
        Assert.Contains("General", navList[0]);
    }

    [Fact]
    public async Task Tab_Htmx_Get_Renders_On_Radio()
    {
        var helper = CreateTabHelper();
        helper.Panel = "settings";
        helper.HxGet = "/Settings/Content";
        helper.HxTarget = "#panel-settings";
        helper.HxSwap = "innerHTML";
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "Settings");

        await helper.ProcessAsync(context, output);

        Assert.Contains("hx-get=\"/Settings/Content\"", navList[0]);
        Assert.Contains("hx-target=\"#panel-settings\"", navList[0]);
        Assert.Contains("hx-swap=\"innerHTML\"", navList[0]);
    }

    [Fact]
    public async Task Tab_Custom_CssClass_Appended()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        helper.CssClass = "my-tab";
        var (context, navList) = CreateTabContext();
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Contains("my-tab", navList[0]);
        Assert.Contains("rhx-tab", navList[0]);
    }

    [Fact]
    public async Task Multiple_Tabs_Register_In_Order_With_Unique_Ids()
    {
        var (context, navList) = CreateTabContext(groupName: "rhx-tabs-g");

        var tab1 = CreateTabHelper();
        tab1.Panel = "first";
        tab1.Active = true;
        var output1 = CreateOutput("rhx-tab", childContent: "First");
        await tab1.ProcessAsync(context, output1);

        var tab2 = CreateTabHelper();
        tab2.Panel = "second";
        var output2 = CreateOutput("rhx-tab", childContent: "Second");
        await tab2.ProcessAsync(context, output2);

        Assert.Equal(2, navList.Count);
        Assert.Contains("First", navList[0]);
        Assert.Contains("rhx-tabs-g-t0", navList[0]);
        Assert.Contains("Second", navList[1]);
        Assert.Contains("rhx-tabs-g-t1", navList[1]);
    }

    [Fact]
    public async Task Tab_Without_Parent_Context_Suppresses()
    {
        var helper = CreateTabHelper();
        helper.Panel = "general";
        var context = CreateContext("rhx-tab"); // no RhxTabs* items
        var output = CreateOutput("rhx-tab", childContent: "General");

        await helper.ProcessAsync(context, output);

        Assert.Null(output.TagName);
    }

    // ══════════════════════════════════════════════
    //  TabPanelTagHelper
    // ══════════════════════════════════════════════

    [Fact]
    public void Panel_Renders_Div()
    {
        var helper = CreatePanelHelper();
        helper.Name = "general";
        helper.Active = true;
        var context = CreateContext("rhx-tab-panel");
        var output = CreateOutput("rhx-tab-panel", childContent: "Content");

        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Panel_Has_Block_Class()
    {
        var helper = CreatePanelHelper();
        helper.Name = "general";
        helper.Active = true;
        var context = CreateContext("rhx-tab-panel");
        var output = CreateOutput("rhx-tab-panel", childContent: "Content");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-tab-panel"));
    }

    [Fact]
    public void Panel_Has_Role_Tabpanel()
    {
        var helper = CreatePanelHelper();
        helper.Name = "general";
        helper.Active = true;
        var context = CreateContext("rhx-tab-panel");
        var output = CreateOutput("rhx-tab-panel", childContent: "Content");

        helper.Process(context, output);

        AssertAttribute(output, "role", "tabpanel");
    }

    [Fact]
    public void Panel_Has_Correct_Id()
    {
        var helper = CreatePanelHelper();
        helper.Name = "general";
        helper.Active = true;
        var context = CreateContext("rhx-tab-panel");
        var output = CreateOutput("rhx-tab-panel", childContent: "Content");

        helper.Process(context, output);

        AssertAttribute(output, "id", "panel-general");
    }

    [Fact]
    public void Panel_Has_Tabindex_Zero()
    {
        var helper = CreatePanelHelper();
        helper.Name = "general";
        helper.Active = true;
        var context = CreateContext("rhx-tab-panel");
        var output = CreateOutput("rhx-tab-panel", childContent: "Content");

        helper.Process(context, output);

        AssertAttribute(output, "tabindex", "0");
    }

    [Fact]
    public void Panel_Has_No_Hidden_Attribute()
    {
        // Visibility is driven by the group's generated <style> (CSS :has), not `hidden`.
        var helper = CreatePanelHelper();
        helper.Name = "general";
        var context = CreateContext("rhx-tab-panel");
        var output = CreateOutput("rhx-tab-panel", childContent: "Content");

        helper.Process(context, output);

        AssertNoAttribute(output, "hidden");
    }

    [Fact]
    public void Active_Panel_Has_Active_Modifier()
    {
        var helper = CreatePanelHelper();
        helper.Name = "general";
        helper.Active = true;
        var context = CreateContext("rhx-tab-panel");
        var output = CreateOutput("rhx-tab-panel", childContent: "Content");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-tab-panel--active"));
    }

    [Fact]
    public void Inactive_Panel_No_Active_Modifier()
    {
        var helper = CreatePanelHelper();
        helper.Name = "general";
        var context = CreateContext("rhx-tab-panel");
        var output = CreateOutput("rhx-tab-panel", childContent: "Content");

        helper.Process(context, output);

        Assert.False(HasClass(output, "rhx-tab-panel--active"));
    }

    [Fact]
    public void Panel_Custom_CssClass_Appended()
    {
        var helper = CreatePanelHelper();
        helper.Name = "general";
        helper.Active = true;
        helper.CssClass = "my-panel";
        var context = CreateContext("rhx-tab-panel");
        var output = CreateOutput("rhx-tab-panel", childContent: "Content");

        helper.Process(context, output);

        Assert.True(HasClass(output, "my-panel"));
        Assert.True(HasClass(output, "rhx-tab-panel"));
    }

    [Fact]
    public void Panel_Htmx_Attributes_Rendered()
    {
        var helper = CreatePanelHelper();
        helper.Name = "lazy";
        helper.Active = true;
        helper.HxGet = "/Lazy/Content";
        helper.HxTrigger = "revealed";
        var context = CreateContext("rhx-tab-panel");
        var output = CreateOutput("rhx-tab-panel", childContent: "");

        helper.Process(context, output);

        AssertAttribute(output, "hx-get", "/Lazy/Content");
        AssertAttribute(output, "hx-trigger", "revealed");
    }

    // ══════════════════════════════════════════════
    //  Selection / keyboard contract (documentation)
    // ══════════════════════════════════════════════
    //
    // Tabs are now JS-free: each tab is a visually-hidden native radio
    // (mutual exclusivity + arrow-key navigation + Space selection are native
    // browser behavior) paired with a <label>. Panel visibility is driven by a
    // per-group generated <style> using CSS :has() on the checked radio.
    // htmx attributes on the radio fire on `change`, enabling lazy panel loads.
}
