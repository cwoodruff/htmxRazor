using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// Kanban is JS-free: cards move between columns via htmx "move" buttons (◀/▶) that POST the
// card id + direction; the server resolves the adjacent column and re-renders the board.
public sealed class KanbanTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Kanban";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Board_renders_three_columns(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var board = page.Locator("#panel-demo-preview div.rhx-kanban");
        await Assertions.Expect(board).ToHaveCountAsync(1);
        await Assertions.Expect(board).ToHaveAttributeAsync("role", "group");

        await Assertions.Expect(
            page.Locator("#panel-demo-preview div.rhx-kanban-column")).ToHaveCountAsync(3);
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
    public async Task Cards_render_with_move_buttons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var cards = page.Locator("#panel-demo-preview [data-rhx-card-id]");
        Assert.True(await cards.CountAsync() >= 6);

        // Each interactive card has ◀/▶ move buttons (no drag-and-drop).
        await Assertions.Expect(
            page.Locator("#panel-demo-preview .rhx-kanban-card__move").First).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Move_next_button_moves_card_to_adjacent_column(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#panel-demo-preview #kanban-demo-container");
        var todoCol = container.Locator("div.rhx-kanban-column[data-rhx-column-id='todo']");

        // The demo board is shared in-memory state across runs; reset to the known layout first.
        await page.Locator("#panel-demo-preview button.rhx-button", new() { HasTextString = "Reset Board" }).ClickAsync();
        await Assertions.Expect(todoCol.Locator("[data-rhx-card-id='1']")).ToHaveCountAsync(1, new() { Timeout = 5000 });

        // Click its "move to next column" (▶) button → htmx moves it to "doing".
        await todoCol.Locator("[data-rhx-card-id='1'] .rhx-kanban-card__move[aria-label='Move to next column']").ClickAsync();

        await Assertions.Expect(
            container.Locator("div.rhx-kanban-column[data-rhx-column-id='doing'] [data-rhx-card-id='1']")
        ).ToHaveCountAsync(1, new() { Timeout = 5000 });
        await Assertions.Expect(todoCol.Locator("[data-rhx-card-id='1']")).ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Reset_button_re_renders_board_via_htmx(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#panel-demo-preview #kanban-demo-container");
        var resetBtn = page.Locator("#panel-demo-preview button.rhx-button", new() { HasTextString = "Reset Board" });
        await resetBtn.ClickAsync();

        await Assertions.Expect(
            container.Locator("div.rhx-kanban-column")).ToHaveCountAsync(3, new() { Timeout = 5000 });
    }
}
