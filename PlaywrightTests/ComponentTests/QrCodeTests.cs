using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class QrCodeTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/QrCode";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_panel_renders_svg_with_role_img(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var svg = page.Locator("#panel-basic-preview svg.rhx-qr-code");
        await Assertions.Expect(svg).ToHaveCountAsync(1);
        await Assertions.Expect(svg).ToHaveAttributeAsync("role", "img");
        await Assertions.Expect(svg).ToHaveAttributeAsync("aria-label", "Example website");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_panel_renders_modules_server_side(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var svg = page.Locator("#panel-basic-preview svg.rhx-qr-code");
        // The QR is generated on the server: dark modules render as path/rect geometry,
        // not via any data-* hook or client script.
        var modules = svg.Locator("path, rect");
        Assert.True(await modules.CountAsync() > 0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Large_panel_uses_size_256(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var svg = page.Locator("#panel-large-preview svg.rhx-qr-code");
        await Assertions.Expect(svg).ToHaveAttributeAsync("width", "256");
        await Assertions.Expect(svg).ToHaveAttributeAsync("height", "256");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Error_correction_panel_renders_all_four_levels(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var svgs = page.Locator("#panel-ec-preview svg.rhx-qr-code");
        await Assertions.Expect(svgs).ToHaveCountAsync(4);
    }
}
