using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DatePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/DatePicker";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Opening_shows_calendar_and_picking_a_day_fills_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator(Scope + ".rhx-date-picker__input");
        var popup = page.Locator(Scope + ".rhx-date-picker__popup");
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();

        await page.Locator(Scope + ".rhx-date-picker__trigger").ClickAsync();
        await Assertions.Expect(popup).ToBeVisibleAsync();

        await popup.Locator(".rhx-calendar__day:not(.rhx-calendar__day--muted):not([disabled])").First.ClickAsync();
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();
        await Assertions.Expect(input).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-date-value]")).Not.ToHaveValueAsync("");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Next_month_navigation_swaps_grid_via_htmx(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-date-picker__trigger").ClickAsync();

        var grid = page.Locator(Scope + ".rhx-calendar__grid");
        var before = await grid.GetAttributeAsync("aria-label");
        await page.Locator(Scope + ".rhx-calendar__nav[aria-label='Next month']").ClickAsync();
        await Assertions.Expect(grid).Not.ToHaveAttributeAsync("aria-label", before!);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Label_opens_month_then_year_views(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-date-picker__trigger").ClickAsync();

        await page.Locator(Scope + ".rhx-calendar__label").ClickAsync();
        await Assertions.Expect(page.Locator(Scope + ".rhx-calendar__months")).ToBeVisibleAsync();
    }

    /// <summary>
    /// Verifies that days outside the min/max range are rendered as disabled.
    ///
    /// The mm-preview picker has rhx-min="2026-01-01" rhx-max="2026-12-31".
    /// Today (2026-06-11) falls inside the range, so the initially-displayed month
    /// (June 2026) contains NO disabled days — asserting .First on disabled would
    /// find nothing and fail non-deterministically.
    ///
    /// Fix: navigate backward via the "Previous month" button (which is the same
    /// htmx nav already proven working in Next_month_navigation_swaps_grid_via_htmx)
    /// until the grid shows January 2026. January's grid includes overflow days from
    /// December 2025 (before min 2026-01-01), which are rendered [disabled].
    /// Using prev-nav avoids the months-picker intermediate view and is more robust.
    /// </summary>
    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_days_outside_min_max_are_not_selectable(string browserName)
    {
        var page = await OpenAsync(browserName, "/Docs/Components/DatePicker");
        var scope = "#panel-mm-preview ";

        // Open the calendar popup.
        await page.Locator(scope + ".rhx-date-picker__trigger").ClickAsync();

        // Navigate backward month by month until we reach January 2026.
        // The grid's aria-label is "MMMM yyyy" (e.g. "June 2026"). We keep clicking
        // "Previous month" until the label changes to "January 2026".
        // Maximum guard: 24 iterations (generous — we start at most in Dec 2027).
        var grid = page.Locator(scope + ".rhx-calendar__grid");
        var prevBtn = page.Locator(scope + ".rhx-calendar__nav[aria-label='Previous month']");
        for (var i = 0; i < 24; i++)
        {
            var label = await grid.GetAttributeAsync("aria-label");
            if (label == "January 2026") break;
            await prevBtn.ClickAsync();
            await Assertions.Expect(grid).Not.ToHaveAttributeAsync("aria-label", label!);
        }

        // Confirm we're now on January 2026.
        await Assertions.Expect(grid).ToHaveAttributeAsync("aria-label", "January 2026");

        // January 2026 grid contains overflow days from December 2025 (< 2026-01-01 min),
        // which are rendered with [disabled].
        var disabled = page.Locator(scope + ".rhx-calendar__day[disabled]").First;
        await Assertions.Expect(disabled).ToBeDisabledAsync();
    }
}
