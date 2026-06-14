using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class SplitPanelTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/SplitPanel";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Horizontal_renders_start_end_separator_and_resizable_start(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var split = page.Locator("#panel-horizontal-preview .rhx-split-panel").First;
        var start = split.Locator(".rhx-split-panel__start");
        await Assertions.Expect(start).ToHaveCountAsync(1);
        await Assertions.Expect(split.Locator(".rhx-split-panel__end")).ToHaveCountAsync(1);

        var divider = split.Locator(".rhx-split-panel__divider");
        await Assertions.Expect(divider).ToHaveAttributeAsync("role", "separator");
        await Assertions.Expect(divider).ToHaveAttributeAsync("aria-orientation", "vertical");

        // Resizing is the native CSS resize grip on the start panel — no JS.
        await Assertions.Expect(start).ToHaveCSSAsync("resize", "horizontal");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Vertical_mode_sets_horizontal_aria_and_vertical_resize(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var split = page.Locator("#panel-vertical-preview .rhx-split-panel").First;
        await Assertions.Expect(split).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("rhx-split-panel--vertical"));

        await Assertions.Expect(split.Locator(".rhx-split-panel__divider"))
            .ToHaveAttributeAsync("aria-orientation", "horizontal");
        await Assertions.Expect(split.Locator(".rhx-split-panel__start"))
            .ToHaveCSSAsync("resize", "vertical");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_split_panel_is_not_resizable(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var split = page.Locator("#panel-disabled-preview .rhx-split-panel").First;
        await Assertions.Expect(split).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("rhx-split-panel--disabled"));
        await Assertions.Expect(split.Locator(".rhx-split-panel__start"))
            .ToHaveCSSAsync("resize", "none");
    }
}
