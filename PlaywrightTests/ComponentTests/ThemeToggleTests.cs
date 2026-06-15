using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// The dark-mode toggle is zero-JS: a sr-only checkbox drives CSS html:has(...) and the choice
// is persisted in the rhx-theme cookie via an htmx POST (rendered back checked server-side).
public sealed class ThemeToggleTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Button";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Toggle_applies_dark_tokens_and_persists_across_reload(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var checkbox = page.Locator(".rhx-theme-toggle .rhx-theme-controller").First;
        await Assertions.Expect(checkbox).Not.ToBeCheckedAsync();

        var bgLight = await page.EvaluateAsync<string>(
            "() => getComputedStyle(document.documentElement).getPropertyValue('--rhx-color-bg')");

        // Clicking the label toggles the wrapped checkbox → CSS :has() applies the dark tokens.
        await page.Locator(".rhx-theme-toggle").First.ClickAsync();
        await Assertions.Expect(checkbox).ToBeCheckedAsync();

        var bgDark = await page.EvaluateAsync<string>(
            "() => getComputedStyle(document.documentElement).getPropertyValue('--rhx-color-bg')");
        Assert.NotEqual(bgLight.Trim(), bgDark.Trim());

        // The change POSTed to /_rhx/theme set the cookie; after reload the toggle renders checked.
        await page.ReloadAsync();
        await Assertions.Expect(
            page.Locator(".rhx-theme-toggle .rhx-theme-controller").First).ToBeCheckedAsync();

        // Reset so the persisted cookie doesn't leak into other tests sharing this context.
        await page.Locator(".rhx-theme-toggle").First.ClickAsync();
    }
}
