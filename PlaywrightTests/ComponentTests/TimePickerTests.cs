using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-time-picker renders a native <input type="time"> (no JS). step is minutes → seconds.
public sealed class TimePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/TimePicker";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_renders_native_time_input_and_accepts_a_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-basic-preview input.rhx-time-picker__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("type", "time");
        await Assertions.Expect(input).ToHaveAttributeAsync("name", "StartTime");
        await Assertions.Expect(input).ToHaveAttributeAsync("step", "1800"); // 30 min

        await input.FillAsync("09:30");
        await Assertions.Expect(input).ToHaveValueAsync("09:30");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Business_hours_example_sets_step_min_max(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-rng-preview input.rhx-time-picker__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("step", "900"); // 15 min
        await Assertions.Expect(input).ToHaveAttributeAsync("min", "09:00");
        await Assertions.Expect(input).ToHaveAttributeAsync("max", "17:00");
    }
}
