using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-date-picker renders a native <input type="date"> (no JS). The OS calendar popup can't be
// driven by Playwright, so these assert the native contract and value entry via FillAsync.
public sealed class DatePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/DatePicker";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_renders_native_date_input_and_accepts_a_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-basic-preview input.rhx-date-picker__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("type", "date");
        await Assertions.Expect(input).ToHaveAttributeAsync("name", "DueDate");

        await input.FillAsync("2026-10-15");
        await Assertions.Expect(input).ToHaveValueAsync("2026-10-15");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Min_max_become_native_attributes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-mm-preview input.rhx-date-picker__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("min", "2026-01-01");
        await Assertions.Expect(input).ToHaveAttributeAsync("max", "2026-12-31");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Value_outside_min_max_is_reported_invalid(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-mm-preview input.rhx-date-picker__control");
        await input.FillAsync("2025-06-01"); // before min 2026-01-01
        Assert.True(await input.EvaluateAsync<bool>("el => el.validity.rangeUnderflow"),
            "A date before min should be reported as rangeUnderflow by the native input.");
    }
}
