using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class PopoverTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Popover";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Popover_is_native_and_hidden_by_default(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popover = page.Locator("#panel-click-preview #pop-click");
        await Assertions.Expect(popover).ToHaveAttributeAsync("popover", "auto");
        await Assertions.Expect(popover).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_trigger_shows_popover(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-click-preview [popovertarget='pop-click']").ClickAsync();
        await Assertions.Expect(page.Locator("#panel-click-preview #pop-click")).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_closes_popover(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator("#panel-click-preview [popovertarget='pop-click']");
        var popover = page.Locator("#panel-click-preview #pop-click");

        await trigger.ClickAsync();
        await Assertions.Expect(popover).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(popover).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Click_outside_closes_popover(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator("#panel-click-preview [popovertarget='pop-click']");
        var popover = page.Locator("#panel-click-preview #pop-click");

        await trigger.ClickAsync();
        await Assertions.Expect(popover).ToBeVisibleAsync();

        await page.Locator("body").ClickAsync(new() { Position = new() { X = 2, Y = 2 } });
        await Assertions.Expect(popover).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Placements_render_four_popovers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popovers = page.Locator("#panel-placements-preview .rhx-popover[popover]");
        await Assertions.Expect(popovers).ToHaveCountAsync(4);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task No_arrow_popover_has_arrow_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popover = page.Locator("#panel-noarrow-preview #pop-noarrow");
        await Assertions.Expect(popover).ToHaveCountAsync(1);
        await Assertions.Expect(popover.Locator(".rhx-popover__arrow")).ToHaveCountAsync(0);
    }
}
