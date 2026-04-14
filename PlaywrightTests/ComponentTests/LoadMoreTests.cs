using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class LoadMoreTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/LoadMore";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_load_more_renders_with_initial_items(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var list = page.Locator("#panel-basic-preview #item-list");
        await Assertions.Expect(list).ToHaveCountAsync(1);

        var initialCards = list.Locator(".rhx-card");
        await Assertions.Expect(initialCards).ToHaveCountAsync(5);

        var button = list.Locator("button.rhx-load-more__button");
        await Assertions.Expect(button).ToContainTextAsync("Load more items");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Load_more_button_uses_outlined_variant(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var button = page.Locator(
            "#panel-basic-preview #item-list button.rhx-load-more__button.rhx-button--neutral.rhx-button--outlined");
        await Assertions.Expect(button).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_load_more_appends_items_and_replaces_button(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var list = page.Locator("#panel-basic-preview #item-list");
        var button = list.Locator("button.rhx-load-more__button");
        var initialCards = await list.Locator(".rhx-card").CountAsync();

        await button.ClickAsync();

        // After the click the load-more wrapper self-removes and a fresh one
        // (the next page) replaces it via the streamed partial.
        await Assertions.Expect(list.Locator(".rhx-card")).Not.ToHaveCountAsync(initialCards, new() { Timeout = 5000 });
        await Assertions.Expect(list.Locator("button.rhx-load-more__button")).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_load_more_increases_card_count(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var list = page.Locator("#panel-basic-preview #item-list");
        var before = await list.Locator(".rhx-card").CountAsync();

        await list.Locator("button.rhx-load-more__button").ClickAsync();

        await Assertions.Expect(list.Locator(".rhx-card")).Not.ToHaveCountAsync(before, new() { Timeout = 5000 });
        var after = await list.Locator(".rhx-card").CountAsync();
        Assert.True(after > before, $"expected card count to grow; before={before}, after={after}");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Brand_variant_applies_brand_button_class(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var button = page.Locator(
            "#panel-variant-preview #item-list2 button.rhx-load-more__button.rhx-button--brand");
        await Assertions.Expect(button).ToHaveCountAsync(1);
        await Assertions.Expect(button).ToContainTextAsync("Show more");
    }
}
