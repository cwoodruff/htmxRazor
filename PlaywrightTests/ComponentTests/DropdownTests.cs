using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DropdownTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Dropdowns";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_dropdown_renders_with_trigger_and_hidden_panel(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-basic-preview [data-rhx-dropdown]");
        await Assertions.Expect(dropdown).ToHaveCountAsync(1);

        var trigger = dropdown.Locator("button").First;
        await Assertions.Expect(trigger).ToBeVisibleAsync();

        var panel = dropdown.Locator("[role='menu']");
        await Assertions.Expect(panel).ToBeHiddenAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_dropdown_opens_on_trigger_click(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-basic-preview [data-rhx-dropdown]");
        var trigger = dropdown.Locator("button").First;
        var panel = dropdown.Locator("[role='menu']");

        await trigger.ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

        var items = panel.Locator("[role='menuitem']");
        await Assertions.Expect(items).Not.ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Panel_opens_directly_below_trigger_not_at_viewport_bottom(string browserName)
    {
        // Regression guard for the recent CSS-based positioning fix:
        // the panel must appear immediately below the trigger, not absolutely
        // positioned at the bottom of the viewport.
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-basic-preview [data-rhx-dropdown]").First;
        var trigger = dropdown.Locator("button").First;

        await trigger.ClickAsync();

        var panel = dropdown.Locator("[role='menu']");
        await Assertions.Expect(panel).ToBeVisibleAsync();

        var triggerBox = await trigger.BoundingBoxAsync();
        var panelBox = await panel.BoundingBoxAsync();

        Assert.NotNull(triggerBox);
        Assert.NotNull(panelBox);

        // Panel top edge must sit just below the trigger bottom (within ~32px margin),
        // proving CSS placement is anchoring to the trigger, not the viewport.
        var gap = panelBox!.Y - (triggerBox!.Y + triggerBox.Height);
        Assert.InRange(gap, -2, 32);

        // And horizontally aligned with the trigger (within a reasonable tolerance)
        Assert.InRange(panelBox.X, triggerBox.X - 20, triggerBox.X + triggerBox.Width + 20);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_item_closes_panel(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-basic-preview [data-rhx-dropdown]");
        var trigger = dropdown.Locator("button").First;
        var panel = dropdown.Locator("[role='menu']");

        await trigger.ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();

        await panel.Locator("[role='menuitem']", new() { HasTextString = "Edit" }).ClickAsync();
        await Assertions.Expect(panel).ToBeHiddenAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_closes_panel(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-basic-preview [data-rhx-dropdown]");
        var trigger = dropdown.Locator("button").First;
        var panel = dropdown.Locator("[role='menu']");

        await trigger.ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(panel).ToBeHiddenAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Placements_render_three_dropdowns(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdowns = page.Locator("#panel-placements-preview [data-rhx-dropdown]");
        await Assertions.Expect(dropdowns).ToHaveCountAsync(3);

        await Assertions.Expect(
            page.Locator("#panel-placements-preview [data-rhx-placement='bottom-start']")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-placements-preview [data-rhx-placement='bottom-end']")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-placements-preview [data-rhx-placement='top-start']")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Checkbox_dropdown_stays_open_after_toggle(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-checkbox-preview [data-rhx-dropdown]");
        var trigger = dropdown.Locator("button").First;
        var panel = dropdown.Locator("[role='menu']");

        await trigger.ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();

        var phoneItem = panel.Locator("[role='menuitemcheckbox']", new() { HasTextString = "Phone" });
        await Assertions.Expect(phoneItem).ToHaveAttributeAsync("aria-checked", "false");
        await phoneItem.ClickAsync();

        // Panel should remain open (stay-open mode)
        await Assertions.Expect(panel).ToBeVisibleAsync();
        await Assertions.Expect(phoneItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task States_open_dropdown_is_visible_on_load(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        // The "Open" variant has rhx-open="true"
        var openDropdown = page.Locator("#panel-states-preview [data-rhx-dropdown].rhx-dropdown--open");
        await Assertions.Expect(openDropdown).ToHaveCountAsync(1);
        await Assertions.Expect(openDropdown.Locator("[role='menu']")).ToBeVisibleAsync();
    }
}
