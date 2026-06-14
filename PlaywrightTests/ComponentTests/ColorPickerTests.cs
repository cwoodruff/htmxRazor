using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-color-picker renders a native <input type="color"> (no JS). The OS color UI can't be
// driven by Playwright, so these assert the native contract and value entry via FillAsync.
public sealed class ColorPickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/ColorPicker";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_renders_native_color_input_with_initial_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-basic-preview input.rhx-color-picker__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("type", "color");
        await Assertions.Expect(input).ToHaveAttributeAsync("name", "color");
        await Assertions.Expect(input).ToHaveValueAsync("#3b82f6");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Accepts_a_new_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-basic-preview input.rhx-color-picker__control");
        await input.FillAsync("#22c55e");
        await Assertions.Expect(input).ToHaveValueAsync("#22c55e");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_picker_is_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-states-preview input.rhx-color-picker__control");
        await Assertions.Expect(input).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Model_binding_sets_a_valid_hex_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        // BrandColor model binds; a native color input always reports a 7-char hex value.
        var value = await page.Locator("#panel-binding-preview input.rhx-color-picker__control")
            .InputValueAsync();
        Assert.Matches("^#[0-9a-f]{6}$", value);
    }
}
