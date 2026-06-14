using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class RelativeTimeTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/RelativeTime";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Long_panel_renders_six_time_elements(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-long-preview time.rhx-relative-time");
        await Assertions.Expect(items).ToHaveCountAsync(6);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Each_time_element_has_iso_datetime_and_server_text(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var first = page.Locator("#panel-long-preview time.rhx-relative-time").First;
        // <time datetime> is the machine-readable value; the human text is rendered server-side.
        await Assertions.Expect(first).ToHaveAttributeAsync(
            "datetime",
            new System.Text.RegularExpressions.Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+\-]\d{2}:\d{2}$"));
        await Assertions.Expect(first).Not.ToBeEmptyAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Past_dates_contain_ago_suffix(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-long-preview time.rhx-relative-time");
        await Assertions.Expect(items.Nth(3)).ToContainTextAsync("ago");
        await Assertions.Expect(items.Nth(4)).ToContainTextAsync("ago");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Future_dates_contain_in_prefix(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-future-preview time.rhx-relative-time");
        await Assertions.Expect(items).ToHaveCountAsync(4);
        await Assertions.Expect(items.Nth(1)).ToContainTextAsync("in ");
        await Assertions.Expect(items.Nth(3)).ToContainTextAsync("in ");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Short_format_renders_compact_text(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-short-preview time.rhx-relative-time");
        await Assertions.Expect(items).ToHaveCountAsync(3);
        await Assertions.Expect(items.First).Not.ToBeEmptyAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Auto_numeric_renders_natural_language(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-auto-preview time.rhx-relative-time");
        await Assertions.Expect(items).ToHaveCountAsync(4);
        await Assertions.Expect(items.First).Not.ToBeEmptyAsync();
    }
}
