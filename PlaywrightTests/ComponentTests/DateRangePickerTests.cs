using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DateRangePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/DateRangePicker";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Picking_two_days_commits_both_hidden_inputs_and_closes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        var popup = page.Locator(Scope + ".rhx-date-range-picker__popup");
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();

        await page.Locator(Scope + ".rhx-date-range-picker__input").ClickAsync();
        await Assertions.Expect(popup).ToBeVisibleAsync();

        var enabled = popup.Locator(".rhx-calendar__day:not(.rhx-calendar__day--muted):not([disabled])");
        await enabled.Nth(5).ClickAsync();   // start
        await Assertions.Expect(popup).ToBeVisibleAsync();   // still open after first pick
        await enabled.Nth(9).ClickAsync();   // end -> commits + closes
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();

        await Assertions.Expect(page.Locator(Scope + "[data-rhx-range-start]")).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-range-end]")).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + ".rhx-date-range-picker__input")).Not.ToHaveValueAsync("");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Preset_last7_sets_both_dates(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-date-range-picker__input").ClickAsync();
        await page.Locator(Scope + "[data-range-preset='last7']").ClickAsync();
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-range-start]")).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-range-end]")).Not.ToHaveValueAsync("");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Two_months_render_and_nav_moves_both(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-date-range-picker__input").ClickAsync();
        await Assertions.Expect(page.Locator(Scope + ".rhx-date-range-picker__month")).ToHaveCountAsync(2);

        var caption = page.Locator(Scope + ".rhx-date-range-picker__cal-caption").First;
        var before = await caption.TextContentAsync();
        await page.Locator(Scope + ".rhx-calendar__nav[aria-label='Next month']").ClickAsync();
        await Assertions.Expect(caption).Not.ToHaveTextAsync(before!);
    }
}
