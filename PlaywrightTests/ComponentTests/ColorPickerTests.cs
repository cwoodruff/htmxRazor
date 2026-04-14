using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class ColorPickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/ColorPicker";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_picker_renders_trigger_with_initial_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var picker = page.Locator("#panel-basic-preview [data-rhx-color-picker]");
        await Assertions.Expect(picker).ToHaveCountAsync(1);

        var trigger = picker.Locator(".rhx-color-picker__trigger");
        await Assertions.Expect(trigger).ToBeVisibleAsync();
        await Assertions.Expect(trigger.Locator(".rhx-color-picker__text")).ToContainTextAsync("#3b82f6");

        var hidden = picker.Locator("input.rhx-color-picker__value");
        await Assertions.Expect(hidden).ToHaveValueAsync("#3b82f6");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_picker_panel_opens_on_trigger_click(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var picker = page.Locator("#panel-basic-preview [data-rhx-color-picker]");
        var trigger = picker.Locator(".rhx-color-picker__trigger");
        var panel = picker.Locator(".rhx-color-picker__panel");

        // Panel hidden by default
        await Assertions.Expect(panel).ToBeHiddenAsync();

        await trigger.ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await Assertions.Expect(panel.Locator(".rhx-color-picker__saturation")).ToBeVisibleAsync();
        await Assertions.Expect(panel.Locator(".rhx-color-picker__hue-input")).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Swatches_panel_shows_preset_buttons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var picker = page.Locator("#panel-swatches-preview [data-rhx-color-picker]");
        await picker.Locator(".rhx-color-picker__trigger").ClickAsync();

        var presets = picker.Locator(".rhx-color-picker__preset");
        await Assertions.Expect(presets).ToHaveCountAsync(8);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_preset_updates_hidden_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var picker = page.Locator("#panel-swatches-preview [data-rhx-color-picker]");
        var hidden = picker.Locator("input.rhx-color-picker__value");
        await Assertions.Expect(hidden).ToHaveValueAsync("#ef4444");

        await picker.Locator(".rhx-color-picker__trigger").ClickAsync();
        // Pick the 5th swatch (#3b82f6 blue)
        await picker.Locator(".rhx-color-picker__preset").Nth(4).ClickAsync();

        // hidden value should change to non-red
        await Assertions.Expect(hidden).Not.ToHaveValueAsync("#ef4444");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Opacity_picker_has_opacity_slider(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var picker = page.Locator("#panel-opacity-preview [data-rhx-color-picker]");
        await Assertions.Expect(picker).ToHaveAttributeAsync("data-rhx-opacity", "");
        await picker.Locator(".rhx-color-picker__trigger").ClickAsync();
        await Assertions.Expect(picker.Locator(".rhx-color-picker__opacity-input")).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Inline_picker_has_panel_visible_without_trigger(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var picker = page.Locator("#panel-inline-preview [data-rhx-color-picker]");
        await Assertions.Expect(picker).ToHaveAttributeAsync("data-rhx-inline", "");
        // No trigger for inline pickers
        await Assertions.Expect(picker.Locator(".rhx-color-picker__trigger")).ToHaveCountAsync(0);
        await Assertions.Expect(picker.Locator(".rhx-color-picker__panel")).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_picker_has_disabled_trigger(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var picker = page.Locator("#panel-states-preview [data-rhx-color-picker]");
        var trigger = picker.Locator(".rhx-color-picker__trigger");
        await Assertions.Expect(trigger).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Click_outside_closes_basic_panel(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var picker = page.Locator("#panel-basic-preview [data-rhx-color-picker]");
        var panel = picker.Locator(".rhx-color-picker__panel");

        await picker.Locator(".rhx-color-picker__trigger").ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();

        // Click on the body, outside any picker
        await page.Locator("body").ClickAsync(new() { Position = new() { X = 5, Y = 5 } });
        await Assertions.Expect(panel).ToBeHiddenAsync();
    }
}
