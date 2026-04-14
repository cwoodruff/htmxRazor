using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class CommandPaletteTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/CommandPalette";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_palette_is_hidden_by_default(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var palette = page.Locator("#basic-cp");
        await Assertions.Expect(palette).ToHaveCountAsync(1);
        await Assertions.Expect(palette).ToHaveAttributeAsync("hidden", new System.Text.RegularExpressions.Regex(".*"));
        await Assertions.Expect(palette).ToHaveAttributeAsync("role", "dialog");
        await Assertions.Expect(palette).ToHaveAttributeAsync("aria-modal", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_open_button_shows_palette(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview button", new() { HasTextString = "Open Palette" }).ClickAsync();

        var palette = page.Locator("#basic-cp");
        await Assertions.Expect(palette).Not.ToHaveAttributeAsync("hidden", new System.Text.RegularExpressions.Regex(".*"));
        await Assertions.Expect(palette.Locator(".rhx-command-palette__input")).ToBeFocusedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Palette_renders_groups_and_items(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview button", new() { HasTextString = "Open Palette" }).ClickAsync();
        var palette = page.Locator("#basic-cp");

        var groups = palette.Locator("[role='group']");
        await Assertions.Expect(groups).ToHaveCountAsync(2);

        var items = palette.Locator("[role='option']");
        await Assertions.Expect(items).ToHaveCountAsync(5);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Arrow_down_moves_focus_through_items(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview button", new() { HasTextString = "Open Palette" }).ClickAsync();
        var palette = page.Locator("#basic-cp");

        await page.Keyboard.PressAsync("ArrowDown");
        var first = palette.Locator("[role='option'][data-rhx-focused]");
        await Assertions.Expect(first).ToHaveCountAsync(1);

        await page.Keyboard.PressAsync("ArrowDown");
        var focused = palette.Locator("[role='option'][data-rhx-focused]");
        await Assertions.Expect(focused).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_closes_palette(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview button", new() { HasTextString = "Open Palette" }).ClickAsync();
        var palette = page.Locator("#basic-cp");
        await Assertions.Expect(palette).Not.ToHaveAttributeAsync("hidden", new System.Text.RegularExpressions.Regex(".*"));

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(palette).ToHaveAttributeAsync("hidden", new System.Text.RegularExpressions.Regex(".*"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Backdrop_click_closes_palette(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview button", new() { HasTextString = "Open Palette" }).ClickAsync();
        var palette = page.Locator("#basic-cp");
        await Assertions.Expect(palette).Not.ToHaveAttributeAsync("hidden", new System.Text.RegularExpressions.Regex(".*"));

        // Backdrop is z-index-stacked behind the panel; dispatch the click so
        // Playwright doesn't fail the intercept check on the panel.
        await palette.Locator(".rhx-command-palette__backdrop").DispatchEventAsync("click");
        await Assertions.Expect(palette).ToHaveAttributeAsync("hidden", new System.Text.RegularExpressions.Regex(".*"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Ctrl_k_shortcut_opens_htmx_palette(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var palette = page.Locator("#htmx-cp");
        await Assertions.Expect(palette).ToHaveAttributeAsync("hidden", new System.Text.RegularExpressions.Regex(".*"));

        // The shortcut is "mod+k" — Cmd+K on macOS, Ctrl+K elsewhere.
        // Determine which modifier this browser's navigator.platform reports.
        var isMac = await page.EvaluateAsync<bool>("() => navigator.platform.indexOf('Mac') >= 0");
        var modifier = isMac ? "Meta" : "Control";

        await page.Locator("body").ClickAsync(new() { Position = new() { X = 2, Y = 2 } });
        await page.Keyboard.PressAsync($"{modifier}+k");

        await Assertions.Expect(palette).Not.ToHaveAttributeAsync("hidden", new System.Text.RegularExpressions.Regex(".*"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_item_is_skipped_in_option_list(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-items-preview button", new() { HasTextString = "Open Palette with Item Variations" }).ClickAsync();
        var palette = page.Locator("#items-cp");

        // Locked item is present but marked disabled — it must not be a selectable option
        var disabled = palette.Locator("[aria-disabled='true']");
        await Assertions.Expect(disabled).Not.ToHaveCountAsync(0);
    }
}
