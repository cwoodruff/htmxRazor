using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-tree is JS-free: branch items are native <details>/<summary>; leaves are plain elements.
// Expand/collapse is the browser's; lazy children load via htmx on the native toggle event.
public sealed class TreeTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Tree";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_tree_renders_with_tree_role_and_items(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var tree = page.Locator("#panel-basic-preview [role='tree']");
        await Assertions.Expect(tree).ToHaveCountAsync(1);
        await Assertions.Expect(tree).ToHaveAttributeAsync("aria-label", "File explorer");

        var count = await tree.Locator(".rhx-tree__item").CountAsync();
        Assert.True(count >= 5, $"expected at least 5 tree items, got {count}");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Expanded_branch_is_open_initially(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var documents = page.Locator("#panel-basic-preview details.rhx-tree__item", new() { HasTextString = "Documents" }).First;
        Assert.True(await documents.EvaluateAsync<bool>("el => el.open"),
            "The pre-expanded branch should render open.");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_collapsed_branch_expands_it(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var images = page.Locator("#panel-basic-preview details.rhx-tree__item", new() { HasTextString = "Images" }).First;
        Assert.False(await images.EvaluateAsync<bool>("el => el.open"));

        await images.Locator("> summary.rhx-tree__item-content").ClickAsync();
        Assert.True(await images.EvaluateAsync<bool>("el => el.open"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_expanded_branch_collapses_it(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var documents = page.Locator("#panel-basic-preview details.rhx-tree__item", new() { HasTextString = "Documents" }).First;
        Assert.True(await documents.EvaluateAsync<bool>("el => el.open"));

        await documents.Locator("> summary.rhx-tree__item-content").ClickAsync();
        Assert.False(await documents.EvaluateAsync<bool>("el => el.open"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Selected_item_is_marked_with_aria_selected(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        // The States demo renders a pre-selected item server-side.
        var selected = page.Locator("#panel-states-preview .rhx-tree__item--selected");
        await Assertions.Expect(selected.First).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_branch_is_marked_with_aria_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var locked = page.Locator("#panel-states-preview .rhx-tree__item", new() { HasTextString = "Locked" }).First;
        await Assertions.Expect(locked).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Lazy_branch_loads_children_via_htmx_on_toggle(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var lazy = page.Locator("#panel-lazy-preview details.rhx-tree__item", new() { HasTextString = "Lazy Folder" }).First;
        var childrenGroup = lazy.Locator("> .rhx-tree__children");
        await Assertions.Expect(childrenGroup).ToHaveCountAsync(1);

        // Opening the <details> fires a native toggle event → htmx fetches the children.
        await lazy.Locator("> summary.rhx-tree__item-content").ClickAsync();
        await Assertions.Expect(
            childrenGroup.GetByText("Loaded child 1")
        ).ToBeVisibleAsync(new() { Timeout = 5000 });
    }
}
