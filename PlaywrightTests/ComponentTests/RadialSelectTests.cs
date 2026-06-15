using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-radial-select is now two native <select>s (category → item). Changing the category fires
// the htmx cascade (also on load) that swaps the item <select>'s <option>s. No JavaScript.
public sealed class RadialSelectTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/RadialSelect";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Renders_category_and_item_selects(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var category = page.Locator(Scope + "select.rhx-radial-select__category");
        var item = page.Locator(Scope + "select.rhx-radial-select__item");

        await Assertions.Expect(category).ToHaveCountAsync(1);
        await Assertions.Expect(item).ToHaveCountAsync(1);
        await Assertions.Expect(category).ToHaveAttributeAsync("name", "cat");
        await Assertions.Expect(item).ToHaveAttributeAsync("name", "FoodItem");
        await Assertions.Expect(category.Locator("option")).ToHaveCountAsync(4);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Default_category_is_selected(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var category = page.Locator(Scope + "select.rhx-radial-select__category");
        await Assertions.Expect(category).ToHaveValueAsync("fruit");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Load_cascade_populates_item_options_for_default_category(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        // The category select's "load" trigger fetches the fruit items via htmx.
        var item = page.Locator(Scope + "select.rhx-radial-select__item");
        await Assertions.Expect(
            item.Locator("option", new() { HasTextString = "Apple" })).ToHaveCountAsync(1, new() { Timeout = 5000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Changing_category_cascades_new_item_options(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var category = page.Locator(Scope + "select.rhx-radial-select__category");
        var item = page.Locator(Scope + "select.rhx-radial-select__item");

        // Wait for the initial (fruit) cascade, then switch to meat.
        await Assertions.Expect(
            item.Locator("option", new() { HasTextString = "Apple" })).ToHaveCountAsync(1, new() { Timeout = 5000 });

        await category.SelectOptionAsync(new SelectOptionValue { Value = "meat" });

        await Assertions.Expect(
            item.Locator("option", new() { HasTextString = "Chicken" })).ToHaveCountAsync(1, new() { Timeout = 5000 });
        await Assertions.Expect(
            item.Locator("option", new() { HasTextString = "Apple" })).ToHaveCountAsync(0);
    }
}
