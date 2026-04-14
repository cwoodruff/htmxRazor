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
    public async Task Each_time_element_has_iso_datetime_and_data_attrs(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var first = page.Locator("#panel-long-preview time.rhx-relative-time").First;
        await Assertions.Expect(first).ToHaveAttributeAsync(
            "datetime",
            new System.Text.RegularExpressions.Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+\-]\d{2}:\d{2}$"));
        await Assertions.Expect(first).ToHaveAttributeAsync(
            "data-rhx-relative-time",
            new System.Text.RegularExpressions.Regex(@"^\d{4}-\d{2}-\d{2}T"));
        await Assertions.Expect(first).ToHaveAttributeAsync("data-rhx-relative-format", "long");
        await Assertions.Expect(first).ToHaveAttributeAsync("data-rhx-relative-numeric", "always");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Past_dates_contain_ago_suffix(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-long-preview time.rhx-relative-time");
        // index 3: -1 day -> "1 day ago"
        await Assertions.Expect(items.Nth(3)).ToContainTextAsync("ago");
        // index 4: -14 days
        await Assertions.Expect(items.Nth(4)).ToContainTextAsync("ago");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Future_dates_contain_in_prefix(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-future-preview time.rhx-relative-time");
        await Assertions.Expect(items).ToHaveCountAsync(4);
        // +10 minutes -> "in 10 minutes"
        await Assertions.Expect(items.Nth(1)).ToContainTextAsync("in ");
        await Assertions.Expect(items.Nth(3)).ToContainTextAsync("in ");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Short_format_uses_short_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-short-preview time.rhx-relative-time");
        await Assertions.Expect(items).ToHaveCountAsync(3);
        await Assertions.Expect(items.First).ToHaveAttributeAsync("data-rhx-relative-format", "short");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Auto_numeric_renders_natural_language(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-auto-preview time.rhx-relative-time");
        await Assertions.Expect(items).ToHaveCountAsync(4);
        await Assertions.Expect(items.First).ToHaveAttributeAsync("data-rhx-relative-numeric", "auto");
    }
}
