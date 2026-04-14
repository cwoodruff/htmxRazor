using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class FormatNumberTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/FormatNumber";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Decimal_panel_renders_four_spans(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var spans = page.Locator("#panel-decimal-preview span.rhx-format-number");
        await Assertions.Expect(spans).ToHaveCountAsync(4);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Decimal_default_formats_with_grouping(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var spans = page.Locator("#panel-decimal-preview span.rhx-format-number");
        // Invariant culture "N" -> "1,234.56"
        await Assertions.Expect(spans.Nth(0)).ToHaveTextAsync("1,234.56");
        // no grouping
        await Assertions.Expect(spans.Nth(1)).ToHaveTextAsync("1234.56");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Currency_panel_renders_six_formatted_values(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var spans = page.Locator("#panel-currency-preview span.rhx-format-number");
        await Assertions.Expect(spans).ToHaveCountAsync(6);

        // Symbol USD
        await Assertions.Expect(spans.Nth(0)).ToContainTextAsync("$");
        await Assertions.Expect(spans.Nth(0)).ToContainTextAsync("1,234.56");
        // Code USD
        await Assertions.Expect(spans.Nth(1)).ToContainTextAsync("USD");
        // Name US dollars
        await Assertions.Expect(spans.Nth(2)).ToContainTextAsync("US dollars");
        // JPY 0 decimals
        await Assertions.Expect(spans.Nth(3)).ToContainTextAsync("¥");
        // EUR
        await Assertions.Expect(spans.Nth(4)).ToContainTextAsync("€");
        // GBP
        await Assertions.Expect(spans.Nth(5)).ToContainTextAsync("£");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Percent_panel_renders_percentage_values(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var spans = page.Locator("#panel-percent-preview span.rhx-format-number");
        await Assertions.Expect(spans).ToHaveCountAsync(2);
        await Assertions.Expect(spans.Nth(0)).ToContainTextAsync("%");
        await Assertions.Expect(spans.Nth(1)).ToContainTextAsync("75.6");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Culture_panel_formats_with_german_locale(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var span = page.Locator("#panel-culture-preview span.rhx-format-number").First;
        // de-DE uses period for thousands and comma for decimal.
        await Assertions.Expect(span).ToContainTextAsync("1.234,");
    }
}
