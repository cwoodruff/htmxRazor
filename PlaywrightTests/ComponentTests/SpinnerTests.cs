using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class SpinnerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Spinner";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sizes_example_renders_four_spinners(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var spinners = page.Locator("#panel-sizes-preview span.rhx-spinner");
        await Assertions.Expect(spinners).ToHaveCountAsync(4);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sizes_include_small_and_large_modifier_variants(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-sizes-preview span.rhx-spinner.rhx-spinner--small")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview span.rhx-spinner.rhx-spinner--large")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Spinners_expose_status_role_and_default_label(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var spinner = page.Locator("#panel-sizes-preview span.rhx-spinner").First;
        await Assertions.Expect(spinner).ToHaveAttributeAsync("role", "status");
        await Assertions.Expect(spinner).ToHaveAttributeAsync("aria-label", "Loading");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_size_spinner_uses_inline_style_and_custom_label(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var custom = page.Locator("#panel-sizes-preview span.rhx-spinner[aria-label='Custom size']");
        await Assertions.Expect(custom).ToHaveCountAsync(1);
        var style = await custom.GetAttributeAsync("style");
        Assert.NotNull(style);
        Assert.Contains("3rem", style);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Htmx_indicator_spinner_has_htmx_indicator_class(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var indicator = page.Locator("#panel-htmx-preview span.rhx-spinner.htmx-indicator#my-indicator");
        await Assertions.Expect(indicator).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Spinner_renders_inline_svg_circle(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var svg = page.Locator("#panel-sizes-preview span.rhx-spinner svg").First;
        await Assertions.Expect(svg).ToHaveCountAsync(1);
        await Assertions.Expect(svg.Locator("circle")).ToHaveCountAsync(1);
    }
}
