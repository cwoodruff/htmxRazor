using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class SelectTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Select";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_select_renders_trigger_and_hidden_listbox(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-select");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-select", "");

        var trigger = wrapper.Locator("button.rhx-select__trigger");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("role", "combobox");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        var listbox = wrapper.Locator("[role='listbox']");
        await Assertions.Expect(listbox).Not.ToBeVisibleAsync();

        await Assertions.Expect(
            wrapper.Locator("span.rhx-select__placeholder")
        ).ToContainTextAsync("Choose a country");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_trigger_opens_listbox(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-children-preview div.rhx-select");
        var trigger = wrapper.Locator("button.rhx-select__trigger");
        var listbox = wrapper.Locator("[role='listbox']");

        await Assertions.Expect(listbox).Not.ToBeVisibleAsync();
        await trigger.ClickAsync();
        await Assertions.Expect(listbox).ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Selecting_an_option_updates_hidden_input_and_display(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-children-preview div.rhx-select");
        var trigger = wrapper.Locator("button.rhx-select__trigger");
        var hidden = wrapper.Locator("input[type='hidden'][name='color']");

        await Assertions.Expect(hidden).ToHaveValueAsync("");

        await trigger.ClickAsync();
        await wrapper.Locator("[role='option'][data-value='green']").ClickAsync();

        await Assertions.Expect(hidden).ToHaveValueAsync("green");
        await Assertions.Expect(wrapper.Locator("span.rhx-select__value")).ToContainTextAsync("Green");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_option_cannot_be_selected(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-children-preview div.rhx-select");
        var purple = wrapper.Locator("[role='option'][data-value='purple']");
        await Assertions.Expect(purple).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Multi_select_sets_multiselectable_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-multi-preview div.rhx-select");
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-select-multiple", "");
        await Assertions.Expect(wrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-select--multiple"));

        var listbox = wrapper.Locator("[role='listbox']");
        await Assertions.Expect(listbox).ToHaveAttributeAsync("aria-multiselectable", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Preselected_value_is_shown_in_display(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-enum-preview div.rhx-select");
        var hidden = wrapper.Locator("input[type='hidden'][name='priority']");
        await Assertions.Expect(hidden).ToHaveValueAsync("Medium");
        await Assertions.Expect(wrapper.Locator("span.rhx-select__value")).ToContainTextAsync("Medium");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task With_clear_example_shows_clear_button_when_value_set(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-clear-preview div.rhx-select");
        var clearBtn = wrapper.Locator("button.rhx-select__clear");
        await Assertions.Expect(clearBtn).ToBeVisibleAsync();

        var hidden = wrapper.Locator("input[type='hidden'][name='country-clear']");
        await Assertions.Expect(hidden).ToHaveValueAsync("US");

        await clearBtn.ClickAsync();
        await Assertions.Expect(hidden).ToHaveValueAsync("");
    }
}
