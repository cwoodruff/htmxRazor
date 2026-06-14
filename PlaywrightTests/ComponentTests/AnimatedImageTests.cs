using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class AnimatedImageTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/AnimatedImage";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Plays_continuously_renders_single_image_no_poster(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-plays-preview .rhx-animated-image").First;
        var img = root.Locator("img.rhx-animated-image__img");
        await Assertions.Expect(img).ToHaveAttributeAsync("alt", "Animated loading");

        // No poster element and not the hoverable variant.
        await Assertions.Expect(root.Locator(".rhx-animated-image__poster")).ToHaveCountAsync(0);
        await Assertions.Expect(root).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("rhx-animated-image--hoverable"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Poster_variant_is_hoverable_and_focusable(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-hover-preview .rhx-animated-image").First;
        await Assertions.Expect(root).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("rhx-animated-image--hoverable"));
        await Assertions.Expect(root).ToHaveAttributeAsync("tabindex", "0");
        await Assertions.Expect(root).ToHaveAttributeAsync("role", "img");
        await Assertions.Expect(root).ToHaveAttributeAsync("aria-label", "Hover to play");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Poster_variant_renders_poster_and_animation(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-hover-preview .rhx-animated-image").First;
        await Assertions.Expect(root.Locator("img.rhx-animated-image__poster")).ToHaveCountAsync(1);
        await Assertions.Expect(root.Locator("img.rhx-animated-image__img")).ToHaveCountAsync(1);
        // No control button / canvas in the JS-free model.
        await Assertions.Expect(root.Locator("button.rhx-animated-image__control")).ToHaveCountAsync(0);
        await Assertions.Expect(root.Locator("canvas")).ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Poster_is_faded_at_rest_and_animation_revealed_on_hover(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var root = page.Locator("#panel-hover-preview .rhx-animated-image").First;
        var animation = root.Locator("img.rhx-animated-image__img");

        // At rest the animation layer is faded out (poster shown).
        await Assertions.Expect(animation).ToHaveCSSAsync("opacity", "0");

        // Hovering reveals it via CSS only.
        await root.HoverAsync();
        await Assertions.Expect(animation).ToHaveCSSAsync("opacity", "1");
    }
}
