using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

/// <summary>
/// Regression tests for the popover arrow. The arrow is a bordered square rotated 45°;
/// CSS strips the two borders facing into the popover (keyed on
/// <c>data-rhx-current-placement</c>) so it reads as a connected tail rather than a
/// detached diamond. This broke for <c>bottom</c> and <c>left</c> in CSS-anchor-capable
/// browsers because the JS positioning path never set that attribute.
/// </summary>
public sealed class PopoverArrowTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Popover";

    // placement -> the two border sides that must be clipped (width 0) for the tail to connect.
    public static IEnumerable<object[]> PlacementCases()
    {
        foreach (var browser in new[] { "chromium", "firefox", "webkit" })
        {
            yield return new object[] { browser, "#pop-top-trigger", "top", "Top", "Left" };
            yield return new object[] { browser, "#pop-bottom-trigger", "bottom", "Bottom", "Right" };
            yield return new object[] { browser, "#pop-left-trigger", "left", "Left", "Bottom" };
            yield return new object[] { browser, "#pop-right-trigger", "right", "Right", "Top" };
        }
    }

    [Theory, MemberData(nameof(PlacementCases))]
    public async Task Arrow_clips_inner_borders_so_it_connects_to_the_popover(
        string browserName, string triggerId, string placement, string clipA, string clipB)
    {
        var page = await OpenAsync(browserName, Path);

        await page.EvalOnSelectorAsync(triggerId, "el => el.scrollIntoView({block:'center'})");
        await page.Locator(triggerId).ClickAsync();
        await page.WaitForTimeoutAsync(150);

        var arrow = page.Locator(
            $"[data-rhx-popover][data-rhx-trigger='{triggerId}'] .rhx-popover__arrow");

        // The popover must advertise its resolved placement so the clipping CSS can apply.
        var popover = page.Locator($"[data-rhx-popover][data-rhx-trigger='{triggerId}']");
        var current = await popover.GetAttributeAsync("data-rhx-current-placement");
        Assert.False(string.IsNullOrEmpty(current),
            $"data-rhx-current-placement was not set for '{placement}' on {browserName}");

        // The two inner-facing borders must be clipped to 0; the outer two must remain.
        // Returned in fixed order: [top, right, bottom, left].
        var widths = await arrow.EvaluateAsync<double[]>(@"el => {
            const s = getComputedStyle(el);
            return [
                parseFloat(s.borderTopWidth),
                parseFloat(s.borderRightWidth),
                parseFloat(s.borderBottomWidth),
                parseFloat(s.borderLeftWidth),
            ];
        }");

        var idx = new Dictionary<string, int>
        {
            ["Top"] = 0, ["Right"] = 1, ["Bottom"] = 2, ["Left"] = 3,
        };

        Assert.Equal(0, widths[idx[clipA]]);
        Assert.Equal(0, widths[idx[clipB]]);
        var keptTotal = Enumerable.Range(0, 4)
            .Where(i => i != idx[clipA] && i != idx[clipB])
            .Sum(i => widths[i]);
        Assert.True(keptTotal > 0, $"expected the outer borders to remain for '{placement}'");
    }
}
