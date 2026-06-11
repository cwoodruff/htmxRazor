using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class RadialSelectTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/RadialSelect";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Renders_trigger_group_and_hidden_pie_menu(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator(Scope + "div.rhx-radial-select");
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-radial-select", "");

        var trigger = wrapper.Locator("button.rhx-radial-select__trigger");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "menu");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        var pie = wrapper.Locator(".rhx-radial-select__pie");
        await Assertions.Expect(pie).Not.ToBeVisibleAsync();
        await Assertions.Expect(pie.Locator("[role='menuitemradio']")).ToHaveCountAsync(4);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Default_category_wedge_is_checked_initially(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var fruit = page.Locator(Scope + "[role='menuitemradio'][data-rhx-radial-option-value='fruit']");
        await Assertions.Expect(fruit).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_trigger_opens_pie(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator(Scope + "button.rhx-radial-select__trigger");
        var pie = page.Locator(Scope + ".rhx-radial-select__pie");

        await trigger.ClickAsync();
        await Assertions.Expect(pie).ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Selecting_wedge_cascades_and_autoselects_first(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator(Scope + "button.rhx-radial-select__trigger");
        await trigger.ClickAsync();

        var meat = page.Locator(Scope + "[role='menuitemradio'][data-rhx-radial-option-value='meat']");
        await meat.ClickAsync();

        // Listbox repopulates with the meat category's options...
        var listbox = page.Locator(Scope + ".rhx-radial-select__listbox");
        var firstOption = listbox.Locator("[role='option']").First;
        await Assertions.Expect(firstOption).ToContainTextAsync("Chicken");
        // ...and the first option is auto-selected.
        await Assertions.Expect(firstOption).ToHaveAttributeAsync("aria-selected", "true");

        // The chosen category is reflected in the hidden category input and on the trigger.
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-radial-category]")).ToHaveValueAsync("meat");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("data-rhx-active-color", "warning");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_closes_pie_and_restores_focus(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator(Scope + "button.rhx-radial-select__trigger");
        var pie = page.Locator(Scope + ".rhx-radial-select__pie");

        await trigger.ClickAsync();
        await Assertions.Expect(pie).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(pie).Not.ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToBeFocusedAsync();
    }
}
