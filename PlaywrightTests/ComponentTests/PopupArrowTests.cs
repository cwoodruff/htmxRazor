using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

/// <summary>
/// Regression tests for the popup arrow. Like the popover, the arrow is a bordered
/// square rotated 45°; CSS strips the two borders facing into the popup (keyed on
/// <c>data-rhx-current-placement</c>) so it reads as a connected tail. In
/// CSS-anchor-capable browsers the popup's CSS-anchor branch previously returned
/// early without setting that attribute or positioning the arrow, so the arrow sat
/// at the popup's 0,0 corner and never pointed at the anchor.
/// </summary>
public sealed class PopupArrowTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Popup";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Arrow_popup_advertises_resolved_placement_and_clips_inner_borders(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#popup-arrow-anchor").ClickAsync();
        var popup = page.Locator("#demo-popup-arrow");
        await Assertions.Expect(popup).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(150);

        // The popup must advertise its resolved placement so the clipping CSS can apply.
        var current = await popup.GetAttributeAsync("data-rhx-current-placement");
        Assert.False(string.IsNullOrEmpty(current),
            $"data-rhx-current-placement was not set on {browserName}");
        Assert.StartsWith("bottom", current);

        // Bottom placement clips the bottom + right borders; the outer two remain.
        // Returned in fixed order: [top, right, bottom, left].
        var arrow = popup.Locator(".rhx-popup__arrow");
        var widths = await arrow.EvaluateAsync<double[]>(@"el => {
            const s = getComputedStyle(el);
            return [
                parseFloat(s.borderTopWidth),
                parseFloat(s.borderRightWidth),
                parseFloat(s.borderBottomWidth),
                parseFloat(s.borderLeftWidth),
            ];
        }");

        Assert.Equal(0, widths[2]); // bottom clipped
        Assert.Equal(0, widths[1]); // right clipped
        Assert.True(widths[0] > 0 && widths[3] > 0, "outer (top/left) borders should remain");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Arrow_points_at_anchor_along_the_top_edge_of_the_popup(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#popup-arrow-anchor").ClickAsync();
        var popup = page.Locator("#demo-popup-arrow");
        await Assertions.Expect(popup).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(150);

        var popupBox = await popup.BoundingBoxAsync();
        var arrowBox = await popup.Locator(".rhx-popup__arrow").BoundingBoxAsync();
        var anchorBox = await page.Locator("#popup-arrow-anchor").BoundingBoxAsync();
        Assert.NotNull(popupBox);
        Assert.NotNull(arrowBox);
        Assert.NotNull(anchorBox);

        // For bottom placement the arrow sits at the popup's top edge (pointing up),
        // not abandoned at the popup's top-left corner.
        Assert.InRange(arrowBox!.Y, popupBox!.Y - 8, popupBox.Y + 8);

        // And it is centered on the anchor, not pinned to the popup's left edge.
        var arrowCenterX = arrowBox.X + arrowBox.Width / 2;
        var anchorCenterX = anchorBox!.X + anchorBox.Width / 2;
        Assert.InRange(Math.Abs(arrowCenterX - anchorCenterX), 0, 20);
    }
}
