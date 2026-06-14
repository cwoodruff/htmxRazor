using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DropdownTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Dropdowns";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_dropdown_renders_with_popover_trigger_and_hidden_panel(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-basic-preview .rhx-dropdown");
        await Assertions.Expect(dropdown).ToHaveCountAsync(1);

        // Trigger is a popover invoker (button[popovertarget]).
        var trigger = dropdown.Locator("button[popovertarget]").First;
        await Assertions.Expect(trigger).ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "menu");

        // Menu is a popover and starts closed (hidden).
        var panel = dropdown.Locator("[role='menu']");
        await Assertions.Expect(panel).ToHaveAttributeAsync("popover", "auto");
        await Assertions.Expect(panel).ToBeHiddenAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_dropdown_opens_on_trigger_click(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-basic-preview .rhx-dropdown");
        var trigger = dropdown.Locator("button[popovertarget]").First;
        var panel = dropdown.Locator("[role='menu']");

        await trigger.ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();

        var items = panel.Locator("[role='menuitem']");
        await Assertions.Expect(items).Not.ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Panel_opens_near_trigger_not_at_viewport_edge(string browserName)
    {
        // The panel is anchor-positioned to the trigger via CSS Anchor Positioning
        // (with a stacked-below fallback). Either way it should appear adjacent to
        // the trigger, not pinned at an arbitrary viewport edge.
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-basic-preview .rhx-dropdown").First;
        var trigger = dropdown.Locator("button[popovertarget]").First;

        await trigger.ClickAsync();

        var panel = dropdown.Locator("[role='menu']");
        await Assertions.Expect(panel).ToBeVisibleAsync();

        var triggerBox = await trigger.BoundingBoxAsync();
        var panelBox = await panel.BoundingBoxAsync();

        Assert.NotNull(triggerBox);
        Assert.NotNull(panelBox);

        // Panel top edge should sit just below the trigger bottom (within margin),
        // proving placement anchors to the trigger.
        var gap = panelBox!.Y - (triggerBox!.Y + triggerBox.Height);
        Assert.InRange(gap, -2, 32);

        // And horizontally aligned with the trigger (within a reasonable tolerance).
        Assert.InRange(panelBox.X, triggerBox.X - 20, triggerBox.X + triggerBox.Width + 20);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_closes_panel(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-basic-preview .rhx-dropdown");
        var trigger = dropdown.Locator("button[popovertarget]").First;
        var panel = dropdown.Locator("[role='menu']");

        await trigger.ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(panel).ToBeHiddenAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Outside_click_closes_panel(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-basic-preview .rhx-dropdown");
        var trigger = dropdown.Locator("button[popovertarget]").First;
        var panel = dropdown.Locator("[role='menu']");

        await trigger.ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();

        // Native light-dismiss: clicking outside closes an auto popover.
        await page.Mouse.ClickAsync(5, 5);
        await Assertions.Expect(panel).ToBeHiddenAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Placements_render_three_dropdowns(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdowns = page.Locator("#panel-placements-preview .rhx-dropdown");
        await Assertions.Expect(dropdowns).ToHaveCountAsync(3);

        await Assertions.Expect(
            page.Locator("#panel-placements-preview .rhx-dropdown[data-rhx-placement='bottom-start']")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-placements-preview .rhx-dropdown[data-rhx-placement='bottom-end']")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-placements-preview .rhx-dropdown[data-rhx-placement='top-start']")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Checkbox_dropdown_renders_checkbox_items(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dropdown = page.Locator("#panel-checkbox-preview .rhx-dropdown");
        var trigger = dropdown.Locator("button[popovertarget]").First;
        var panel = dropdown.Locator("[role='menu']");

        await trigger.ClickAsync();
        await Assertions.Expect(panel).ToBeVisibleAsync();

        // Checkbox items render with their initial aria-checked state.
        // (Interactive toggling was dropped along with the component JS.)
        var phoneItem = panel.Locator("[role='menuitemcheckbox']", new() { HasTextString = "Phone" });
        await Assertions.Expect(phoneItem).ToHaveAttributeAsync("aria-checked", "false");
        var nameItem = panel.Locator("[role='menuitemcheckbox']", new() { HasTextString = "Name" });
        await Assertions.Expect(nameItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    // Note: there is no "open on load" test — a popover cannot be shown on initial load
    // without JavaScript (the `open` attribute does not apply to popovers), so rhx-open is a
    // no-op in the JS-free dropdown.
}
