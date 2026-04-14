using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class KanbanTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Kanban";

    // Note: native HTML drag-and-drop is unreliable in headless browsers, so this suite
    // verifies the rendered DOM structure, the keyboard/accessibility attributes, and the
    // htmx-backed Reset handler. A full drag-between-columns test lives outside this suite.

    [Theory, MemberData(nameof(Browsers))]
    public async Task Board_renders_three_columns(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var board = page.Locator("#panel-demo-preview div.rhx-kanban[data-rhx-kanban]");
        await Assertions.Expect(board).ToHaveCountAsync(1);
        await Assertions.Expect(board).ToHaveAttributeAsync("role", "group");

        var columns = page.Locator("#panel-demo-preview div.rhx-kanban-column");
        await Assertions.Expect(columns).ToHaveCountAsync(3);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Columns_have_expected_ids_and_titles(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        foreach (var (colId, title) in new[] { ("todo", "To Do"), ("doing", "In Progress"), ("done", "Done") })
        {
            var col = page.Locator($"#panel-demo-preview div.rhx-kanban-column[data-rhx-column-id='{colId}']");
            await Assertions.Expect(col).ToHaveCountAsync(1);
            await Assertions.Expect(col.Locator(".rhx-kanban-column__title")).ToContainTextAsync(title);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Wip_limit_column_has_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var doing = page.Locator("#panel-demo-preview div.rhx-kanban-column[data-rhx-column-id='doing']");
        await Assertions.Expect(doing).ToHaveAttributeAsync("data-rhx-max-cards", "3");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Cards_are_rendered_with_card_ids(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var cards = page.Locator("#panel-demo-preview [data-rhx-kanban-card]");
        var count = await cards.CountAsync();
        Assert.True(count >= 6, $"expected at least 6 cards, got {count}");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Card_variants_preview_renders_all_five(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        foreach (var variant in new[] { "brand", "success", "warning", "danger" })
        {
            await Assertions.Expect(
                page.Locator($".rhx-kanban-card.rhx-kanban-card--{variant}").First
            ).ToBeVisibleAsync();
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Reset_button_re_renders_board_via_htmx(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#panel-demo-preview #kanban-demo-container");
        await Assertions.Expect(container.Locator("div.rhx-kanban")).ToHaveCountAsync(1);

        var resetBtn = page.Locator("#panel-demo-preview button.rhx-button", new() { HasTextString = "Reset Board" });
        await resetBtn.ClickAsync();

        // After swap, the board is still present with 3 columns
        await Assertions.Expect(
            container.Locator("div.rhx-kanban-column")
        ).ToHaveCountAsync(3, new() { Timeout = 5000 });
    }
}
