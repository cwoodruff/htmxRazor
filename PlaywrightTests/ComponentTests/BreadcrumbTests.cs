using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class BreadcrumbTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Breadcrumb";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Model_bound_breadcrumb_renders_nav_with_ordered_list(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var nav = page.Locator("#panel-model-bound-preview nav.rhx-breadcrumb");
        await Assertions.Expect(nav).ToHaveCountAsync(1);
        await Assertions.Expect(nav).ToHaveAttributeAsync("aria-label", "Breadcrumb");

        var items = nav.Locator("ol.rhx-breadcrumb__list > li.rhx-breadcrumb__item");
        await Assertions.Expect(items).ToHaveCountAsync(3);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Model_bound_breadcrumb_last_item_marked_as_current_page(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var current = page.Locator("#panel-model-bound-preview li.rhx-breadcrumb__item[aria-current='page']");
        await Assertions.Expect(current).ToHaveCountAsync(1);
        await Assertions.Expect(current.Locator("span.rhx-breadcrumb__current")).ToHaveTextAsync("Breadcrumb");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Child_items_render_with_correct_hrefs(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var links = page.Locator("#panel-child-items-preview a.rhx-breadcrumb__link");
        await Assertions.Expect(links).ToHaveCountAsync(2);
        await Assertions.Expect(links.Nth(0)).ToHaveAttributeAsync("href", "/");
        await Assertions.Expect(links.Nth(0)).ToHaveTextAsync("Home");
        await Assertions.Expect(links.Nth(1)).ToHaveAttributeAsync("href", "/Docs/Components/Breadcrumb");
        await Assertions.Expect(links.Nth(1)).ToHaveTextAsync("Navigation");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Child_items_final_entry_is_static_span_with_current_marker(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var current = page.Locator("#panel-child-items-preview li.rhx-breadcrumb__item[aria-current='page']");
        await Assertions.Expect(current).ToHaveCountAsync(1);
        await Assertions.Expect(current.Locator("span.rhx-breadcrumb__current")).ToHaveTextAsync("Breadcrumb");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_separator_renders_chevron_character_and_label(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var nav = page.Locator("#panel-custom-separator-preview nav.rhx-breadcrumb");
        await Assertions.Expect(nav).ToHaveAttributeAsync("aria-label", "Site navigation");

        var separators = nav.Locator("span.rhx-breadcrumb__separator");
        await Assertions.Expect(separators).ToHaveCountAsync(2);
        await Assertions.Expect(separators.First).ToHaveTextAsync("\u203A");
        await Assertions.Expect(separators.First).ToHaveAttributeAsync("aria-hidden", "true");
    }
}
