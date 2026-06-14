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
        // A closed native <dialog> has no [open] attribute and is not visible.
        await Assertions.Expect(drawer).Not.ToHaveAttributeAsync(
            "open", new System.Text.RegularExpressions.Regex(".*"));
        await Assertions.Expect(drawer).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_open_trigger_shows_end_drawer(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-end-preview [command='show-modal'][commandfor='end-drawer']").ClickAsync();

        var drawer = page.Locator("#end-drawer");
        await Assertions.Expect(drawer).ToHaveAttributeAsync(
            "open", new System.Text.RegularExpressions.Regex(".*"));
        await Assertions.Expect(drawer).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Close_button_closes_drawer(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-end-preview [commandfor='end-drawer']").First.ClickAsync();
        var drawer = page.Locator("#end-drawer");
        await Assertions.Expect(drawer).ToBeVisibleAsync();

        await drawer.Locator(".rhx-drawer__close").ClickAsync();
        await Assertions.Expect(drawer).Not.ToHaveAttributeAsync(
            "open", new System.Text.RegularExpressions.Regex(".*"));
        await Assertions.Expect(drawer).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Close_command_button_closes_drawer(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-end-preview [commandfor='end-drawer']").First.ClickAsync();
        var drawer = page.Locator("#end-drawer");
        await Assertions.Expect(drawer).ToBeVisibleAsync();

        // A "Close" button uses command="close" commandfor="end-drawer" (header + footer both do).
        await drawer.Locator("[command='close'][commandfor='end-drawer']").First.ClickAsync();
        await Assertions.Expect(drawer).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_closes_drawer(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-end-preview [commandfor='end-drawer']").First.ClickAsync();
        var drawer = page.Locator("#end-drawer");
        await Assertions.Expect(drawer).ToBeVisibleAsync();

        // A modal <dialog> closes on Escape natively.
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(drawer).Not.ToBeVisibleAsync();
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
    }
}
