using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class CheckboxTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Checkbox";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_checkbox_renders_native_input_with_label(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-checkbox");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-checkbox", "");

        var native = wrapper.Locator("input[type='checkbox']");
        await Assertions.Expect(native).ToHaveAttributeAsync("name", "agree");
        await Assertions.Expect(native).Not.ToBeCheckedAsync();

        await Assertions.Expect(wrapper.Locator("span.rhx-checkbox__text"))
            .ToHaveTextAsync("I agree to the terms and conditions");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_label_toggles_checked_state(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var label = page.Locator("#panel-basic-preview label.rhx-checkbox__label");
        var native = page.Locator("#panel-basic-preview input[type='checkbox'][name='agree']");

        await Assertions.Expect(native).Not.ToBeCheckedAsync();
        await label.ClickAsync();
        await Assertions.Expect(native).ToBeCheckedAsync();
        await label.ClickAsync();
        await Assertions.Expect(native).Not.ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Prechecked_checkbox_is_initially_checked(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-prechecked-preview input[type='checkbox'][name='newsletter']");
        await Assertions.Expect(native).ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Hint_is_rendered_and_linked_via_aria_describedby(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-hint-preview input[type='checkbox'][name='marketing']");
        var describedBy = await native.GetAttributeAsync("aria-describedby");
        Assert.False(string.IsNullOrEmpty(describedBy));

        var hint = page.Locator($"#panel-hint-preview #{describedBy}");
        await Assertions.Expect(hint).ToContainTextAsync("We'll only send relevant updates");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Indeterminate_checkbox_has_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-indeterminate-preview div.rhx-checkbox");
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-indeterminate", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_checkbox_cannot_be_toggled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var disabled = page.Locator("#panel-states-preview input[type='checkbox'][name='cb-dis']");
        await Assertions.Expect(disabled).ToBeDisabledAsync();
        await Assertions.Expect(disabled).Not.ToBeCheckedAsync();

        var disabledChecked = page.Locator("#panel-states-preview input[type='checkbox'][name='cb-dis-checked']");
        await Assertions.Expect(disabledChecked).ToBeDisabledAsync();
        await Assertions.Expect(disabledChecked).ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sizes_render_small_and_large_modifiers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-sizes-preview div.rhx-checkbox.rhx-checkbox--small")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview div.rhx-checkbox.rhx-checkbox--large")
        ).ToHaveCountAsync(1);
    }
}
