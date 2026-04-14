using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class FormatDateTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/FormatDate";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_format_renders_time_elements_with_datetime_attr(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-custom-preview time.rhx-format-date");
        await Assertions.Expect(items).ToHaveCountAsync(3);

        // Sample date is 2025-03-15T10:30:45Z — first example is "yyyy-MM-dd"
        await Assertions.Expect(items.Nth(0)).ToHaveTextAsync("2025-03-15");
        await Assertions.Expect(items.Nth(1)).ToHaveTextAsync("10:30:45");
        await Assertions.Expect(items.Nth(2)).ToHaveTextAsync("March 15, 2025 10:30 AM");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Every_time_has_iso_datetime_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var first = page.Locator("#panel-custom-preview time.rhx-format-date").First;
        // ISO-8601: yyyy-MM-ddTHH:mm:ss with offset
        await Assertions.Expect(first).ToHaveAttributeAsync(
            "datetime",
            new System.Text.RegularExpressions.Regex(@"^2025-03-15T\d{2}:\d{2}:\d{2}[+\-]\d{2}:\d{2}$"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Individual_parts_render_expected_formats(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-parts-preview time.rhx-format-date");
        await Assertions.Expect(items).ToHaveCountAsync(5);
        await Assertions.Expect(items.Nth(0)).ToHaveTextAsync("March 15, 2025");
        await Assertions.Expect(items.Nth(1)).ToHaveTextAsync("Mar 15");
        await Assertions.Expect(items.Nth(2)).ToHaveTextAsync("3/15/2025");
        // weekday "long" + month "long" + day numeric -> "Saturday, March 15"
        await Assertions.Expect(items.Nth(3)).ToHaveTextAsync("Saturday, March 15");
        await Assertions.Expect(items.Nth(4)).ToHaveTextAsync("25");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Time_panel_renders_three_time_elements(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-time-preview time.rhx-format-date");
        await Assertions.Expect(items).ToHaveCountAsync(3);
        // 24h hour/minute -> "10:30"
        await Assertions.Expect(items.Nth(0)).ToHaveTextAsync("10:30");
        // 12h hour/minute/second -> "10:30:45 AM"
        await Assertions.Expect(items.Nth(1)).ToHaveTextAsync("10:30:45 AM");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Timezone_converts_to_local_hour(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-tz-preview time.rhx-format-date");
        await Assertions.Expect(items).ToHaveCountAsync(2);
        // NYC = UTC-5 (or -4 in DST); 10:30 UTC -> between 05:30 and 06:30
        await Assertions.Expect(items.Nth(0)).ToHaveTextAsync(
            new System.Text.RegularExpressions.Regex(@"^(5|6):30$"));
        // Tokyo = UTC+9 -> 19:30
        await Assertions.Expect(items.Nth(1)).ToHaveTextAsync("19:30");
    }
}
