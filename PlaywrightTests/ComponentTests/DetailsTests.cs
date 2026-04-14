using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DetailsTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Details";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_details_renders_as_native_element(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var details = page.Locator("#panel-basic-preview details.rhx-details").First;
        await Assertions.Expect(details).ToBeVisibleAsync();
        await Assertions.Expect(details).ToHaveJSPropertyAsync("open", false);

        var summary = details.Locator("summary.rhx-details__summary");
        await Assertions.Expect(summary).ToContainTextAsync("What is htmxRazor?");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_summary_toggles_open_state(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var details = page.Locator("#panel-basic-preview details.rhx-details").First;
        var summary = details.Locator("summary.rhx-details__summary");

        // Initially closed.
        await Assertions.Expect(details).ToHaveJSPropertyAsync("open", false);
        // Content exists but native <details> hides it when not open.
        var content = details.Locator(".rhx-details__content");
        await Assertions.Expect(content).Not.ToBeVisibleAsync();

        await summary.ClickAsync();
        await Assertions.Expect(details).ToHaveJSPropertyAsync("open", true);
        await Assertions.Expect(content).ToBeVisibleAsync();

        await summary.ClickAsync();
        await Assertions.Expect(details).ToHaveJSPropertyAsync("open", false);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Open_by_default_renders_with_open_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var details = page.Locator("#panel-open-preview details.rhx-details").First;
        await Assertions.Expect(details).ToHaveJSPropertyAsync("open", true);
        await Assertions.Expect(details.Locator(".rhx-details__content")).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Faq_renders_three_independent_items(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var items = page.Locator("#panel-faq-preview details.rhx-details");
        await Assertions.Expect(items).ToHaveCountAsync(3);

        var second = items.Nth(1);
        await second.Locator("summary").ClickAsync();
        await Assertions.Expect(second).ToHaveJSPropertyAsync("open", true);

        // First item remains independently closed.
        await Assertions.Expect(items.First).ToHaveJSPropertyAsync("open", false);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_details_has_disabled_marker(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var details = page.Locator("#panel-disabled-preview details.rhx-details").First;
        await Assertions.Expect(details).ToHaveAttributeAsync("data-rhx-disabled", "");
        await Assertions.Expect(details).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-details--disabled"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Keyboard_enter_toggles_details(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var details = page.Locator("#panel-basic-preview details.rhx-details").First;
        var summary = details.Locator("summary.rhx-details__summary");

        await summary.FocusAsync();
        await page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(details).ToHaveJSPropertyAsync("open", true);
    }
}
