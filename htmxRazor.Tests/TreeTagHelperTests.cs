using htmxRazor.Components.Navigation;
using Xunit;

namespace htmxRazor.Tests;

// rhx-tree is JS-free: branch items are native <details>/<summary>, leaves are plain elements.
// Expand/collapse + keyboard are the browser's; lazy children load via htmx on the toggle event.
public class TreeTagHelperTests : TagHelperTestBase
{
    private TreeTagHelper CreateTreeHelper()
    {
        var helper = new TreeTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    private TreeItemTagHelper CreateItemHelper()
    {
        var helper = new TreeItemTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    // ══════════════════════════════════════════════
    //  TreeTagHelper
    // ══════════════════════════════════════════════

    [Fact]
    public void Tree_Renders_Div_With_Block_Class_And_Role()
    {
        var helper = CreateTreeHelper();
        var context = CreateContext("rhx-tree");
        var output = CreateOutput("rhx-tree");

        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-tree"));
        AssertAttribute(output, "role", "tree");
    }

    [Fact]
    public void Tree_AriaLabel()
    {
        var helper = CreateTreeHelper();
        helper.AriaLabel = "File explorer";
        var context = CreateContext("rhx-tree");
        var output = CreateOutput("rhx-tree");

        helper.Process(context, output);

        AssertAttribute(output, "aria-label", "File explorer");
    }

    [Fact]
    public void Tree_No_AriaLabel_When_Not_Set()
    {
        var helper = CreateTreeHelper();
        var context = CreateContext("rhx-tree");
        var output = CreateOutput("rhx-tree");

        helper.Process(context, output);

        AssertNoAttribute(output, "aria-label");
    }

    [Fact]
    public void Tree_Custom_CssClass()
    {
        var helper = CreateTreeHelper();
        helper.CssClass = "my-tree";
        var context = CreateContext("rhx-tree");
        var output = CreateOutput("rhx-tree");

        helper.Process(context, output);

        Assert.True(HasClass(output, "my-tree"));
        Assert.True(HasClass(output, "rhx-tree"));
    }

    // ══════════════════════════════════════════════
    //  TreeItemTagHelper — branch vs leaf
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Branch_Renders_Details_With_Summary()
    {
        var helper = CreateItemHelper();
        helper.Label = "Documents";
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "<div>child</div>");

        await helper.ProcessAsync(context, output);

        Assert.Equal("details", output.TagName);
        Assert.True(HasClass(output, "rhx-tree__item"));
        var content = output.Content.GetContent();
        Assert.Contains("<summary class=\"rhx-tree__item-content\"", content);
        Assert.Contains("rhx-tree__expand-icon", content);
        Assert.False(HasClass(output, "rhx-tree__item--leaf"));
    }

    [Fact]
    public async Task Leaf_Renders_Div_No_Details()
    {
        var helper = CreateItemHelper();
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "file.txt");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-tree__item--leaf"));
        var content = output.Content.GetContent();
        Assert.DoesNotContain("rhx-tree__expand-icon", content);
        Assert.DoesNotContain("<summary", content);
        Assert.DoesNotContain("rhx-tree__children", content);
    }

    // ══════════════════════════════════════════════
    //  Label resolution
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Label_From_Property()
    {
        var helper = CreateItemHelper();
        helper.Label = "Documents";
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-tree__item-label", content);
        Assert.Contains("Documents", content);
    }

    [Fact]
    public async Task Label_From_Child_Content()
    {
        var helper = CreateItemHelper();
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "readme.txt");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-tree__item-label", content);
        Assert.Contains("readme.txt", content);
    }

    [Fact]
    public async Task Label_Property_Is_HtmlEncoded()
    {
        var helper = CreateItemHelper();
        helper.Label = "Tom & Jerry";
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "<div>child</div>");

        await helper.ProcessAsync(context, output);

        Assert.Contains("Tom &amp; Jerry", output.Content.GetContent());
    }

    // ══════════════════════════════════════════════
    //  Children group
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Branch_Has_Children_Group()
    {
        var helper = CreateItemHelper();
        helper.Label = "Documents";
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "<div>child</div>");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("class=\"rhx-tree__children\"", content);
        Assert.Contains("role=\"group\"", content);
    }

    // ══════════════════════════════════════════════
    //  Expanded / Collapsed (native <details open>)
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Expanded_Sets_Open_Attribute()
    {
        var helper = CreateItemHelper();
        helper.Label = "Documents";
        helper.Expanded = true;
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "<div>child</div>");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "open", "open");
    }

    [Fact]
    public async Task Collapsed_Has_No_Open_Attribute()
    {
        var helper = CreateItemHelper();
        helper.Label = "Documents";
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "<div>child</div>");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "open");
    }

    // ══════════════════════════════════════════════
    //  Selected / Disabled
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Selected_Has_AriaSelected()
    {
        var helper = CreateItemHelper();
        helper.Selected = true;
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "item");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "aria-selected", "true");
        Assert.True(HasClass(output, "rhx-tree__item--selected"));
    }

    [Fact]
    public async Task Not_Selected_No_AriaSelected()
    {
        var helper = CreateItemHelper();
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "item");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "aria-selected");
        Assert.False(HasClass(output, "rhx-tree__item--selected"));
    }

    [Fact]
    public async Task Disabled_Has_AriaDisabled()
    {
        var helper = CreateItemHelper();
        helper.Disabled = true;
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "item");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "aria-disabled", "true");
        Assert.True(HasClass(output, "rhx-tree__item--disabled"));
    }

    // ══════════════════════════════════════════════
    //  Lazy loading (native <details> toggle + htmx)
    // ══════════════════════════════════════════════

    [Fact]
    public async Task Lazy_Is_A_Branch_With_Children_Group()
    {
        var helper = CreateItemHelper();
        helper.Lazy = true;
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "Projects");

        await helper.ProcessAsync(context, output);

        Assert.Equal("details", output.TagName);
        Assert.True(HasClass(output, "rhx-tree__item--lazy"));
        Assert.False(HasClass(output, "rhx-tree__item--leaf"));
        var content = output.Content.GetContent();
        Assert.Contains("rhx-tree__expand-icon", content);
        Assert.Contains("rhx-tree__children", content);
    }

    [Fact]
    public async Task Htmx_Attributes_Rendered_On_Details()
    {
        // Lazy children load on the native <details> `toggle` event.
        var helper = CreateItemHelper();
        helper.Lazy = true;
        helper.HxGet = "/api/children/1";
        helper.HxTarget = "find .rhx-tree__children";
        helper.HxSwap = "innerHTML";
        helper.HxTrigger = "toggle once";
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "Projects");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "hx-get", "/api/children/1");
        AssertAttribute(output, "hx-target", "find .rhx-tree__children");
        AssertAttribute(output, "hx-swap", "innerHTML");
        AssertAttribute(output, "hx-trigger", "toggle once");
    }

    [Fact]
    public async Task Custom_CssClass()
    {
        var helper = CreateItemHelper();
        helper.CssClass = "my-item";
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "item");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "my-item"));
        Assert.True(HasClass(output, "rhx-tree__item"));
    }

    [Fact]
    public async Task Item_Content_Present()
    {
        var helper = CreateItemHelper();
        var context = CreateContext("rhx-tree-item");
        var output = CreateOutput("rhx-tree-item", childContent: "file.txt");

        await helper.ProcessAsync(context, output);

        Assert.Contains("class=\"rhx-tree__item-content\"", output.Content.GetContent());
    }
}
