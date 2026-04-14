using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class ZoomableFrameTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/ZoomableFrame";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Image_variant_renders_img_not_iframe(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-image-preview .rhx-zoomable-frame").First;
        await Assertions.Expect(root).ToHaveAttributeAsync("role", "application");
        await Assertions.Expect(root).ToHaveAttributeAsync("tabindex", "0");

        var img = root.Locator(".rhx-zoomable-frame__content img");
        await Assertions.Expect(img).ToHaveAttributeAsync("src", "https://picsum.photos/id/1015/1200/800");
        await Assertions.Expect(img).ToHaveAttributeAsync("alt", "Mountain landscape");
        await Assertions.Expect(img).ToHaveAttributeAsync("draggable", "false");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Scale_bounds_are_reflected_in_data_attributes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-image-preview .rhx-zoomable-frame").First;
        await Assertions.Expect(root).ToHaveAttributeAsync("data-rhx-min-scale", "0.50");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-rhx-max-scale", "4.00");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-rhx-scale", "1.00");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Pre_zoomed_variant_applies_initial_transform(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-prezoomed-preview .rhx-zoomable-frame").First;
        await Assertions.Expect(root).ToHaveAttributeAsync("data-rhx-scale", "2.00");

        var content = root.Locator(".rhx-zoomable-frame__content");
        var transform = await content.EvaluateAsync<string>("el => el.style.transform");
        Assert.Contains("scale(2", transform);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Plus_key_zooms_in_and_fires_event(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-image-preview .rhx-zoomable-frame").First;
        await root.EvaluateAsync(
            "el => { window.__rhxLastZoomScale = -1; el.addEventListener('rhx:zoom:change', e => { window.__rhxLastZoomScale = e.detail.scale; }); }");

        await root.FocusAsync();
        await page.Keyboard.PressAsync("=");

        var scale = await page.EvaluateAsync<double>("() => window.__rhxLastZoomScale");
        Assert.True(scale > 1.0, $"Expected scale > 1 after '=' key, got {scale}");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Minus_key_zooms_out(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-prezoomed-preview .rhx-zoomable-frame").First;
        await root.FocusAsync();
        await page.Keyboard.PressAsync("-");

        var content = root.Locator(".rhx-zoomable-frame__content");
        var transform = await content.EvaluateAsync<string>("el => el.style.transform");
        // Starting at 2.0, "-" reduces by 0.25 -> 1.75.
        Assert.Contains("scale(1.75", transform);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Zero_key_resets_zoom(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-prezoomed-preview .rhx-zoomable-frame").First;
        await root.FocusAsync();
        await page.Keyboard.PressAsync("0");

        var content = root.Locator(".rhx-zoomable-frame__content");
        var transform = await content.EvaluateAsync<string>("el => el.style.transform");
        Assert.Contains("scale(1)", transform);
    }
}
