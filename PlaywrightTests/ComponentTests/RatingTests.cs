using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class RatingTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Rating";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_rating_renders_radiogroup_with_five_radios(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-rating");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);

        var group = wrapper.Locator("[role='radiogroup']");
        await Assertions.Expect(group).ToHaveCountAsync(1);

        await Assertions.Expect(
            wrapper.Locator("input[type='radio'][name='rating']")
        ).ToHaveCountAsync(5);

        // Star labels accompany the radios.
        await Assertions.Expect(
            wrapper.Locator("label.rhx-rating__star")
        ).ToHaveCountAsync(5);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_third_star_checks_third_radio(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-rating");
        var thirdRadio = wrapper.Locator("input[type='radio'][value='3']");
        var thirdLabel = wrapper.Locator("label.rhx-rating__star[for$='-3']");

        await thirdLabel.ClickAsync();

        await Assertions.Expect(thirdRadio).ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Preset_value_checks_third_radio(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-preset-preview div.rhx-rating");
        var thirdRadio = wrapper.Locator("input[type='radio'][name='rating-pre'][value='3']");
        await Assertions.Expect(thirdRadio).ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Half_star_readonly_renders_three_full_and_one_half(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-half-preview div.rhx-rating");
        await Assertions.Expect(wrapper).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("rhx-rating--readonly"));

        // Readonly display uses static span stars, no radios.
        await Assertions.Expect(
            wrapper.Locator("input[type='radio']")
        ).ToHaveCountAsync(0);

        await Assertions.Expect(
            wrapper.Locator("span.rhx-rating__star--filled")
        ).ToHaveCountAsync(3);
        await Assertions.Expect(
            wrapper.Locator("span.rhx-rating__star--half")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_max_renders_ten_radios(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-max-preview div.rhx-rating");

        await Assertions.Expect(
            wrapper.Locator("input[type='radio'][name='rating-10']")
        ).ToHaveCountAsync(10);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Readonly_rating_shows_filled_stars_and_no_radios(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-readonly-preview div.rhx-rating");
        await Assertions.Expect(wrapper).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("rhx-rating--readonly"));

        await Assertions.Expect(
            wrapper.Locator("input[type='radio']")
        ).ToHaveCountAsync(0);

        // value="4" → 4 filled stars.
        await Assertions.Expect(
            wrapper.Locator("span.rhx-rating__star--filled")
        ).ToHaveCountAsync(4);

        var hidden = wrapper.Locator("input[type='hidden'][name='rating-ro']");
        await Assertions.Expect(hidden).ToHaveValueAsync("4");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_rating_has_modifier_static_stars_and_keeps_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-states-preview div.rhx-rating");
        await Assertions.Expect(wrapper).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("rhx-rating--disabled"));

        await Assertions.Expect(
            wrapper.Locator("input[type='radio']")
        ).ToHaveCountAsync(0);

        var hidden = wrapper.Locator("input[type='hidden'][name='rt-dis']");
        await Assertions.Expect(hidden).ToHaveValueAsync("2");
    }
}
