using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class LiveRegionTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/LiveRegion";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_panel_renders_polite_status_region(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var region = page.Locator("#panel-basic-preview #search-results.rhx-live-region");
        await Assertions.Expect(region).ToHaveCountAsync(1);
        await Assertions.Expect(region).ToHaveAttributeAsync("role", "status");
        await Assertions.Expect(region).ToHaveAttributeAsync("aria-live", "polite");
        await Assertions.Expect(region).ToHaveAttributeAsync("aria-atomic", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Assertive_panel_uses_alert_role_and_assertive_live(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var region = page.Locator("#panel-assertive-preview div.rhx-live-region");
        await Assertions.Expect(region).ToHaveCountAsync(1);
        await Assertions.Expect(region).ToHaveAttributeAsync("role", "alert");
        await Assertions.Expect(region).ToHaveAttributeAsync("aria-live", "assertive");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Assertive_region_contains_danger_callout(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var callout = page.Locator("#panel-assertive-preview div.rhx-live-region .rhx-callout");
        await Assertions.Expect(callout).ToContainTextAsync("Connection lost");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Visually_hidden_region_has_modifier_class(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var region = page.Locator("#panel-hidden-preview #sr-only-demo.rhx-live-region");
        await Assertions.Expect(region).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"rhx-live-region--visually-hidden"));
        await Assertions.Expect(region).ToHaveAttributeAsync("aria-live", "polite");
        await Assertions.Expect(region).ToContainTextAsync("3 results found");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Htmx_search_updates_live_region_contents(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-basic-preview input[name='q']");
        var region = page.Locator("#panel-basic-preview #search-results");

        await Assertions.Expect(region).ToContainTextAsync("Type to search");

        await input.FillAsync("hello");
        // handler returns "Found N results for \"hello\"." or "No results found."
        await Assertions.Expect(region).ToContainTextAsync(
            new System.Text.RegularExpressions.Regex(@"(Found \d+ result|No results found)"),
            new() { Timeout = 5000 });
    }
}
