using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class SplitPanelTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/SplitPanel";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Horizontal_renders_start_end_and_separator(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var split = page.Locator("#panel-horizontal-preview .rhx-split-panel").First;
        await Assertions.Expect(split.Locator(".rhx-split-panel__start")).ToHaveCountAsync(1);
        await Assertions.Expect(split.Locator(".rhx-split-panel__end")).ToHaveCountAsync(1);

        var divider = split.Locator(".rhx-split-panel__divider");
        await Assertions.Expect(divider).ToHaveAttributeAsync("role", "separator");
        await Assertions.Expect(divider).ToHaveAttributeAsync("aria-valuenow", "30");
        await Assertions.Expect(divider).ToHaveAttributeAsync("aria-orientation", "vertical");
        await Assertions.Expect(divider).ToHaveAttributeAsync("tabindex", "0");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Vertical_mode_sets_horizontal_aria_orientation(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var split = page.Locator("#panel-vertical-preview .rhx-split-panel").First;
        await Assertions.Expect(split).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-split-panel--vertical"));

        var divider = split.Locator(".rhx-split-panel__divider");
        await Assertions.Expect(divider).ToHaveAttributeAsync("aria-orientation", "horizontal");
        await Assertions.Expect(divider).ToHaveAttributeAsync("aria-valuenow", "40");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Arrow_key_moves_divider(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var split = page.Locator("#panel-horizontal-preview .rhx-split-panel").First;
        var divider = split.Locator(".rhx-split-panel__divider");

        await divider.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");

        // After one right-arrow press the value should be greater than 30.
        var after = await divider.GetAttributeAsync("aria-valuenow");
        Assert.NotNull(after);
        Assert.True(int.Parse(after!) > 30, $"Expected value > 30 after ArrowRight, got {after}");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Snap_variant_carries_snap_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var split = page.Locator("#panel-snap-preview .rhx-split-panel").First;
        await Assertions.Expect(split).ToHaveAttributeAsync("data-rhx-snap", "25,50,75");
        await Assertions.Expect(split.Locator(".rhx-split-panel__divider"))
            .ToHaveAttributeAsync("aria-valuenow", "50");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_split_panel_has_disabled_class_and_untabbable_divider(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var split = page.Locator("#panel-disabled-preview .rhx-split-panel").First;
        await Assertions.Expect(split).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-split-panel--disabled"));
        await Assertions.Expect(split).ToHaveAttributeAsync("data-rhx-disabled", "");

        var divider = split.Locator(".rhx-split-panel__divider");
        await Assertions.Expect(divider).ToHaveAttributeAsync("tabindex", "-1");
        await Assertions.Expect(divider).ToHaveAttributeAsync("aria-valuenow", "60");
    }
}
