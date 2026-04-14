using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class AnimatedImageTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/AnimatedImage";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Playing_component_renders_img_and_pause_button(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-playpause-preview .rhx-animated-image").First;
        await Assertions.Expect(root).Not.ToHaveAttributeAsync("data-rhx-paused", "");

        var img = root.Locator("img.rhx-animated-image__img");
        await Assertions.Expect(img).ToHaveAttributeAsync("alt", "Animated loading");

        var control = root.Locator("button.rhx-animated-image__control");
        await Assertions.Expect(control).ToHaveAttributeAsync("aria-label", "Pause animation");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Initially_paused_component_has_paused_marker(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-paused-preview .rhx-animated-image").First;
        await Assertions.Expect(root).ToHaveAttributeAsync("data-rhx-paused", "");
        await Assertions.Expect(root).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-animated-image--paused"));

        var control = root.Locator("button.rhx-animated-image__control");
        await Assertions.Expect(control).ToHaveAttributeAsync("aria-label", "Play animation");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_control_toggles_aria_label(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-playpause-preview .rhx-animated-image").First;
        var control = root.Locator("button.rhx-animated-image__control");

        await Assertions.Expect(control).ToHaveAttributeAsync("aria-label", "Pause animation");
        await control.ClickAsync();
        await Assertions.Expect(control).ToHaveAttributeAsync("aria-label", "Play animation");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-rhx-paused", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_control_toggles_paused_class(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-playpause-preview .rhx-animated-image").First;
        var control = root.Locator("button.rhx-animated-image__control");

        await control.ClickAsync();
        await Assertions.Expect(root).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-animated-image--paused"));

        await control.ClickAsync();
        await Assertions.Expect(root).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-animated-image--paused"));
        await Assertions.Expect(root).Not.ToHaveAttributeAsync("data-rhx-paused", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Component_has_canvas_sibling_for_frame_capture(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-playpause-preview .rhx-animated-image").First;
        var canvas = root.Locator("canvas.rhx-animated-image__canvas");
        await Assertions.Expect(canvas).ToHaveCountAsync(1);
        await Assertions.Expect(canvas).ToHaveAttributeAsync("aria-hidden", "true");
    }
}
