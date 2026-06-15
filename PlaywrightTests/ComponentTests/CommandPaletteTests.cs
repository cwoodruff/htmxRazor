using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-command-palette is a native modal <dialog> opened by an invoker button
// (command="show-modal" commandfor). Backdrop/focus-trap/Escape are the browser's; results
// are server-rendered and filtered via htmx. No JavaScript, no Cmd+K shortcut.
public sealed class CommandPaletteTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/CommandPalette";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_palette_is_closed_by_default(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var palette = page.Locator("#basic-cp");
        await Assertions.Expect(palette).ToHaveCountAsync(1);
        Assert.False(await palette.EvaluateAsync<bool>("el => el.open"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Invoker_button_opens_palette_and_focuses_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview button", new() { HasTextString = "Open Palette" }).ClickAsync();

        var palette = page.Locator("#basic-cp");
        Assert.True(await palette.EvaluateAsync<bool>("el => el.open"));
        await Assertions.Expect(palette.Locator(".rhx-command-palette__input")).ToBeFocusedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Palette_renders_groups_and_items(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview button", new() { HasTextString = "Open Palette" }).ClickAsync();
        var palette = page.Locator("#basic-cp");

        await Assertions.Expect(palette.Locator("[role='group']")).ToHaveCountAsync(2);
        await Assertions.Expect(palette.Locator("[role='option']")).ToHaveCountAsync(5);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_closes_palette(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview button", new() { HasTextString = "Open Palette" }).ClickAsync();
        var palette = page.Locator("#basic-cp");
        Assert.True(await palette.EvaluateAsync<bool>("el => el.open"));

        await page.Keyboard.PressAsync("Escape");
        Assert.False(await palette.EvaluateAsync<bool>("el => el.open"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Item_with_href_is_a_native_link(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview button", new() { HasTextString = "Open Palette" }).ClickAsync();
        var palette = page.Locator("#basic-cp");

        // Navigation items are real anchors (no JS) — at least one with an href.
        await Assertions.Expect(palette.Locator("a.rhx-command-palette__item[href]").First).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_item_is_marked_aria_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-items-preview button", new() { HasTextString = "Open Palette with Item Variations" }).ClickAsync();
        var palette = page.Locator("#items-cp");

        await Assertions.Expect(palette.Locator("[aria-disabled='true']").First).ToHaveCountAsync(1);
    }
}
