using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// Tabs are JS-free: each tab is a visually-hidden native radio (.rhx-tab__radio)
// paired with a clickable <label class="rhx-tab">. Selection is native radio
// behavior; panel visibility is driven by CSS :has() on the checked radio.
public sealed class TabsTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Tabs";

    // Selects the inner demo <rhx-tab-group> inside a preview panel by its aria-label.
    private static ILocator InnerGroup(IPage page, string previewPanelId, string ariaLabel) =>
        page.Locator($"#{previewPanelId} div.rhx-tab-group")
            .Filter(new() { Has = page.Locator($"[role='tablist'][aria-label='{ariaLabel}']") });

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_tablist_renders_three_tabs(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = InnerGroup(page, "panel-basic-preview", "Account settings");
        var tabs = group.Locator("[role='tablist'] .rhx-tab");
        await Assertions.Expect(tabs).ToHaveCountAsync(3);

        // The first tab's radio is checked on load.
        var firstRadio = group.Locator("[role='tablist'] .rhx-tab__radio").First;
        await Assertions.Expect(firstRadio).ToBeCheckedAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_tab_activates_matching_panel(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = InnerGroup(page, "panel-basic-preview", "Account settings");
        var generalPanel = group.Locator("#panel-general");
        var profilePanel = group.Locator("#panel-profile");
        var profileTab = group.Locator("[role='tablist'] .rhx-tab", new() { HasTextString = "Profile" });

        await Assertions.Expect(generalPanel).ToBeVisibleAsync();
        await Assertions.Expect(profilePanel).Not.ToBeVisibleAsync();

        await profileTab.ClickAsync();

        var profileRadio = group.Locator("[role='tablist'] .rhx-tab__radio").Nth(1);
        await Assertions.Expect(profileRadio).ToBeCheckedAsync();
        await Assertions.Expect(profilePanel).ToBeVisibleAsync();
        await Assertions.Expect(generalPanel).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Arrow_key_navigation_moves_selection(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = InnerGroup(page, "panel-basic-preview", "Account settings");
        var firstRadio = group.Locator("[role='tablist'] .rhx-tab__radio").First;

        await firstRadio.FocusAsync();
        // Native radio group: ArrowRight/Down moves to and selects the next radio.
        await page.Keyboard.PressAsync("ArrowRight");

        var profileRadio = group.Locator("[role='tablist'] .rhx-tab__radio").Nth(1);
        await Assertions.Expect(profileRadio).ToBeCheckedAsync();
        await Assertions.Expect(group.Locator("#panel-profile")).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_tab_radio_is_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = InnerGroup(page, "panel-disabled-preview", "Disabled tab example");
        var disabledLabel = group.Locator("[role='tablist'] .rhx-tab", new() { HasTextString = "Disabled" });
        await Assertions.Expect(disabledLabel).ToHaveClassAsync(new Regex("rhx-tab--disabled"));

        // The corresponding radio (second) is natively disabled.
        var disabledRadio = group.Locator("[role='tablist'] .rhx-tab__radio").Nth(1);
        await Assertions.Expect(disabledRadio).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Vertical_placement_adds_modifier_class(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var startGroup = InnerGroup(page, "panel-placements-preview", "Start tabs example");
        await Assertions.Expect(startGroup).ToHaveClassAsync(new Regex("rhx-tab-group--start"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Lazy_tab_loads_content_via_htmx_on_change(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = InnerGroup(page, "panel-lazy-preview", "Lazy-loaded tabs example");
        var lazyOnePanel = group.Locator("#panel-lazy-one");

        // Selecting the tab fires hx-get on the radio's change event.
        var lazyTab = group.Locator("[role='tablist'] .rhx-tab", new() { HasTextString = "Lazy Tab 1" });
        await lazyTab.ClickAsync();

        await Assertions.Expect(lazyOnePanel).ToBeVisibleAsync();
        // The spinner placeholder is replaced by server-rendered content.
        await Assertions.Expect(lazyOnePanel).Not.ToContainTextAsync("Loading...");
    }
}
