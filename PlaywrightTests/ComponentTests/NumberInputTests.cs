using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-number-input renders a native <input type="number"> (no JS). The browser provides the
// increment/decrement spinners; --no-steppers hides them via CSS.
public sealed class NumberInputTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/NumberInput";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_number_input_renders_native_with_constraints(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-number-input");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);

        var native = wrapper.Locator("input[type='number']");
        await Assertions.Expect(native).ToHaveAttributeAsync("name", "quantity");
        await Assertions.Expect(native).ToHaveValueAsync("1");
        await Assertions.Expect(native).ToHaveAttributeAsync("min", "0");
        await Assertions.Expect(native).ToHaveAttributeAsync("max", "99");
        await Assertions.Expect(native).ToHaveAttributeAsync("step", "1");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Native_step_up_and_down_change_the_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-basic-preview input[type='number'][name='quantity']");
        await Assertions.Expect(native).ToHaveValueAsync("1");

        // The browser's native spinner behavior (stepUp/stepDown) — no custom JS buttons.
        await native.EvaluateAsync("el => el.stepUp()");
        await native.DispatchEventAsync("change");
        await Assertions.Expect(native).ToHaveValueAsync("2");

        await native.EvaluateAsync("el => el.stepDown()");
        await native.DispatchEventAsync("change");
        await Assertions.Expect(native).ToHaveValueAsync("1");
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
    public async Task No_steppers_modifier_is_applied(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-nosteppers-preview div.rhx-number-input");
        await Assertions.Expect(wrapper).ToHaveClassAsync(new Regex("rhx-number-input--no-steppers"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_number_input_disables_field(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-states-preview div.rhx-number-input input[type='number']");
        await Assertions.Expect(native).ToBeDisabledAsync();
    }
}
