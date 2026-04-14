using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class QrCodeTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/QrCode";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_panel_renders_canvas_with_role_img(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var canvas = page.Locator("#panel-basic-preview canvas.rhx-qr-code");
        await Assertions.Expect(canvas).ToHaveCountAsync(1);
        await Assertions.Expect(canvas).ToHaveAttributeAsync("role", "img");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("aria-label", "Example website");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_panel_sets_data_attributes_for_client_rendering(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var canvas = page.Locator("#panel-basic-preview canvas.rhx-qr-code");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("data-rhx-qr-value", "https://example.com");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("data-rhx-qr-ec", "M");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("data-rhx-qr-size", "128");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Large_panel_uses_size_256_and_high_error_correction(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var canvas = page.Locator("#panel-large-preview canvas.rhx-qr-code");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("data-rhx-qr-size", "256");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("data-rhx-qr-ec", "H");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("width", "256");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("height", "256");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_colors_panel_passes_fill_and_background(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var canvas = page.Locator("#panel-colors-preview canvas.rhx-qr-code");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("data-rhx-qr-fill", "#6366f1");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("data-rhx-qr-background", "#f0f0ff");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Rounded_panel_passes_radius_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var canvas = page.Locator("#panel-rounded-preview canvas.rhx-qr-code");
        await Assertions.Expect(canvas).ToHaveAttributeAsync("data-rhx-qr-radius", "0.50");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Error_correction_panel_renders_all_four_levels(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var canvases = page.Locator("#panel-ec-preview canvas.rhx-qr-code");
        await Assertions.Expect(canvases).ToHaveCountAsync(4);

        await Assertions.Expect(canvases.Nth(0)).ToHaveAttributeAsync("data-rhx-qr-ec", "L");
        await Assertions.Expect(canvases.Nth(1)).ToHaveAttributeAsync("data-rhx-qr-ec", "M");
        await Assertions.Expect(canvases.Nth(2)).ToHaveAttributeAsync("data-rhx-qr-ec", "Q");
        await Assertions.Expect(canvases.Nth(3)).ToHaveAttributeAsync("data-rhx-qr-ec", "H");
    }
}
