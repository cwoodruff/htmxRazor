using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class NumberInputTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/NumberInput";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_number_input_renders_native_with_constraints(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-number-input");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-number-input", "");

        var native = wrapper.Locator("input[type='number']");
        await Assertions.Expect(native).ToHaveAttributeAsync("name", "quantity");
        await Assertions.Expect(native).ToHaveValueAsync("1");
        await Assertions.Expect(native).ToHaveAttributeAsync("min", "0");
        await Assertions.Expect(native).ToHaveAttributeAsync("max", "99");
        await Assertions.Expect(native).ToHaveAttributeAsync("step", "1");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Stepper_buttons_increment_and_decrement_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-number-input");
        var native = wrapper.Locator("input[type='number']");
        var increment = wrapper.Locator("button.rhx-number-input__increment");
        var decrement = wrapper.Locator("button.rhx-number-input__decrement");

        await Assertions.Expect(native).ToHaveValueAsync("1");

        await increment.ClickAsync();
        await Assertions.Expect(native).ToHaveValueAsync("2");

        await increment.ClickAsync();
        await Assertions.Expect(native).ToHaveValueAsync("3");

        await decrement.ClickAsync();
        await Assertions.Expect(native).ToHaveValueAsync("2");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Typing_a_value_updates_the_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-basic-preview input[type='number'][name='quantity']");
        await native.FillAsync("42");
        await Assertions.Expect(native).ToHaveValueAsync("42");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Decimal_step_example_allows_decimal_values(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-decimal-preview input[type='number'][name='price']");
        await Assertions.Expect(native).ToHaveAttributeAsync("step", "0.01");
        await Assertions.Expect(native).ToHaveValueAsync("9.99");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task No_steppers_modifier_hides_stepper_buttons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-nosteppers-preview div.rhx-number-input");
        await Assertions.Expect(wrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-number-input--no-steppers"));

        await Assertions.Expect(
            wrapper.Locator("button.rhx-number-input__increment")
        ).ToHaveCountAsync(0);
        await Assertions.Expect(
            wrapper.Locator("button.rhx-number-input__decrement")
        ).ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_number_input_disables_field_and_steppers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-states-preview div.rhx-number-input");
        var native = wrapper.Locator("input[type='number']");
        await Assertions.Expect(native).ToBeDisabledAsync();
        await Assertions.Expect(
            wrapper.Locator("button.rhx-number-input__increment")
        ).ToBeDisabledAsync();
        await Assertions.Expect(
            wrapper.Locator("button.rhx-number-input__decrement")
        ).ToBeDisabledAsync();
    }
}
