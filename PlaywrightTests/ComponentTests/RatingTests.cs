using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class RatingTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Rating";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_rating_renders_five_stars_and_hidden_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-rating");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-max", "5");

        await Assertions.Expect(
            wrapper.Locator("span.rhx-rating__star")
        ).ToHaveCountAsync(5);

        var hidden = wrapper.Locator("input[type='hidden'][name='rating']");
        await Assertions.Expect(hidden).ToHaveValueAsync("0");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_third_star_sets_value_to_three(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-rating");
        var hidden = wrapper.Locator("input[type='hidden'][name='rating']");
        var thirdStar = wrapper.Locator("span.rhx-rating__star[data-value='3']");

        await thirdStar.ClickAsync();
        await Assertions.Expect(hidden).ToHaveValueAsync("3");
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("aria-valuenow", "3");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Preset_value_renders_first_three_stars_filled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-preset-preview div.rhx-rating");
        var hidden = wrapper.Locator("input[type='hidden'][name='rating-pre']");
        await Assertions.Expect(hidden).ToHaveValueAsync("3");

        await Assertions.Expect(
            wrapper.Locator("span.rhx-rating__star--filled")
        ).ToHaveCountAsync(3);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Half_star_example_renders_three_full_and_one_half(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-half-preview div.rhx-rating");
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-precision", "0.5");

        await Assertions.Expect(
            wrapper.Locator("span.rhx-rating__star--filled")
        ).ToHaveCountAsync(3);
        await Assertions.Expect(
            wrapper.Locator("span.rhx-rating__star--half")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_max_renders_ten_stars(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-max-preview div.rhx-rating");
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-max", "10");

        await Assertions.Expect(
            wrapper.Locator("span.rhx-rating__star")
        ).ToHaveCountAsync(10);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Readonly_rating_does_not_update_on_click(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-readonly-preview div.rhx-rating");
        await Assertions.Expect(wrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-rating--readonly"));

        var hidden = wrapper.Locator("input[type='hidden'][name='rating-ro']");
        await Assertions.Expect(hidden).ToHaveValueAsync("4");

        await wrapper.Locator("span.rhx-rating__star[data-value='1']").ClickAsync();
        // Readonly — value should not change
        await Assertions.Expect(hidden).ToHaveValueAsync("4");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_rating_has_disabled_modifier_and_keeps_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-states-preview div.rhx-rating");
        await Assertions.Expect(wrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-rating--disabled"));

        var hidden = wrapper.Locator("input[type='hidden'][name='rt-dis']");
        await Assertions.Expect(hidden).ToHaveValueAsync("2");
    }
}
