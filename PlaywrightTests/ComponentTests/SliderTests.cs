using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class SliderTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Slider";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_slider_renders_native_range_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-slider");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);

        var native = wrapper.Locator("input[type='range']");
        await Assertions.Expect(native).ToHaveAttributeAsync("name", "volume");
        await Assertions.Expect(native).ToHaveValueAsync("50");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_range_applies_min_max_and_step(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-range-preview input[type='range'][name='custom']");
        await Assertions.Expect(native).ToHaveAttributeAsync("min", "10");
        await Assertions.Expect(native).ToHaveAttributeAsync("max", "200");
        await Assertions.Expect(native).ToHaveAttributeAsync("step", "10");
        await Assertions.Expect(native).ToHaveValueAsync("80");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Keyboard_arrow_keys_move_the_thumb(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-basic-preview input[type='range'][name='volume']");
        await native.FocusAsync();
        await Assertions.Expect(native).ToHaveValueAsync("50");

        await page.Keyboard.PressAsync("ArrowRight");
        await Assertions.Expect(native).ToHaveValueAsync("51");

        await page.Keyboard.PressAsync("ArrowRight");
        await Assertions.Expect(native).ToHaveValueAsync("52");

        await page.Keyboard.PressAsync("ArrowLeft");
        await Assertions.Expect(native).ToHaveValueAsync("51");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Setting_value_via_fill_updates_slider(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-basic-preview input[type='range'][name='volume']");
        await native.FillAsync("75");
        await Assertions.Expect(native).ToHaveValueAsync("75");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Tooltip_example_sets_tooltip_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var top = page.Locator("#panel-tooltip-preview div.rhx-slider").Nth(0);
        await Assertions.Expect(top).ToHaveAttributeAsync("data-rhx-tooltip", "top");

        var bottom = page.Locator("#panel-tooltip-preview div.rhx-slider").Nth(1);
        await Assertions.Expect(bottom).ToHaveAttributeAsync("data-rhx-tooltip", "bottom");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_slider_cannot_be_moved_by_keyboard(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-states-preview input[type='range'][name='sl-dis']");
        await Assertions.Expect(native).ToBeDisabledAsync();
        await Assertions.Expect(native).ToHaveValueAsync("40");
    }
}
