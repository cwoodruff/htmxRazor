using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class ComboboxTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Combobox";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_combobox_renders_text_input_and_hidden_listbox(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-combobox");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-combobox", "");

        var input = wrapper.Locator("input[role='combobox']");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-autocomplete", "list");
        await Assertions.Expect(input).ToHaveAttributeAsync("placeholder", "Search cities...");

        var listbox = wrapper.Locator("[role='listbox']");
        await Assertions.Expect(listbox).Not.ToBeVisibleAsync();

        var hidden = wrapper.Locator("input[type='hidden'][name='city']");
        await Assertions.Expect(hidden).ToHaveValueAsync("");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Focusing_the_input_opens_the_listbox(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-children-preview div.rhx-combobox");
        var input = wrapper.Locator("input[role='combobox']");
        var listbox = wrapper.Locator("[role='listbox']");

        await input.FocusAsync();
        await Assertions.Expect(listbox).ToBeVisibleAsync();
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Typing_filters_options_client_side(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-children-preview div.rhx-combobox");
        var input = wrapper.Locator("input[role='combobox']");

        await input.ClickAsync();
        await input.FillAsync("ban");

        var visibleOptions = wrapper.Locator("[role='option']:visible");
        await Assertions.Expect(visibleOptions).ToHaveCountAsync(1);
        await Assertions.Expect(visibleOptions.First).ToContainTextAsync("Banana");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_an_option_commits_value_to_hidden_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-children-preview div.rhx-combobox");
        var input = wrapper.Locator("input[role='combobox']");
        var hidden = wrapper.Locator("input[type='hidden'][name='fruit']");

        await input.ClickAsync();
        await wrapper.Locator("[role='option'][data-value='cherry']").ClickAsync();

        await Assertions.Expect(hidden).ToHaveValueAsync("cherry");
        await Assertions.Expect(input).ToHaveValueAsync("Cherry");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Filled_variant_has_filled_modifier_class(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-filled-preview div.rhx-combobox");
        await Assertions.Expect(wrapper).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-combobox--filled"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_combobox_input_is_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-states-preview div.rhx-combobox.rhx-combobox--disabled");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);

        var input = wrapper.Locator("input[role='combobox']");
        await Assertions.Expect(input).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Readonly_combobox_input_has_readonly_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-states-preview div.rhx-combobox.rhx-combobox--readonly");
        var input = wrapper.Locator("input[role='combobox']");
        await Assertions.Expect(input).ToHaveAttributeAsync("readonly", "");
    }
}
