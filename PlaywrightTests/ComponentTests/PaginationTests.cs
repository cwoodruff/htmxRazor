using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class PaginationTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Pagination";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Live_demo_renders_nav_with_current_page_one(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var nav = page.Locator("#panel-htmx-preview #paging-live nav.rhx-pagination");
        await Assertions.Expect(nav).ToHaveCountAsync(1);
        await Assertions.Expect(nav).ToHaveAttributeAsync("aria-label", "Pagination");

        var current = nav.Locator("button[aria-current='page']");
        await Assertions.Expect(current).ToHaveTextAsync("1");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Live_demo_lists_first_page_items(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#panel-htmx-preview #paging-live");
        await Assertions.Expect(container).ToContainTextAsync("Item 1");
        await Assertions.Expect(container).ToContainTextAsync("Item 10");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_page_two_swaps_in_new_items(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#panel-htmx-preview #paging-live");
        var page2Btn = container.Locator("button[aria-label='Go to page 2']");
        await page2Btn.ClickAsync();

        await Assertions.Expect(container).ToContainTextAsync("Item 11", new() { Timeout = 5000 });
        await Assertions.Expect(
            container.Locator("button[aria-current='page']")
        ).ToHaveTextAsync("2");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Previous_button_is_disabled_on_page_one(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var prev = page.Locator("#panel-htmx-preview #paging-live button[aria-label='Previous page']");
        await Assertions.Expect(prev).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_demo_starts_on_page_five(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var current = page.Locator("#panel-basic-preview #paging-basic button[aria-current='page']");
        await Assertions.Expect(current).ToHaveTextAsync("5");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Info_demo_shows_page_info_text(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var info = page.Locator("#panel-info-preview #paging-info nav.rhx-pagination .rhx-pagination__info");
        await Assertions.Expect(info).ToContainTextAsync("Page 7 of");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task No_edges_demo_has_no_first_last_buttons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var nav = page.Locator("#panel-noedges-preview #paging-noedges nav.rhx-pagination");
        await Assertions.Expect(nav.Locator("button[aria-label='First page']")).ToHaveCountAsync(0);
        await Assertions.Expect(nav.Locator("button[aria-label='Last page']")).ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Size_variant_classes_are_applied(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-sizes-preview #paging-small nav.rhx-pagination.rhx-pagination--small")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview #paging-large nav.rhx-pagination.rhx-pagination--large")
        ).ToHaveCountAsync(1);
    }
}
