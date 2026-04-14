using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class RadioGroupTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/RadioGroup";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_radio_group_renders_as_fieldset_with_radios(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var fieldset = page.Locator("#panel-basic-preview fieldset.rhx-radio-group");
        await Assertions.Expect(fieldset).ToHaveCountAsync(1);
        await Assertions.Expect(fieldset).ToHaveAttributeAsync("role", "radiogroup");

        var legend = fieldset.Locator("legend.rhx-radio-group__legend");
        await Assertions.Expect(legend).ToHaveTextAsync("Notification Frequency");

        var radios = fieldset.Locator("input[type='radio'][name='frequency']");
        var count = await radios.CountAsync();
        Assert.True(count >= 2);

        foreach (var radio in await radios.AllAsync())
        {
            await Assertions.Expect(radio).Not.ToBeCheckedAsync();
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Preselected_radio_matches_initial_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var selected = page.Locator("#panel-preselected-preview input[type='radio'][name='frequency-pre'][value='weekly']");
        await Assertions.Expect(selected).ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_radio_label_selects_that_option(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var mdRadio = page.Locator("#panel-inline-preview input[type='radio'][name='size'][value='md']");
        var lgRadio = page.Locator("#panel-inline-preview input[type='radio'][name='size'][value='lg']");

        await Assertions.Expect(mdRadio).ToBeCheckedAsync();
        await Assertions.Expect(lgRadio).Not.ToBeCheckedAsync();

        // Native radios are sr-only, so click the associated label
        var lgLabel = page.Locator("#panel-inline-preview label.rhx-radio", new() { HasTextString = "Large" }).First;
        await lgLabel.ClickAsync();

        await Assertions.Expect(lgRadio).ToBeCheckedAsync();
        await Assertions.Expect(mdRadio).Not.ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Inline_group_renders_inline_modifier(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var fieldset = page.Locator("#panel-inline-preview fieldset.rhx-radio-group.rhx-radio-group--inline");
        await Assertions.Expect(fieldset).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Child_options_render_with_disabled_state(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var radios = page.Locator("#panel-children-preview input[type='radio'][name='color']");
        await Assertions.Expect(radios).ToHaveCountAsync(4);

        var purple = page.Locator("#panel-children-preview input[type='radio'][value='purple']");
        await Assertions.Expect(purple).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_group_disables_all_radios(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var fieldset = page.Locator("#panel-states-preview fieldset.rhx-radio-group--disabled").First;
        await Assertions.Expect(fieldset).ToHaveCountAsync(1);

        var radios = fieldset.Locator("input[type='radio']");
        foreach (var radio in await radios.AllAsync())
        {
            await Assertions.Expect(radio).ToBeDisabledAsync();
        }
    }
}
