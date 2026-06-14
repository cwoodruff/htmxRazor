using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class CarouselTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Carousel";

    // This demo page does NOT wrap examples in rhx-tab-group, so carousels live
    // directly in the page. Use aria-label to scope to a specific example.
    private static ILocator GalleryCarousel(IPage page) =>
        page.Locator("[data-rhx-scrollable][aria-label='Image gallery']");

    private static ILocator MinimalCarousel(IPage page) =>
        page.Locator("[data-rhx-scrollable][aria-label='Minimal carousel']");

    [Theory, MemberData(nameof(Browsers))]
    public async Task Gallery_renders_four_slides_and_nav_buttons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var carousel = GalleryCarousel(page);
        await Assertions.Expect(carousel).ToHaveCountAsync(1);
        await Assertions.Expect(carousel).ToHaveAttributeAsync("role", "region");
        await Assertions.Expect(carousel).ToHaveAttributeAsync("aria-roledescription", "carousel");

        // Scroll-snap viewport exists (shim contract).
        await Assertions.Expect(carousel.Locator("[data-rhx-scroll-viewport]")).ToHaveCountAsync(1);

        var slides = carousel.Locator(".rhx-carousel__slide");
        await Assertions.Expect(slides).ToHaveCountAsync(4);

        // Prev/next buttons exist with the shim hook attributes.
        await Assertions.Expect(carousel.Locator("[data-rhx-scroll-prev]")).ToHaveCountAsync(1);
        await Assertions.Expect(carousel.Locator("[data-rhx-scroll-next]")).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Viewport_uses_css_scroll_snap(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var carousel = GalleryCarousel(page);
        var viewport = carousel.Locator("[data-rhx-scroll-viewport]");

        var snapType = await viewport.EvaluateAsync<string>(
            "el => getComputedStyle(el).getPropertyValue('scroll-snap-type')");
        Assert.Contains("x", snapType);

        var overflowX = await viewport.EvaluateAsync<string>(
            "el => getComputedStyle(el).overflowX");
        Assert.Contains(overflowX, new[] { "auto", "scroll" });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Viewport_is_scroll_snap_and_nav_is_wired(string browserName)
    {
        // The actual smooth scroll on next/prev is driven by the shared scroll-button shim
        // (a sanctioned temporary holdout) and is verified manually across browsers; here we
        // assert the deterministic JS-free setup: a scroll-snap viewport + wired nav buttons.
        var page = await OpenAsync(browserName, Path);

        var carousel = GalleryCarousel(page);
        var viewport = carousel.Locator("[data-rhx-scroll-viewport]");

        var overflowX = await viewport.EvaluateAsync<string>("el => getComputedStyle(el).overflowX");
        Assert.Contains(overflowX, new[] { "auto", "scroll" });

        var snap = await viewport.EvaluateAsync<string>("el => getComputedStyle(el).scrollSnapType");
        Assert.Contains("x", snap);

        await Assertions.Expect(carousel.Locator("[data-rhx-scroll-prev]")).ToHaveCountAsync(1);
        await Assertions.Expect(carousel.Locator("[data-rhx-scroll-next]")).ToHaveCountAsync(1);

        // The shared shim is loaded on the page.
        await Assertions.Expect(
            page.Locator("script[src$='/_rhx/js/rhx-scroll-buttons.js']")).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Minimal_carousel_has_no_nav_buttons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var carousel = MinimalCarousel(page);
        await Assertions.Expect(carousel.Locator("[data-rhx-scroll-prev]")).ToHaveCountAsync(0);
        await Assertions.Expect(carousel.Locator("[data-rhx-scroll-next]")).ToHaveCountAsync(0);

        // But the scroll-snap viewport (native swipe/keyboard) is still present.
        await Assertions.Expect(carousel.Locator("[data-rhx-scroll-viewport]")).ToHaveCountAsync(1);
    }
}
