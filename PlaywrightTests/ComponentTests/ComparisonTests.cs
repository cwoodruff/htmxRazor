using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class ComparisonTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Comparison";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Default_renders_before_after_layers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var comp = page.Locator("#panel-beforeafter-preview .rhx-comparison").First;
        await Assertions.Expect(comp).ToBeVisibleAsync();

        await Assertions.Expect(comp.Locator(".rhx-comparison__before img"))
            .ToHaveAttributeAsync("alt", "Original photo");
        await Assertions.Expect(comp.Locator(".rhx-comparison__after img"))
            .ToHaveAttributeAsync("alt", "Grayscale version");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task After_panel_is_user_resizable_via_css(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var after = page.Locator("#panel-beforeafter-preview .rhx-comparison__after").First;
        // Resizing is the native CSS resize grip — no JS.
        await Assertions.Expect(after).ToHaveCSSAsync("resize", "horizontal");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_position_reflects_in_after_width(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var after = page.Locator("#panel-position-preview .rhx-comparison__after").First;
        var width = await after.EvaluateAsync<string>("el => el.style.width");
        Assert.Equal("25%", width);
    }
}
