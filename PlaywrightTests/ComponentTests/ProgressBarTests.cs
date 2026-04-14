using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class ProgressBarTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/ProgressBar";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Determinate_panel_renders_four_progress_bars(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var bars = page.Locator("#panel-determinate-preview div.rhx-progress-bar");
        await Assertions.Expect(bars).ToHaveCountAsync(4);
        await Assertions.Expect(bars.First).ToHaveAttributeAsync("role", "progressbar");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Determinate_progress_bars_have_aria_value_attributes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var bars = page.Locator("#panel-determinate-preview div.rhx-progress-bar");
        await Assertions.Expect(bars.Nth(0)).ToHaveAttributeAsync("aria-valuenow", "0");
        await Assertions.Expect(bars.Nth(1)).ToHaveAttributeAsync("aria-valuenow", "25");
        await Assertions.Expect(bars.Nth(2)).ToHaveAttributeAsync("aria-valuenow", "65");
        await Assertions.Expect(bars.Nth(3)).ToHaveAttributeAsync("aria-valuenow", "100");

        foreach (var i in new[] { 0, 1, 2, 3 })
        {
            await Assertions.Expect(bars.Nth(i)).ToHaveAttributeAsync("aria-valuemin", "0");
            await Assertions.Expect(bars.Nth(i)).ToHaveAttributeAsync("aria-valuemax", "100");
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Label_property_becomes_aria_label(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var bars = page.Locator("#panel-determinate-preview div.rhx-progress-bar");
        await Assertions.Expect(bars.Nth(0)).ToHaveAttributeAsync("aria-label", "Not started");
        await Assertions.Expect(bars.Nth(2)).ToHaveAttributeAsync("aria-label", "Upload progress");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Determinate_bar_contains_track_and_fill_with_percent_label(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var upload = page.Locator("#panel-determinate-preview div.rhx-progress-bar").Nth(2);
        await Assertions.Expect(upload.Locator(".rhx-progress-bar__track")).ToHaveCountAsync(1);

        var fill = upload.Locator(".rhx-progress-bar__fill");
        await Assertions.Expect(fill).ToHaveAttributeAsync(
            "style",
            new System.Text.RegularExpressions.Regex(@"width:\s*65%"));

        await Assertions.Expect(upload.Locator(".rhx-progress-bar__label")).ToHaveTextAsync("65%");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Indeterminate_bar_has_modifier_class_and_no_valuenow(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var bar = page.Locator("#panel-indeterminate-preview div.rhx-progress-bar.rhx-progress-bar--indeterminate");
        await Assertions.Expect(bar).ToHaveCountAsync(1);
        await Assertions.Expect(bar).ToHaveAttributeAsync("role", "progressbar");
        await Assertions.Expect(bar).ToHaveAttributeAsync("aria-label", "Loading");

        // Indeterminate mode omits aria-valuenow entirely.
        var hasValueNow = await bar.EvaluateAsync<bool>("el => el.hasAttribute('aria-valuenow')");
        Assert.False(hasValueNow);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Htmx_start_task_swaps_in_a_progress_bar(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var startBtn = page.Locator("#panel-htmx-preview #progress-container button.rhx-button");
        await Assertions.Expect(startBtn).ToContainTextAsync("Start Task");
        await startBtn.ClickAsync();

        var bar = page.Locator("#panel-htmx-preview #progress-container div.rhx-progress-bar").First;
        await Assertions.Expect(bar).ToHaveAttributeAsync("role", "progressbar", new() { Timeout = 5000 });
    }
}
