using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-datetime-picker renders a native <input type="datetime-local"> (no JS).
public sealed class DateTimePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/DateTimePicker";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_renders_native_datetime_input_and_accepts_a_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator(Scope + "input.rhx-datetime-picker__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("type", "datetime-local");
        await Assertions.Expect(input).ToHaveAttributeAsync("name", "StartsAt");
        await Assertions.Expect(input).ToHaveAttributeAsync("step", "1800"); // 30 min

        await input.FillAsync("2026-10-15T09:30");
        await Assertions.Expect(input).ToHaveValueAsync("2026-10-15T09:30");
    }
}
