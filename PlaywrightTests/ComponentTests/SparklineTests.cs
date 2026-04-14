using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class SparklineTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Sparkline";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Line_panel_renders_three_svg_sparklines(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var svgs = page.Locator("#panel-line-preview svg.rhx-sparkline");
        await Assertions.Expect(svgs).ToHaveCountAsync(3);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Line_sparklines_use_polyline_and_img_role(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var first = page.Locator("#panel-line-preview svg.rhx-sparkline").First;
        await Assertions.Expect(first).ToHaveAttributeAsync("role", "img");
        await Assertions.Expect(first).ToHaveAttributeAsync("aria-label", "CPU history");

        var polyline = first.Locator("polyline.rhx-sparkline__line");
        await Assertions.Expect(polyline).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Area_sparklines_include_polygon_area(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var svgs = page.Locator("#panel-area-preview svg.rhx-sparkline");
        await Assertions.Expect(svgs).ToHaveCountAsync(2);

        // Area renders a polygon and a polyline
        var polygons = page.Locator("#panel-area-preview svg.rhx-sparkline polygon.rhx-sparkline__area");
        await Assertions.Expect(polygons).ToHaveCountAsync(2);

        var polylines = page.Locator("#panel-area-preview svg.rhx-sparkline polyline.rhx-sparkline__line");
        await Assertions.Expect(polylines).ToHaveCountAsync(2);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Bar_sparklines_render_rect_elements(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var svgs = page.Locator("#panel-bar-preview svg.rhx-sparkline");
        await Assertions.Expect(svgs).ToHaveCountAsync(2);

        var bars = page.Locator("#panel-bar-preview svg.rhx-sparkline rect.rhx-sparkline__bar");
        // Both bar charts have at least one rect each
        var barCount = await bars.CountAsync();
        Assert.True(barCount > 0, $"Expected at least one rect bar, got {barCount}");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Inline_sparklines_render_with_custom_size_style(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var svgs = page.Locator("#panel-inline-preview svg.rhx-sparkline");
        await Assertions.Expect(svgs).ToHaveCountAsync(3);

        var first = svgs.First;
        await Assertions.Expect(first).ToHaveAttributeAsync(
            "style",
            new System.Text.RegularExpressions.Regex(@"width:80px"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Min_max_sparklines_have_aria_label_and_view_box(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var svgs = page.Locator("#panel-minmax-preview svg.rhx-sparkline");
        await Assertions.Expect(svgs).ToHaveCountAsync(2);
        await Assertions.Expect(svgs.First).ToHaveAttributeAsync("aria-label", "CPU percentage (0-100)");
        await Assertions.Expect(svgs.First).ToHaveAttributeAsync("viewBox", "0 0 200 40");
    }
}
