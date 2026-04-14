using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class CarouselTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Carousel";

    // This demo page does NOT wrap examples in rhx-tab-group, so carousels live
    // directly in the page. Use aria-label to scope to a specific example.
    private static ILocator GalleryCarousel(IPage page) =>
        page.Locator("[data-rhx-carousel][aria-label='Image gallery']");

    private static ILocator MinimalCarousel(IPage page) =>
        page.Locator("[data-rhx-carousel][aria-label='Minimal carousel']");

    [Theory, MemberData(nameof(Browsers))]
    public async Task Gallery_renders_four_slides_and_nav_buttons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var carousel = GalleryCarousel(page);
        await Assertions.Expect(carousel).ToHaveCountAsync(1);
        await Assertions.Expect(carousel).ToHaveAttributeAsync("role", "region");
        await Assertions.Expect(carousel).ToHaveAttributeAsync("aria-roledescription", "carousel");

        var items = carousel.Locator(".rhx-carousel__item");
        await Assertions.Expect(items).ToHaveCountAsync(4);

        await Assertions.Expect(carousel.Locator(".rhx-carousel__nav-button--prev")).ToHaveCountAsync(1);
        await Assertions.Expect(carousel.Locator(".rhx-carousel__nav-button--next")).ToHaveCountAsync(1);

        var dots = carousel.Locator(".rhx-carousel__dot");
        await Assertions.Expect(dots).ToHaveCountAsync(4);
        await Assertions.Expect(dots.First).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_next_advances_slide_and_transforms_track(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var carousel = GalleryCarousel(page);
        var nextBtn = carousel.Locator(".rhx-carousel__nav-button--next");
        var dots = carousel.Locator(".rhx-carousel__dot");

        await nextBtn.ClickAsync();

        await Assertions.Expect(dots.Nth(0)).ToHaveAttributeAsync("aria-selected", "false");
        await Assertions.Expect(dots.Nth(1)).ToHaveAttributeAsync("aria-selected", "true");

        // Track transform should move to the second slide.
        var track = carousel.Locator(".rhx-carousel__track");
        var transform = await track.EvaluateAsync<string>("el => el.style.transform");
        Assert.Contains("-100", transform);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_prev_at_start_without_loop_is_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var carousel = GalleryCarousel(page);
        var prevBtn = carousel.Locator(".rhx-carousel__nav-button--prev");
        await Assertions.Expect(prevBtn).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_pagination_dot_jumps_to_slide(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var carousel = GalleryCarousel(page);
        var dots = carousel.Locator(".rhx-carousel__dot");

        await dots.Nth(2).ClickAsync();
        await Assertions.Expect(dots.Nth(2)).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Keyboard_arrow_advances_slide_on_minimal_carousel(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var carousel = MinimalCarousel(page);
        // Carousel root has no tabindex so we dispatch keydown directly to test
        // the documented keyboard behavior.
        await carousel.EvaluateAsync(
            "el => el.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowRight', bubbles: true }))");

        var track = carousel.Locator(".rhx-carousel__track");
        var transform = await track.EvaluateAsync<string>("el => el.style.transform");
        Assert.Contains("-100", transform);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Minimal_carousel_has_no_nav_or_pagination(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var carousel = MinimalCarousel(page);
        await Assertions.Expect(carousel.Locator(".rhx-carousel__nav-button--prev")).ToHaveCountAsync(0);
        await Assertions.Expect(carousel.Locator(".rhx-carousel__dot")).ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Slide_change_event_fires_on_navigation(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var carousel = GalleryCarousel(page);
        await carousel.EvaluateAsync(
            "el => { window.__rhxLastCarouselIndex = -1; el.addEventListener('rhx:carousel:change', e => { window.__rhxLastCarouselIndex = e.detail.index; }); }");

        await carousel.Locator(".rhx-carousel__nav-button--next").ClickAsync();

        var index = await page.EvaluateAsync<int>("() => window.__rhxLastCarouselIndex");
        Assert.Equal(1, index);
    }
}
