using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class AnimationTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Animation";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Fade_in_animation_renders_with_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var anim = page.Locator("#panel-fade-preview .rhx-animation").First;
        await Assertions.Expect(anim).ToHaveAttributeAsync("data-rhx-animation", "fadeIn");
        await Assertions.Expect(anim).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-animation--playing"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Slide_animations_carry_duration_and_delay(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var slides = page.Locator("#panel-slide-preview .rhx-animation");
        await Assertions.Expect(slides).ToHaveCountAsync(3);

        var left = slides.Nth(0);
        await Assertions.Expect(left).ToHaveAttributeAsync("data-rhx-animation", "slideInLeft");
        await Assertions.Expect(left).ToHaveAttributeAsync("data-rhx-duration", "500");

        var right = slides.Nth(1);
        await Assertions.Expect(right).ToHaveAttributeAsync("data-rhx-animation", "slideInRight");
        await Assertions.Expect(right).ToHaveAttributeAsync("data-rhx-delay", "200");

        var up = slides.Nth(2);
        await Assertions.Expect(up).ToHaveAttributeAsync("data-rhx-animation", "slideInUp");
        await Assertions.Expect(up).ToHaveAttributeAsync("data-rhx-delay", "400");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Bounce_and_zoom_render_separately(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-bouncezoom-preview .rhx-animation");
        await Assertions.Expect(items).ToHaveCountAsync(2);
        await Assertions.Expect(items.Nth(0)).ToHaveAttributeAsync("data-rhx-animation", "bounceIn");
        await Assertions.Expect(items.Nth(1)).ToHaveAttributeAsync("data-rhx-animation", "zoomIn");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Continuous_animations_set_infinite_iterations(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-continuous-preview .rhx-animation");
        await Assertions.Expect(items).ToHaveCountAsync(3);

        foreach (var i in new[] { 0, 1, 2 })
        {
            await Assertions.Expect(items.Nth(i)).ToHaveAttributeAsync("data-rhx-iterations", "infinite");
        }

        await Assertions.Expect(items.Nth(0)).ToHaveAttributeAsync("data-rhx-animation", "pulse");
        await Assertions.Expect(items.Nth(1)).ToHaveAttributeAsync("data-rhx-animation", "bounce");
        await Assertions.Expect(items.Nth(2)).ToHaveAttributeAsync("data-rhx-animation", "spin");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Paused_animation_carries_paused_marker(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var paused = page.Locator("#panel-paused-preview #paused-anim");
        await Assertions.Expect(paused).ToHaveAttributeAsync("data-rhx-paused", "");
        await Assertions.Expect(paused).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-animation--playing"));
    }
}
