using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class ProgressRingTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/ProgressRing";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Values_panel_renders_four_progress_rings(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var rings = page.Locator("#panel-values-preview svg.rhx-progress-ring");
        await Assertions.Expect(rings).ToHaveCountAsync(4);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Rings_have_progressbar_role_and_aria_attributes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var rings = page.Locator("#panel-values-preview svg.rhx-progress-ring");

        await Assertions.Expect(rings.Nth(0)).ToHaveAttributeAsync("role", "progressbar");
        await Assertions.Expect(rings.Nth(0)).ToHaveAttributeAsync("aria-valuenow", "0");
        await Assertions.Expect(rings.Nth(1)).ToHaveAttributeAsync("aria-valuenow", "25");
        await Assertions.Expect(rings.Nth(2)).ToHaveAttributeAsync("aria-valuenow", "65");
        await Assertions.Expect(rings.Nth(3)).ToHaveAttributeAsync("aria-valuenow", "100");

        await Assertions.Expect(rings.First).ToHaveAttributeAsync("aria-valuemin", "0");
        await Assertions.Expect(rings.First).ToHaveAttributeAsync("aria-valuemax", "100");
        await Assertions.Expect(rings.First).ToHaveAttributeAsync("aria-label", "Not started");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Rings_contain_track_fill_circles_and_label_text(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var ring = page.Locator("#panel-values-preview svg.rhx-progress-ring").Nth(2); // value=65
        await Assertions.Expect(ring.Locator("circle.rhx-progress-ring__track")).ToHaveCountAsync(1);

        var fill = ring.Locator("circle.rhx-progress-ring__fill");
        await Assertions.Expect(fill).ToHaveCountAsync(1);
        await Assertions.Expect(fill).ToHaveAttributeAsync("stroke-dasharray", "65 100");

        await Assertions.Expect(ring.Locator("text.rhx-progress-ring__label")).ToHaveTextAsync("65%");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Rings_use_36_view_box(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var ring = page.Locator("#panel-values-preview svg.rhx-progress-ring").First;
        await Assertions.Expect(ring).ToHaveAttributeAsync("viewBox", "0 0 36 36");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_stroke_widths_are_applied_to_track_and_fill(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var rings = page.Locator("#panel-strokes-preview svg.rhx-progress-ring");
        await Assertions.Expect(rings).ToHaveCountAsync(3);

        // Third ring: track=3, indicator=5
        var last = rings.Nth(2);
        await Assertions.Expect(last.Locator("circle.rhx-progress-ring__track")).ToHaveAttributeAsync("stroke-width", "3");
        await Assertions.Expect(last.Locator("circle.rhx-progress-ring__fill")).ToHaveAttributeAsync("stroke-width", "5");
    }
}
