using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DataTableTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/DataTable";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_table_renders_with_caption_and_headers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var table = page.Locator("#panel-basic-preview div.rhx-data-table");
        await Assertions.Expect(table).ToHaveCountAsync(1);

        var caption = page.Locator("#panel-basic-preview caption.rhx-data-table__caption");
        await Assertions.Expect(caption).ToContainTextAsync("Product inventory");

        foreach (var header in new[] { "Name", "Category", "Price", "Stock" })
        {
            await Assertions.Expect(
                page.Locator("#panel-basic-preview th.rhx-data-table__header", new() { HasTextString = header })
            ).ToHaveCountAsync(1);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_table_body_has_rows(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var rows = page.Locator("#panel-basic-preview tbody.rhx-data-table__body tr");
        var count = await rows.CountAsync();
        Assert.True(count >= 10, $"expected at least 10 rows, got {count}");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Bordered_and_compact_modifiers_are_applied(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var table = page.Locator(
            "#panel-mod-preview div.rhx-data-table.rhx-data-table--bordered.rhx-data-table--compact");
        await Assertions.Expect(table).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Pagination_next_button_loads_next_page(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#panel-pag-preview #pag-table-container");
        await Assertions.Expect(container).ToContainTextAsync("Page 1 of");

        var nextBtn = container.Locator("button.rhx-data-table__pagination-button[aria-label='Next page']");
        await nextBtn.ClickAsync();

        await Assertions.Expect(container).ToContainTextAsync("Page 2 of", new() { Timeout = 5000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sortable_headers_swap_table_on_click(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#panel-htmx-preview #htmx-table-container");
        var sortBtn = container.Locator(
            "th.rhx-data-table__header--sortable button.rhx-data-table__sort-button",
            new() { HasTextString = "Name" });

        await Assertions.Expect(sortBtn).ToHaveAttributeAsync("aria-sort", "none");
        await sortBtn.ClickAsync();

        // After swap, a new sort button exists with aria-sort=ascending
        var sortedBtn = container.Locator(
            "button.rhx-data-table__sort-button[aria-sort='ascending']",
            new() { HasTextString = "Name" });
        await Assertions.Expect(sortedBtn).ToHaveCountAsync(1, new() { Timeout = 5000 });
    }
}
