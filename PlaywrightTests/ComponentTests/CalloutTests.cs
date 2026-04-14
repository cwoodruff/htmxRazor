using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class CalloutTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Callout";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Variants_render_five_callouts_with_alert_role(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var callouts = page.Locator("#panel-variants-preview div.rhx-callout");
        await Assertions.Expect(callouts).ToHaveCountAsync(5);

        foreach (var variant in new[] { "neutral", "brand", "success", "warning", "danger" })
        {
            var el = page.Locator($"#panel-variants-preview div.rhx-callout.rhx-callout--{variant}");
            await Assertions.Expect(el).ToHaveCountAsync(1);
            await Assertions.Expect(el).ToHaveAttributeAsync("role", "alert");
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Every_callout_renders_icon_and_content_slot(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-variants-preview div.rhx-callout span.rhx-callout__icon")
        ).ToHaveCountAsync(5);
        await Assertions.Expect(
            page.Locator("#panel-variants-preview div.rhx-callout div.rhx-callout__content")
        ).ToHaveCountAsync(5);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Closable_callouts_render_close_buttons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var closeButtons = page.Locator("#panel-closable-preview button.rhx-callout__close");
        await Assertions.Expect(closeButtons).ToHaveCountAsync(2);
        await Assertions.Expect(closeButtons.First).ToHaveAttributeAsync("aria-label", "Close");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Auto_dismiss_callout_has_duration_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var callout = page.Locator("#panel-autodismiss-preview div.rhx-callout").First;
        await Assertions.Expect(callout).ToHaveAttributeAsync("data-rhx-duration", "5000");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Htmx_dismiss_removes_notification_from_dom(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var notif = page.Locator("#panel-htmx-preview #notif-1");
        await Assertions.Expect(notif).ToHaveCountAsync(1);

        await page.Locator("#panel-htmx-preview #notif-1 button.rhx-callout__close").ClickAsync();

        await Assertions.Expect(page.Locator("#panel-htmx-preview #notif-1"))
            .ToHaveCountAsync(0, new() { Timeout = 5000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_icon_callout_renders_brand_variant(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var callout = page.Locator("#panel-icon-preview div.rhx-callout.rhx-callout--brand");
        await Assertions.Expect(callout).ToHaveCountAsync(1);
        await Assertions.Expect(callout.Locator("span.rhx-callout__icon")).ToHaveCountAsync(1);
    }
}
