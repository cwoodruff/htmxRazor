using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-select renders a native <select> (no JS). Listbox/keyboard/type-ahead are the browser's.
public sealed class SelectTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Select";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_select_renders_native_control_with_placeholder(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-select");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);

        var control = wrapper.Locator("select.rhx-select__control");
        await Assertions.Expect(control).ToHaveCountAsync(1);

        // Placeholder is an empty-value option, selected while nothing is chosen.
        var placeholder = control.Locator("option[value='']");
        await Assertions.Expect(placeholder).ToContainTextAsync("Choose a country");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Selecting_an_option_updates_select_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var control = page.Locator("#panel-children-preview select.rhx-select__control[name='color']");
        await Assertions.Expect(control).ToHaveValueAsync("");

        await control.SelectOptionAsync(new SelectOptionValue { Value = "green" });
        await Assertions.Expect(control).ToHaveValueAsync("green");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_option_is_natively_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var control = page.Locator("#panel-children-preview select.rhx-select__control[name='color']");
        var purple = control.Locator("option[value='purple']");
        Assert.True(await purple.EvaluateAsync<bool>("el => el.disabled"),
            "The purple option should be natively disabled.");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Multi_select_renders_native_multiple(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-multi-preview div.rhx-select");
        await Assertions.Expect(wrapper).ToHaveClassAsync(new Regex("rhx-select--multiple"));

        var control = wrapper.Locator("select.rhx-select__control");
        await Assertions.Expect(control).ToHaveAttributeAsync("multiple", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Preselected_enum_value_is_selected(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var control = page.Locator("#panel-enum-preview select.rhx-select__control[name='priority']");
        await Assertions.Expect(control).ToHaveValueAsync("Medium");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_and_readonly_states(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var disabled = page.Locator("#panel-states-preview select.rhx-select__control[name='country-disabled']");
        await Assertions.Expect(disabled).ToBeDisabledAsync();

        // Readonly is emulated with a disabled select + a hidden mirror that still submits.
        var readonlyWrap = page.Locator("#panel-states-preview div.rhx-select").Nth(1);
        await Assertions.Expect(readonlyWrap.Locator("select")).ToBeDisabledAsync();
        await Assertions.Expect(
            readonlyWrap.Locator("input[type='hidden'][name='country-readonly']")).ToHaveValueAsync("US");
    }

    // Issue #14 (now native): rhx-required enforces HTML form validation directly on the
    // native <select>; an empty required select reports valueMissing and blocks submit.
    [Theory, MemberData(nameof(Browsers))]
    public async Task Required_select_blocks_native_form_submission_until_a_value_is_chosen(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var scope = page.Locator("#panel-required-preview");
        var control = scope.Locator("select.rhx-select__control[name='required-country']");

        await Assertions.Expect(control).ToHaveAttributeAsync("required", "");
        Assert.True(await control.EvaluateAsync<bool>("el => el.validity.valueMissing"),
            "Empty required select should report valueMissing=true.");

        var urlBefore = page.Url;
        await scope.Locator("button[type='submit']").ClickAsync();
        await page.WaitForTimeoutAsync(150);
        Assert.Equal(urlBefore, page.Url);

        // Choosing a real value clears the constraint violation.
        await control.SelectOptionAsync(new SelectOptionValue { Index = 1 });
        Assert.False(await control.EvaluateAsync<bool>("el => el.validity.valueMissing"),
            "After selecting a value the required select should be valid.");
    }
}
