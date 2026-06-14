using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class AnimationTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Animation";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Fade_in_animation_applied_via_css(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var anim = page.Locator("#panel-fade-preview .rhx-animation").First;
        // The animation is emitted server-side as an inline CSS `animation` shorthand (no JS).
        await Assertions.Expect(anim).ToHaveCSSAsync("animation-name", "rhx-fadeIn");
        await Assertions.Expect(anim).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("rhx-animation--playing"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Slide_animations_carry_duration_and_delay(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var slides = page.Locator("#panel-slide-preview .rhx-animation");
        await Assertions.Expect(slides).ToHaveCountAsync(3);

        var left = slides.Nth(0);
        await Assertions.Expect(left).ToHaveCSSAsync("animation-name", "rhx-slideInLeft");
        await Assertions.Expect(left).ToHaveCSSAsync("animation-duration", "0.5s");

        var right = slides.Nth(1);
        await Assertions.Expect(right).ToHaveCSSAsync("animation-name", "rhx-slideInRight");
        await Assertions.Expect(right).ToHaveCSSAsync("animation-delay", "0.2s");

        var up = slides.Nth(2);
        await Assertions.Expect(up).ToHaveCSSAsync("animation-name", "rhx-slideInUp");
        await Assertions.Expect(up).ToHaveCSSAsync("animation-delay", "0.4s");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Bounce_and_zoom_render_separately(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-bouncezoom-preview .rhx-animation");
        await Assertions.Expect(items).ToHaveCountAsync(2);
        await Assertions.Expect(items.Nth(0)).ToHaveCSSAsync("animation-name", "rhx-bounceIn");
        await Assertions.Expect(items.Nth(1)).ToHaveCSSAsync("animation-name", "rhx-zoomIn");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Continuous_animations_set_infinite_iterations(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-continuous-preview .rhx-animation");
        await Assertions.Expect(items).ToHaveCountAsync(3);

        foreach (var i in new[] { 0, 1, 2 })
        {
            await Assertions.Expect(items.Nth(i)).ToHaveCSSAsync("animation-iteration-count", "infinite");
        }

        await Assertions.Expect(items.Nth(0)).ToHaveCSSAsync("animation-name", "rhx-pulse");
        await Assertions.Expect(items.Nth(1)).ToHaveCSSAsync("animation-name", "rhx-bounce");
        await Assertions.Expect(items.Nth(2)).ToHaveCSSAsync("animation-name", "rhx-spin");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Paused_animation_is_play_state_paused(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var paused = page.Locator("#panel-paused-preview #paused-anim");
        await Assertions.Expect(paused).ToHaveCSSAsync("animation-play-state", "paused");
        await Assertions.Expect(paused).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("rhx-animation--playing"));
    }
}
