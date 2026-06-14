using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-date-range-picker renders two native <input type="date"> controls (start + end), no JS.
public sealed class DateRangePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/DateRangePicker";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Renders_two_native_date_inputs_with_their_names(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var start = page.Locator(Scope + "input.rhx-date-range-picker__start");
        var end = page.Locator(Scope + "input.rhx-date-range-picker__end");

        await Assertions.Expect(start).ToHaveAttributeAsync("type", "date");
        await Assertions.Expect(start).ToHaveAttributeAsync("name", "From");
        await Assertions.Expect(end).ToHaveAttributeAsync("type", "date");
        await Assertions.Expect(end).ToHaveAttributeAsync("name", "To");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Both_inputs_accept_values(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var start = page.Locator(Scope + "input.rhx-date-range-picker__start");
        var end = page.Locator(Scope + "input.rhx-date-range-picker__end");

        await start.FillAsync("2026-10-13");
        await end.FillAsync("2026-10-17");

        await Assertions.Expect(start).ToHaveValueAsync("2026-10-13");
        await Assertions.Expect(end).ToHaveValueAsync("2026-10-17");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Group_is_labelled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var group = page.Locator(Scope + ".rhx-date-range-picker__control[role='group']");
        await Assertions.Expect(group).ToHaveCountAsync(1);
    }
}
