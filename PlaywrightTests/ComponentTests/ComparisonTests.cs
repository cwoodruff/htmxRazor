using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class ComparisonTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Comparison";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Default_renders_before_after_layers_and_handle(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var comp = page.Locator("#panel-beforeafter-preview .rhx-comparison").First;
        await Assertions.Expect(comp).ToBeVisibleAsync();

        await Assertions.Expect(comp.Locator(".rhx-comparison__before img")).ToHaveAttributeAsync("alt", "Original photo");
        await Assertions.Expect(comp.Locator(".rhx-comparison__after img")).ToHaveAttributeAsync("alt", "Grayscale version");

        var handle = comp.Locator(".rhx-comparison__handle");
        await Assertions.Expect(handle).ToHaveAttributeAsync("role", "slider");
        await Assertions.Expect(handle).ToHaveAttributeAsync("aria-valuenow", "50");
        await Assertions.Expect(handle).ToHaveAttributeAsync("aria-valuemin", "0");
        await Assertions.Expect(handle).ToHaveAttributeAsync("aria-valuemax", "100");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_position_reflects_in_aria_and_style(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var comp = page.Locator("#panel-position-preview .rhx-comparison").First;
        var handle = comp.Locator(".rhx-comparison__handle");
        await Assertions.Expect(handle).ToHaveAttributeAsync("aria-valuenow", "25");

        var leftStyle = await handle.EvaluateAsync<string>("el => el.style.left");
        Assert.Equal("25%", leftStyle);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Arrow_key_decreases_and_increases_position(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var comp = page.Locator("#panel-beforeafter-preview .rhx-comparison").First;
        var handle = comp.Locator(".rhx-comparison__handle");

        // Drive the keyboard listener directly so focus side-effects can't
        // change the position before we read the starting value.
        async Task<int> ValueAsync() => int.Parse((await handle.GetAttributeAsync("aria-valuenow"))!);
        async Task PressAsync(string key) => await handle.EvaluateAsync(
            $"el => el.dispatchEvent(new KeyboardEvent('keydown', {{ key: '{key}', bubbles: true }}))");

        var start = await ValueAsync();
        await PressAsync("ArrowLeft");
        Assert.Equal(start - 1, await ValueAsync());

        await PressAsync("ArrowRight");
        await PressAsync("ArrowRight");
        Assert.Equal(start + 1, await ValueAsync());
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Home_and_end_jump_to_bounds(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var comp = page.Locator("#panel-beforeafter-preview .rhx-comparison").First;
        var handle = comp.Locator(".rhx-comparison__handle");

        await handle.FocusAsync();
        await page.Keyboard.PressAsync("Home");
        await Assertions.Expect(handle).ToHaveAttributeAsync("aria-valuenow", "0");

        await page.Keyboard.PressAsync("End");
        await Assertions.Expect(handle).ToHaveAttributeAsync("aria-valuenow", "100");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Change_event_fires_on_keyboard_move(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var comp = page.Locator("#panel-beforeafter-preview .rhx-comparison").First;
        await comp.EvaluateAsync(
            "el => { window.__rhxLastComparePos = -1; el.addEventListener('rhx:comparison:change', e => { window.__rhxLastComparePos = e.detail.position; }); }");

        var handle = comp.Locator(".rhx-comparison__handle");
        await handle.FocusAsync();
        var start = int.Parse((await handle.GetAttributeAsync("aria-valuenow"))!);
        await page.Keyboard.PressAsync("ArrowRight");

        var pos = await page.EvaluateAsync<double>("() => window.__rhxLastComparePos");
        Assert.Equal(start + 1, pos);
    }
}
