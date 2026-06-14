using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class PopupTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Popup";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Popup_is_hidden_by_default(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popup = page.Locator("#demo-popup");
        await Assertions.Expect(popup).ToHaveCountAsync(1);
        // Hidden via CSS (display:none) until the --active class is added.
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Popup_carries_anchor_positioning_style(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popup = page.Locator("#demo-popup");
        await Assertions.Expect(popup).ToHaveAttributeAsync(
            "style",
            new System.Text.RegularExpressions.Regex(@"position-anchor:\s*--demo-popup"));
        await Assertions.Expect(popup).ToHaveAttributeAsync(
            "style",
            new System.Text.RegularExpressions.Regex(@"position-area:\s*bottom span-right"));
        // No legacy JS data hooks.
        await Assertions.Expect(popup).Not.ToHaveAttributeAsync(
            "data-rhx-popup", new System.Text.RegularExpressions.Regex(".*"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_anchor_button_shows_popup(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#popup-anchor").ClickAsync();

        var popup = page.Locator("#demo-popup");
        await Assertions.Expect(popup).ToBeVisibleAsync();
        await Assertions.Expect(popup).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-popup--active\b"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_anchor_again_hides_popup(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var anchor = page.Locator("#popup-anchor");
        var popup = page.Locator("#demo-popup");

        await anchor.ClickAsync();
        await Assertions.Expect(popup).ToBeVisibleAsync();

        await anchor.ClickAsync();
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Arrow_popup_renders_arrow_element(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popup = page.Locator("#demo-popup-arrow");
        await Assertions.Expect(popup).ToHaveCountAsync(1);
        await Assertions.Expect(popup.Locator(".rhx-popup__arrow")).Not.ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Arrow_popup_toggles_on_anchor_click(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var anchor = page.Locator("#popup-arrow-anchor");
        var popup = page.Locator("#demo-popup-arrow");

        await anchor.ClickAsync();
        await Assertions.Expect(popup).ToBeVisibleAsync();
        await Assertions.Expect(popup).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-popup--active\b"));
    }
}
