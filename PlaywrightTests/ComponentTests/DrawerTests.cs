using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DrawerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Drawer";

    [Theory, MemberData(nameof(Browsers))]
    public async Task End_drawer_is_hidden_by_default(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var drawer = page.Locator("#end-drawer");
        await Assertions.Expect(drawer).ToHaveCountAsync(1);
        await Assertions.Expect(drawer).ToHaveAttributeAsync("aria-hidden", "true");
        await Assertions.Expect(drawer).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-drawer--open\b"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_open_trigger_shows_end_drawer(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-end-preview [data-rhx-drawer-open='end-drawer']").ClickAsync();

        var drawer = page.Locator("#end-drawer");
        await Assertions.Expect(drawer).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-drawer--open\b"));
        await Assertions.Expect(drawer).ToHaveAttributeAsync("aria-hidden", "false");
        await Assertions.Expect(drawer.Locator(".rhx-drawer__panel")).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Close_button_closes_drawer(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-end-preview [data-rhx-drawer-open='end-drawer']").ClickAsync();
        var drawer = page.Locator("#end-drawer");
        await Assertions.Expect(drawer).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-drawer--open\b"));

        await drawer.Locator(".rhx-drawer__close").ClickAsync();
        await Assertions.Expect(drawer).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-drawer--open\b"));
        await Assertions.Expect(drawer).ToHaveAttributeAsync("aria-hidden", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Close_trigger_with_target_id_closes_drawer(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-end-preview [data-rhx-drawer-open='end-drawer']").ClickAsync();
        var drawer = page.Locator("#end-drawer");
        await Assertions.Expect(drawer).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-drawer--open\b"));

        await drawer.Locator("[data-rhx-drawer-close='end-drawer']").ClickAsync();
        await Assertions.Expect(drawer).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-drawer--open\b"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_closes_drawer(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-end-preview [data-rhx-drawer-open='end-drawer']").ClickAsync();
        var drawer = page.Locator("#end-drawer");
        await Assertions.Expect(drawer).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-drawer--open\b"));

        // Focus inside the drawer so Escape bubbles to the drawer's keydown listener.
        await drawer.Locator(".rhx-drawer__close").FocusAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(drawer).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-drawer--open\b"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Start_drawer_has_start_placement_modifier(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var drawer = page.Locator("#start-drawer");
        await Assertions.Expect(drawer).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-drawer--start\b"));
        await Assertions.Expect(drawer).ToHaveAttributeAsync("data-rhx-placement", "start");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Top_and_bottom_drawers_render(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#top-drawer[data-rhx-placement='top']")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#bottom-drawer[data-rhx-placement='bottom']")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Contained_drawer_has_contained_modifier(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var drawer = page.Locator("#contained-drawer");
        await Assertions.Expect(drawer).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-drawer--contained\b"));
        await Assertions.Expect(drawer).ToHaveAttributeAsync("data-rhx-contained", "");
    }
}
