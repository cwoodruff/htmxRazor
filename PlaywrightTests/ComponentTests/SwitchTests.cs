using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class SwitchTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Switch";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_switch_renders_native_with_switch_role(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-switch");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-switch", "");

        var native = wrapper.Locator("input[type='checkbox'][role='switch']");
        await Assertions.Expect(native).ToHaveAttributeAsync("name", "darkMode");
        await Assertions.Expect(native).ToHaveAttributeAsync("aria-checked", "false");
        await Assertions.Expect(native).Not.ToBeCheckedAsync();

        await Assertions.Expect(wrapper.Locator("span.rhx-switch__track")).ToHaveCountAsync(1);
        await Assertions.Expect(wrapper.Locator("span.rhx-switch__thumb")).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_label_toggles_switch(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var label = page.Locator("#panel-basic-preview label.rhx-switch__label");
        var native = page.Locator("#panel-basic-preview input[type='checkbox'][name='darkMode']");

        await Assertions.Expect(native).Not.ToBeCheckedAsync();
        await label.ClickAsync();
        await Assertions.Expect(native).ToBeCheckedAsync();
        await label.ClickAsync();
        await Assertions.Expect(native).Not.ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Prechecked_switch_is_on_initially(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-prechecked-preview input[type='checkbox'][name='notifications']");
        await Assertions.Expect(native).ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Hint_is_linked_via_aria_describedby(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-hint-preview input[type='checkbox'][name='autoSave']");
        var describedBy = await native.GetAttributeAsync("aria-describedby");
        Assert.False(string.IsNullOrEmpty(describedBy));

        var hint = page.Locator($"#panel-hint-preview #{describedBy}");
        await Assertions.Expect(hint).ToContainTextAsync("Automatically save changes");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_switches_cannot_be_toggled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var disabled = page.Locator("#panel-states-preview input[role='switch'][name='sw-dis']");
        await Assertions.Expect(disabled).ToBeDisabledAsync();
        await Assertions.Expect(disabled).Not.ToBeCheckedAsync();

        var disabledOn = page.Locator("#panel-states-preview input[role='switch'][name='sw-dis-on']");
        await Assertions.Expect(disabledOn).ToBeDisabledAsync();
        await Assertions.Expect(disabledOn).ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sizes_render_small_and_large_modifiers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-sizes-preview div.rhx-switch.rhx-switch--small")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview div.rhx-switch.rhx-switch--large")
        ).ToHaveCountAsync(1);
    }
}
