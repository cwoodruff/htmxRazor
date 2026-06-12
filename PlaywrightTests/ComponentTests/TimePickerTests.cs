using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class TimePickerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/TimePicker";
    private const string Scope = "#panel-basic-preview ";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Opening_shows_times_and_picking_one_fills_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator(Scope + ".rhx-time-picker__input");
        var listbox = page.Locator(Scope + ".rhx-time-picker__listbox");
        await Assertions.Expect(listbox).Not.ToBeVisibleAsync();

        await input.ClickAsync();
        await Assertions.Expect(listbox).ToBeVisibleAsync();

        await listbox.Locator("[role='option'][data-time='09:30']").ClickAsync();
        await Assertions.Expect(listbox).Not.ToBeVisibleAsync();
        await Assertions.Expect(input).ToHaveValueAsync("9:30 AM");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-time-value]")).ToHaveValueAsync("09:30");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Keyboard_down_and_enter_selects(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        var input = page.Locator(Scope + ".rhx-time-picker__input");
        await input.ClickAsync();
        await Assertions.Expect(page.Locator(Scope + ".rhx-time-picker__listbox")).ToBeVisibleAsync();

        await input.PressAsync("ArrowDown");
        await input.PressAsync("Enter");
        await Assertions.Expect(input).Not.ToHaveValueAsync("");
        await Assertions.Expect(page.Locator(Scope + "[data-rhx-time-value]")).Not.ToHaveValueAsync("");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task TwentyFourHour_range_example_uses_24h_options(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        var scope = "#panel-rng-preview ";
        await page.Locator(scope + ".rhx-time-picker__input").ClickAsync();
        var listbox = page.Locator(scope + ".rhx-time-picker__listbox");
        await Assertions.Expect(listbox).ToBeVisibleAsync();
        await Assertions.Expect(listbox.Locator("[role='option']")).ToHaveCountAsync(33);
        await Assertions.Expect(listbox.Locator("[role='option']").First).ToHaveTextAsync("09:00");
    }
}
