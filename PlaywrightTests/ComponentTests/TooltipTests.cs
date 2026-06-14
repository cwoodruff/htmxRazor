using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class TooltipTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Tooltip";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_renders_wrappers_with_hidden_bubbles(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrappers = page.Locator("#panel-basic-preview .rhx-tooltip-wrap");
        await Assertions.Expect(wrappers).ToHaveCountAsync(3);

        // Bubbles exist but are hidden until hover/focus.
        var bubble = wrappers.First.Locator(".rhx-tooltip");
        await Assertions.Expect(bubble).ToContainTextAsync("Save your changes");
        await Assertions.Expect(bubble).ToHaveCSSAsync("visibility", "hidden");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Hover_shows_bubble(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrap = page.Locator("#panel-basic-preview .rhx-tooltip-wrap").First;
        await wrap.HoverAsync();
        await Assertions.Expect(wrap.Locator(".rhx-tooltip")).ToHaveCSSAsync("visibility", "visible");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Focus_shows_bubble(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrap = page.Locator("#panel-focus-preview .rhx-tooltip-wrap").First;
        await wrap.Locator("button").FocusAsync();
        await Assertions.Expect(wrap.Locator(".rhx-tooltip")).ToHaveCSSAsync("visibility", "visible");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Placements_each_set_modifier_class(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        foreach (var placement in new[] { "top", "bottom", "left", "right" })
        {
            await Assertions.Expect(
                page.Locator($"#panel-placements-preview .rhx-tooltip-wrap--{placement}"))
                .ToHaveCountAsync(1);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_tooltip_renders_no_bubble(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrap = page.Locator("#panel-disabled-preview .rhx-tooltip-wrap--disabled");
        await Assertions.Expect(wrap).ToHaveCountAsync(1);
        await Assertions.Expect(wrap.Locator(".rhx-tooltip")).ToHaveCountAsync(0);
    }
}
