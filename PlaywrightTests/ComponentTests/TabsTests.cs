using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

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
        var tabs = group.Locator("[role='tablist'] button[role='tab']");
        await Assertions.Expect(tabs).ToHaveCountAsync(3);

        var firstTab = tabs.First;
        await Assertions.Expect(firstTab).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(firstTab).ToHaveAttributeAsync("tabindex", "0");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_tab_activates_matching_panel(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = InnerGroup(page, "panel-basic-preview", "Account settings");
        var generalPanel = group.Locator("#panel-general");
        var profilePanel = group.Locator("#panel-profile");
        var profileTab = group.Locator("[role='tablist'] button[role='tab']", new() { HasTextString = "Profile" });

        await Assertions.Expect(generalPanel).Not.ToHaveAttributeAsync("hidden", "hidden");
        await Assertions.Expect(profilePanel).ToHaveAttributeAsync("hidden", "hidden");

        await profileTab.ClickAsync();

        await Assertions.Expect(profileTab).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(profilePanel).Not.ToHaveAttributeAsync("hidden", "hidden");
        await Assertions.Expect(generalPanel).ToHaveAttributeAsync("hidden", "hidden");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Arrow_key_navigation_moves_active_tab(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = InnerGroup(page, "panel-basic-preview", "Account settings");
        var firstTab = group.Locator("[role='tablist'] button[role='tab']").First;

        await firstTab.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");

        var profileTab = group.Locator("[role='tablist'] button[role='tab']", new() { HasTextString = "Profile" });
        await Assertions.Expect(profileTab).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_tab_has_aria_disabled_and_is_not_interactive(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = InnerGroup(page, "panel-disabled-preview", "Disabled tab example");
        var disabledTab = group.Locator("[role='tablist'] button[role='tab']", new() { HasTextString = "Disabled" });

        await Assertions.Expect(disabledTab).ToHaveAttributeAsync("aria-disabled", "true");
        await Assertions.Expect(disabledTab).ToBeDisabledAsync();
        await Assertions.Expect(disabledTab).ToHaveAttributeAsync("aria-selected", "false");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Closable_tab_can_be_removed(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = InnerGroup(page, "panel-closable-preview", "Closable tabs example");
        var tabs = group.Locator("[role='tablist'] button[role='tab']");
        await Assertions.Expect(tabs).ToHaveCountAsync(3);

        await group.Locator("[role='tablist'] .rhx-tab__close").First.ClickAsync();
        await Assertions.Expect(tabs).ToHaveCountAsync(2);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Manual_activation_separates_focus_from_selection(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = InnerGroup(page, "panel-manual-preview", "Manual activation example");
        var firstTab = group.Locator("[role='tablist'] button[role='tab']").First;
        var secondTab = group.Locator("[role='tablist'] button[role='tab']").Nth(1);

        await firstTab.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");

        // Manual activation: focus moves but selection does not until Enter/Space.
        await Assertions.Expect(firstTab).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(secondTab).ToHaveAttributeAsync("aria-selected", "false");

        await page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(secondTab).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Vertical_placement_sets_aria_orientation(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var startTablist = page.Locator("#panel-placements-preview [role='tablist'][aria-label='Start tabs example']");
        await Assertions.Expect(startTablist).ToHaveAttributeAsync("aria-orientation", "vertical");
    }
}
