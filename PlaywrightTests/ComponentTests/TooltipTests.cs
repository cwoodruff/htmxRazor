using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class TooltipTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Tooltip";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Tooltip_wrappers_carry_content_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrappers = page.Locator("#panel-basic-preview [data-rhx-tooltip]");
        await Assertions.Expect(wrappers).ToHaveCountAsync(3);
        await Assertions.Expect(wrappers.First).ToHaveAttributeAsync("data-rhx-tooltip", "Save your changes");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Hover_shows_singleton_tooltip_popup(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var saveBtn = page.Locator("#panel-basic-preview [data-rhx-tooltip='Save your changes'] button");
        await saveBtn.HoverAsync();

        var popup = page.Locator("#rhx-tooltip-popup.rhx-tooltip--visible");
        await Assertions.Expect(popup).ToBeVisibleAsync();
        await Assertions.Expect(popup.Locator(".rhx-tooltip__content")).ToContainTextAsync("Save your changes");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Mouseleave_hides_tooltip(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var saveBtn = page.Locator("#panel-basic-preview [data-rhx-tooltip='Save your changes'] button");
        await saveBtn.HoverAsync();
        var popup = page.Locator("#rhx-tooltip-popup");
        await Assertions.Expect(popup).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-tooltip--visible\b"));

        // Move pointer elsewhere
        await page.Mouse.MoveAsync(0, 0);
        await Assertions.Expect(popup).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-tooltip--visible\b"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Focus_shows_tooltip(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var saveBtn = page.Locator("#panel-basic-preview [data-rhx-tooltip='Save your changes'] button");
        await saveBtn.FocusAsync();

        var popup = page.Locator("#rhx-tooltip-popup.rhx-tooltip--visible");
        await Assertions.Expect(popup).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Placements_each_set_data_placement(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        foreach (var placement in new[] { "top", "bottom", "left", "right" })
        {
            var w = page.Locator($"#panel-placements-preview [data-rhx-tooltip-placement='{placement}']");
            await Assertions.Expect(w).ToHaveCountAsync(1);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Click_trigger_tooltip_toggles_on_click(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-click-preview [data-rhx-tooltip-trigger='click']");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);

        var btn = wrapper.Locator("button");
        var popup = page.Locator("#rhx-tooltip-popup");

        await btn.ClickAsync();
        await Assertions.Expect(popup).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-tooltip--visible\b"));

        await btn.ClickAsync();
        await Assertions.Expect(popup).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex(@"\brhx-tooltip--visible\b"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_tooltip_does_not_show_on_hover(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-disabled-preview [data-rhx-tooltip-disabled]");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);

        await wrapper.Locator("button").HoverAsync();

        // No tooltip popup for disabled tooltips. If a popup element exists from a
        // previous test interaction on this page it should not become visible.
        var popup = page.Locator("#rhx-tooltip-popup.rhx-tooltip--visible");
        await Assertions.Expect(popup).ToHaveCountAsync(0);
    }
}
