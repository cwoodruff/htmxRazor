using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class FormatBytesTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/FormatBytes";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Short_format_renders_six_spans(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var spans = page.Locator("#panel-short-preview span.rhx-format-bytes");
        await Assertions.Expect(spans).ToHaveCountAsync(6);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Short_format_uses_short_unit_labels(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        // 1024 -> "1 KB", 1572864 -> "1.5 MB", 1073741824 -> "1 GB", 1099511627776 -> "1 TB"
        var spans = page.Locator("#panel-short-preview span.rhx-format-bytes");
        await Assertions.Expect(spans.Nth(2)).ToHaveTextAsync("1 KB");
        await Assertions.Expect(spans.Nth(3)).ToHaveTextAsync("1.5 MB");
        await Assertions.Expect(spans.Nth(4)).ToHaveTextAsync("1 GB");
        await Assertions.Expect(spans.Nth(5)).ToHaveTextAsync("1 TB");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Long_format_uses_spelled_out_singular_and_plural(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var spans = page.Locator("#panel-long-preview span.rhx-format-bytes");
        await Assertions.Expect(spans).ToHaveCountAsync(3);
        // value=1 -> singular "1 byte"; value=0 -> plural "0 bytes"; value=1572864 -> "1.5 megabytes"
        await Assertions.Expect(spans.Nth(0)).ToHaveTextAsync("1 byte");
        await Assertions.Expect(spans.Nth(1)).ToHaveTextAsync("0 bytes");
        await Assertions.Expect(spans.Nth(2)).ToHaveTextAsync("1.5 megabytes");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Narrow_format_omits_space_between_value_and_unit(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var spans = page.Locator("#panel-narrow-preview span.rhx-format-bytes");
        await Assertions.Expect(spans).ToHaveCountAsync(2);
        await Assertions.Expect(spans.Nth(0)).ToHaveTextAsync("1KB");
        await Assertions.Expect(spans.Nth(1)).ToHaveTextAsync("1.5MB");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Bits_format_uses_bit_unit_labels(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var spans = page.Locator("#panel-bits-preview span.rhx-format-bytes");
        await Assertions.Expect(spans).ToHaveCountAsync(3);
        // 500 -> "500 b", 1000 -> "1 kb", 1000000 -> "1 Mb"
        await Assertions.Expect(spans.Nth(0)).ToHaveTextAsync("500 b");
        await Assertions.Expect(spans.Nth(1)).ToHaveTextAsync("1 kb");
        await Assertions.Expect(spans.Nth(2)).ToHaveTextAsync("1 Mb");
    }
}
