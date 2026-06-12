using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DateTimePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/DateTimePicker";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Picking_a_day_then_a_time_commits_iso_and_closes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator(Scope + ".rhx-datetime-picker__input");
        var popup = page.Locator(Scope + ".rhx-datetime-picker__popup");
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();

        await input.ClickAsync();
        await Assertions.Expect(popup).ToBeVisibleAsync();

        await popup.Locator(".rhx-calendar__day:not(.rhx-calendar__day--muted):not([disabled])").First.ClickAsync();
        // popup stays open until a time is also chosen; hidden not yet committed
        await Assertions.Expect(popup).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-dt-value]")).ToHaveValueAsync("");

        await popup.Locator(".rhx-datetime-picker__times [role='option'][data-time='09:30']").ClickAsync();
        await Assertions.Expect(popup).Not.ToBeVisibleAsync();
        await Assertions.Expect(input).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-dt-value]")).ToHaveValueAsync(new Regex(@"^\d{4}-\d{2}-\d{2}T09:30$"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Placeholder_renders_a_literal_ampersand_not_a_double_encoded_entity(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        // Regression: the demo markup wrote "&amp;" which the tag helper re-encoded,
        // surfacing a literal "&amp;" in the field. The placeholder must show a real "&".
        var input = page.Locator(Scope + ".rhx-datetime-picker__input");
        await Assertions.Expect(input).ToHaveAttributeAsync("placeholder", "Pick date & time…");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Month_navigation_swaps_calendar_and_keeps_time_pane(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-datetime-picker__input").ClickAsync();

        var grid = page.Locator(Scope + ".rhx-calendar__grid");
        var before = await grid.GetAttributeAsync("aria-label");
        await page.Locator(Scope + ".rhx-calendar__nav[aria-label='Next month']").ClickAsync();
        await Assertions.Expect(grid).Not.ToHaveAttributeAsync("aria-label", before!);
        await Assertions.Expect(page.Locator(Scope + ".rhx-datetime-picker__times [role='option']").First).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clear_resets_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.Locator(Scope + ".rhx-datetime-picker__input").ClickAsync();
        var popup = page.Locator(Scope + ".rhx-datetime-picker__popup");

        await popup.Locator(".rhx-calendar__day:not(.rhx-calendar__day--muted):not([disabled])").First.ClickAsync();
        await popup.Locator("[data-rhx-dt-clear]").ClickAsync();
        await Assertions.Expect(page.Locator(Scope + ".rhx-datetime-picker__input")).ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-dt-value]")).ToHaveValueAsync("");
    }
}
