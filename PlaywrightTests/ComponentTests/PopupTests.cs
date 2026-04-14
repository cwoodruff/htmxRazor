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
        await Assertions.Expect(popup).ToHaveAttributeAsync("hidden", new System.Text.RegularExpressions.Regex(".*"));
        await Assertions.Expect(popup).ToHaveAttributeAsync("aria-hidden", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Popup_carries_anchor_and_placement_data_attributes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popup = page.Locator("#demo-popup");
        await Assertions.Expect(popup).ToHaveAttributeAsync("data-rhx-anchor", "#popup-anchor");
        await Assertions.Expect(popup).ToHaveAttributeAsync("data-rhx-placement", "bottom-start");
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
    public async Task Open_popup_is_positioned_near_its_anchor(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#popup-anchor").ClickAsync();
        var popup = page.Locator("#demo-popup");
        await Assertions.Expect(popup).ToBeVisibleAsync();

        var anchorBox = await page.Locator("#popup-anchor").BoundingBoxAsync();
        var popupBox = await popup.BoundingBoxAsync();
        Assert.NotNull(anchorBox);
        Assert.NotNull(popupBox);

        // bottom-start placement: popup should sit near anchor bottom
        var verticalDelta = Math.Abs(popupBox!.Y - (anchorBox!.Y + anchorBox.Height));
        Assert.InRange(verticalDelta, 0, 60);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Arrow_popup_renders_arrow_element(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var popup = page.Locator("#demo-popup-arrow");
        await Assertions.Expect(popup).ToHaveCountAsync(1);
        await Assertions.Expect(popup.Locator(".rhx-popup__arrow, [data-rhx-popup-arrow]")).Not.ToHaveCountAsync(0);
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
