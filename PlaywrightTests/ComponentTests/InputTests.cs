using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class InputTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Input";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_input_renders_label_and_native_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-input");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);

        var label = wrapper.Locator("label.rhx-input__label");
        await Assertions.Expect(label).ToHaveTextAsync("Full Name");

        var native = wrapper.Locator("input.rhx-input__native");
        await Assertions.Expect(native).ToHaveAttributeAsync("type", "text");
        await Assertions.Expect(native).ToHaveAttributeAsync("name", "name");
        await Assertions.Expect(native).ToHaveAttributeAsync("placeholder", "Enter your name");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Typing_updates_the_input_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-basic-preview input[name='name']");
        await native.FillAsync("Ada Lovelace");
        await Assertions.Expect(native).ToHaveValueAsync("Ada Lovelace");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Types_example_renders_email_password_and_number_inputs(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-types-preview input[name='email'][type='email']")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-types-preview input[name='password'][type='password']")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-types-preview input[name='number'][type='number']")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_input_is_not_interactive(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var disabled = page.Locator("#panel-states-preview input[name='disabled']");
        await Assertions.Expect(disabled).ToBeDisabledAsync();
        await Assertions.Expect(disabled).ToHaveValueAsync("Cannot edit");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Readonly_input_cannot_be_edited(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var ro = page.Locator("#panel-states-preview input[name='readonly']");
        await Assertions.Expect(ro).ToHaveAttributeAsync("readonly", "");
        await Assertions.Expect(ro).ToHaveValueAsync("Read only value");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sizes_render_small_medium_and_large_modifiers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-sizes-preview div.rhx-input.rhx-input--small")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview div.rhx-input.rhx-input--large")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Label_for_attribute_targets_native_input_id(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var label = page.Locator("#panel-basic-preview label.rhx-input__label");
        var forValue = await label.GetAttributeAsync("for");
        Assert.Equal("name", forValue);

        var native = page.Locator("#panel-basic-preview input[name='name']");
        await Assertions.Expect(native).ToHaveAttributeAsync("id", "name");
    }
}
