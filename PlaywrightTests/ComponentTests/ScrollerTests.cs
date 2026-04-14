using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class ScrollerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Scroller";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Horizontal_scroller_renders_content_and_shadows(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var scroller = page.Locator("#panel-horizontal-preview .rhx-scroller").First;
        await Assertions.Expect(scroller).ToHaveAttributeAsync("data-rhx-orientation", "horizontal");
        await Assertions.Expect(scroller).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-scroller--horizontal"));

        await Assertions.Expect(scroller.Locator(".rhx-scroller__content")).ToHaveCountAsync(1);
        await Assertions.Expect(scroller.Locator(".rhx-scroller__shadow--start")).ToHaveCountAsync(1);
        await Assertions.Expect(scroller.Locator(".rhx-scroller__shadow--end")).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Horizontal_overflow_enables_end_shadow_initially(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var scroller = page.Locator("#panel-horizontal-preview .rhx-scroller").First;
        var endShadow = scroller.Locator(".rhx-scroller__shadow--end");

        await Assertions.Expect(endShadow).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-scroller__shadow--active"));
        // At scrollLeft=0 the start shadow is inactive.
        var startShadow = scroller.Locator(".rhx-scroller__shadow--start");
        await Assertions.Expect(startShadow).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-scroller__shadow--active"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Scrolling_right_activates_start_shadow(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var scroller = page.Locator("#panel-horizontal-preview .rhx-scroller").First;
        var content = scroller.Locator(".rhx-scroller__content");

        await content.EvaluateAsync("el => { el.scrollLeft = 200; el.dispatchEvent(new Event('scroll')); }");

        var startShadow = scroller.Locator(".rhx-scroller__shadow--start");
        await Assertions.Expect(startShadow).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-scroller__shadow--active"));

        // Verify scroll position actually changed.
        var scrollLeft = await content.EvaluateAsync<double>("el => el.scrollLeft");
        Assert.True(scrollLeft > 0, $"Expected scrollLeft > 0, got {scrollLeft}");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Vertical_scroller_sets_orientation(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var scroller = page.Locator("#panel-vertical-preview .rhx-scroller").First;
        await Assertions.Expect(scroller).ToHaveAttributeAsync("data-rhx-orientation", "vertical");
        await Assertions.Expect(scroller).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-scroller--vertical"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Both_direction_scroller_tracks_vertical_scroll(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var scroller = page.Locator("#panel-both-preview .rhx-scroller").First;
        await Assertions.Expect(scroller).ToHaveAttributeAsync("data-rhx-orientation", "both");

        var content = scroller.Locator(".rhx-scroller__content");
        await content.EvaluateAsync("el => { el.scrollTop = 80; el.dispatchEvent(new Event('scroll')); }");

        var startShadow = scroller.Locator(".rhx-scroller__shadow--start");
        await Assertions.Expect(startShadow).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-scroller__shadow--active"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task No_overflow_scroller_hides_shadow_indicators(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var scroller = page.Locator("#panel-nooverflow-preview .rhx-scroller").First;
        var startShadow = scroller.Locator(".rhx-scroller__shadow--start");
        var endShadow = scroller.Locator(".rhx-scroller__shadow--end");

        // Neither shadow should be active when content fits entirely.
        await Assertions.Expect(startShadow).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-scroller__shadow--active"));
        await Assertions.Expect(endShadow).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-scroller__shadow--active"));
    }
}
