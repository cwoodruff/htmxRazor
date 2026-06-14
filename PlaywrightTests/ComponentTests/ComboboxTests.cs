using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-combobox renders a native <input list> bound to a <datalist> (no JS).
// Filtering, the suggestion popup, and keyboard support are native browser behavior.
public sealed class ComboboxTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Combobox";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_combobox_renders_input_bound_to_datalist(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-combobox");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);

        var input = wrapper.Locator("input.rhx-combobox__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("placeholder", "Search cities...");
        await Assertions.Expect(input).ToHaveAttributeAsync("autocomplete", "off");

        // The input's `list` points to a sibling <datalist> with the same id.
        var listId = await input.GetAttributeAsync("list");
        Assert.False(string.IsNullOrEmpty(listId));
        await Assertions.Expect(wrapper.Locator($"datalist#{listId}")).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Child_options_become_datalist_suggestions(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-children-preview div.rhx-combobox");
        var options = wrapper.Locator("datalist > option");
        await Assertions.Expect(options).ToHaveCountAsync(5);
        // Datalist option value IS the display text (native autocomplete has no hidden code).
        await Assertions.Expect(wrapper.Locator("datalist > option[value='Cherry']")).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Typing_a_suggestion_sets_the_input_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-children-preview input.rhx-combobox__control[name='fruit']");
        await input.FillAsync("Cherry");
        await Assertions.Expect(input).ToHaveValueAsync("Cherry");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Filled_variant_has_filled_modifier_class(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-filled-preview div.rhx-combobox");
        await Assertions.Expect(wrapper).ToHaveClassAsync(new Regex("rhx-combobox--filled"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_combobox_input_is_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-states-preview div.rhx-combobox.rhx-combobox--disabled");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);
        await Assertions.Expect(wrapper.Locator("input.rhx-combobox__control")).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Readonly_combobox_input_has_readonly_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-states-preview div.rhx-combobox.rhx-combobox--readonly");
        var input = wrapper.Locator("input.rhx-combobox__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("readonly", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Server_filter_fetches_options_into_datalist_on_type(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-htmx-preview div.rhx-combobox");
        var input = wrapper.Locator("input.rhx-combobox__control[name='userId']");

        await input.FillAsync("a");
        // htmx swaps server-rendered <option>s into the datalist.
        await Assertions.Expect(wrapper.Locator("datalist > option").First).ToHaveCountAsync(1);
        await Assertions.Expect(wrapper.Locator("datalist > option[value='Alice Johnson']")).ToHaveCountAsync(1);
    }
}
