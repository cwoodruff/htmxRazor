using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class TreeTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Tree";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_tree_renders_with_tree_role(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var tree = page.Locator("#panel-basic-preview [role='tree']");
        await Assertions.Expect(tree).ToHaveCountAsync(1);
        await Assertions.Expect(tree).ToHaveAttributeAsync("aria-label", "File explorer");

        var items = tree.Locator("[role='treeitem']");
        var count = await items.CountAsync();
        Assert.True(count >= 5, $"expected at least 5 tree items, got {count}");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Expanded_branch_shows_children_initially(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var documents = page.Locator("#panel-basic-preview [role='treeitem']", new() { HasTextString = "Documents" }).First;
        await Assertions.Expect(documents).ToHaveAttributeAsync("aria-expanded", "true");

        var childrenGroup = documents.Locator("> .rhx-tree__children");
        await Assertions.Expect(childrenGroup).Not.ToHaveAttributeAsync("hidden", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_collapsed_branch_expands_it(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var images = page.Locator("#panel-basic-preview [role='treeitem']", new() { HasTextString = "Images" }).First;
        await Assertions.Expect(images).ToHaveAttributeAsync("aria-expanded", "false");

        await images.Locator("> .rhx-tree__item-content").ClickAsync();
        await Assertions.Expect(images).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_expanded_branch_collapses_it(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var documents = page.Locator("#panel-basic-preview [role='treeitem']", new() { HasTextString = "Documents" }).First;
        await Assertions.Expect(documents).ToHaveAttributeAsync("aria-expanded", "true");

        await documents.Locator("> .rhx-tree__item-content").ClickAsync();
        await Assertions.Expect(documents).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Multiple_selection_toggles_aria_selected(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var tree = page.Locator("#panel-multiple-preview [role='tree']");
        var apple = tree.Locator("[role='treeitem']", new() { HasTextString = "Apple" }).First;
        var banana = tree.Locator("[role='treeitem']", new() { HasTextString = "Banana" }).First;

        await apple.Locator("> .rhx-tree__item-content").ClickAsync();
        await banana.Locator("> .rhx-tree__item-content").ClickAsync();

        await Assertions.Expect(apple).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(banana).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_branch_is_marked_with_aria_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var locked = page.Locator("#panel-states-preview [role='treeitem']", new() { HasTextString = "Locked" }).First;
        await Assertions.Expect(locked).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Lazy_branch_loads_children_via_htmx(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var lazy = page.Locator("#panel-lazy-preview [role='treeitem']", new() { HasTextString = "Lazy Folder" }).First;
        var childrenGroup = lazy.Locator("> .rhx-tree__children");

        await Assertions.Expect(childrenGroup).ToHaveCountAsync(1);

        await lazy.Locator("> .rhx-tree__item-content").ClickAsync();

        await Assertions.Expect(
            childrenGroup.Locator("text=Loaded child 1")
        ).ToBeVisibleAsync(new() { Timeout = 5000 });
    }
}
