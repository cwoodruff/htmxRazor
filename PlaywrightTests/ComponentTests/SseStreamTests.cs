using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class SseStreamTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/SseStream";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clock_stream_renders_with_sse_attributes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var stream = page.Locator("#panel-clock-preview div.rhx-sse-stream");
        await Assertions.Expect(stream).ToHaveCountAsync(1);
        await Assertions.Expect(stream).ToHaveAttributeAsync(
            "sse-connect", "/Docs/Components/SseStream?handler=Clock");
        await Assertions.Expect(stream).ToHaveAttributeAsync("sse-swap", "clock");
        await Assertions.Expect(stream).ToHaveAttributeAsync("aria-live", "polite");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clock_stream_includes_sse_extension(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var stream = page.Locator("#panel-clock-preview div.rhx-sse-stream");
        var hxExt = await stream.GetAttributeAsync("hx-ext");
        Assert.NotNull(hxExt);
        Assert.Contains("sse", hxExt);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clock_stream_receives_first_tick(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var stream = page.Locator("#panel-clock-preview div.rhx-sse-stream");
        // The server pushes one HH:mm:ss-formatted <strong> per second for 10 seconds.
        await Assertions.Expect(stream.Locator("strong"))
            .ToBeVisibleAsync(new() { Timeout = 10_000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Countdown_stream_uses_close_on_event(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var stream = page.Locator("#panel-close-preview div.rhx-sse-stream");
        await Assertions.Expect(stream).ToHaveAttributeAsync("sse-close", "done");
        await Assertions.Expect(stream).ToHaveAttributeAsync("sse-swap", "tick");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Countdown_stream_swaps_in_tick_content(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var stream = page.Locator("#panel-close-preview div.rhx-sse-stream");
        // Countdown pushes "Countdown: N" spans at ~1/sec starting from 5.
        await Assertions.Expect(stream)
            .ToContainTextAsync("Countdown:", new() { Timeout = 10_000 });
    }
}
