using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class PopoverTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Popover";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Click_popover_is_hidden_by_default(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popover = page.Locator("#panel-click-preview [data-rhx-popover]");
        await Assertions.Expect(popover).ToHaveCountAsync(1);
        await Assertions.Expect(popover).Not.ToHaveAttributeAsync("data-rhx-visible", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_trigger_shows_popover(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator("#panel-click-preview #pop-click-trigger");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "dialog");

        await trigger.ClickAsync();

        var popover = page.Locator("#panel-click-preview [data-rhx-popover]");
        await Assertions.Expect(popover).ToHaveAttributeAsync("data-rhx-visible", "");
        await Assertions.Expect(popover).ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_trigger_again_closes_popover(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator("#panel-click-preview #pop-click-trigger");
        var popover = page.Locator("#panel-click-preview [data-rhx-popover]");

        await trigger.ClickAsync();
        await Assertions.Expect(popover).ToHaveAttributeAsync("data-rhx-visible", "");

        await trigger.ClickAsync();
        await Assertions.Expect(popover).Not.ToHaveAttributeAsync("data-rhx-visible", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_closes_popover(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator("#panel-click-preview #pop-click-trigger");
        var popover = page.Locator("#panel-click-preview [data-rhx-popover]");

        await trigger.ClickAsync();
        await Assertions.Expect(popover).ToHaveAttributeAsync("data-rhx-visible", "");

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(popover).Not.ToHaveAttributeAsync("data-rhx-visible", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Click_outside_closes_popover(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator("#panel-click-preview #pop-click-trigger");
        var popover = page.Locator("#panel-click-preview [data-rhx-popover]");

        await trigger.ClickAsync();
        await Assertions.Expect(popover).ToHaveAttributeAsync("data-rhx-visible", "");

        await page.Locator("body").ClickAsync(new() { Position = new() { X = 2, Y = 2 } });
        await Assertions.Expect(popover).Not.ToHaveAttributeAsync("data-rhx-visible", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Hover_popover_shows_on_mouseenter(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator("#panel-hover-preview #pop-hover-trigger");
        var popover = page.Locator("#panel-hover-preview [data-rhx-popover]");

        await trigger.HoverAsync();
        await Assertions.Expect(popover).ToHaveAttributeAsync("data-rhx-visible", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Placements_render_four_popovers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popovers = page.Locator("#panel-placements-preview [data-rhx-popover]");
        await Assertions.Expect(popovers).ToHaveCountAsync(4);

        foreach (var placement in new[] { "top", "bottom", "left", "right" })
        {
            await Assertions.Expect(
                page.Locator($"#panel-placements-preview [data-rhx-popover][data-rhx-placement='{placement}']")
            ).ToHaveCountAsync(1);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task No_arrow_popover_has_arrow_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popover = page.Locator("#panel-noarrow-preview [data-rhx-popover]");
        await Assertions.Expect(popover).ToHaveCountAsync(1);
        await Assertions.Expect(popover.Locator(".rhx-popover__arrow")).ToHaveCountAsync(0);
    }
}
