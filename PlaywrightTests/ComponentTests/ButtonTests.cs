using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class ButtonTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Button";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Variants_render_five_buttons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var variants = page.Locator("#panel-variants-preview button.rhx-button");
        await Assertions.Expect(variants).ToHaveCountAsync(5);

        foreach (var variant in new[] { "neutral", "brand", "success", "warning", "danger" })
        {
            await Assertions.Expect(
                page.Locator($"#panel-variants-preview button.rhx-button.rhx-button--{variant}")
            ).ToHaveCountAsync(1);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_button_is_not_interactive(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        var disabled = page.Locator("#panel-states-preview button.rhx-button").First;
        await Assertions.Expect(disabled).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Loading_button_has_aria_busy(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        var loading = page.Locator("#panel-states-preview button.rhx-button--loading");
        await Assertions.Expect(loading).ToHaveAttributeAsync("aria-busy", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Htmx_click_updates_result(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var loadBtn = page.Locator("#panel-htmx-preview button.rhx-button", new() { HasTextString = "Load Greeting" });
        var result = page.Locator("#btn-result");

        await Assertions.Expect(result).ToContainTextAsync("Click a button");
        await loadBtn.ClickAsync();
        await Assertions.Expect(result).Not.ToContainTextAsync("Click a button", new() { Timeout = 5000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Link_button_renders_as_anchor(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        var link = page.Locator("#panel-links-preview a.rhx-button").First;
        await Assertions.Expect(link).ToHaveAttributeAsync("href", "/Docs/GettingStarted");
    }
}
