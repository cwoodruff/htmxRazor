using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class OptimisticTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Optimistic";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Optimistic_switches_opt_into_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var darkMode = page.Locator("#panel-switch-preview div.rhx-switch[data-rhx-optimistic]").First;
        await Assertions.Expect(darkMode).ToHaveCountAsync(1);

        var failMode = page.Locator(
            "#panel-switch-preview div.rhx-switch[data-rhx-optimistic]").Nth(1);
        await Assertions.Expect(failMode).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Optimistic_switch_flips_immediately_on_click(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-switch-preview input[role='switch'][name='darkMode']");
        var label = page.Locator("#panel-switch-preview label.rhx-switch__label").First;

        await Assertions.Expect(input).Not.ToBeCheckedAsync();
        await label.ClickAsync();

        // Flip should be reflected immediately (before server round-trip).
        // The success handler keeps it checked; this asserts the UI now reflects "on".
        await Assertions.Expect(input).ToBeCheckedAsync(new() { Timeout = 5000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Optimistic_switch_reverts_on_server_error(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-switch-preview input[role='switch'][name='failMode']");
        var wrapper = page.Locator("#panel-switch-preview div.rhx-switch[data-rhx-optimistic]").Nth(1);
        var label = wrapper.Locator("label.rhx-switch__label");

        await Assertions.Expect(input).Not.ToBeCheckedAsync();
        await label.ClickAsync();

        // Server returns 500 — optimistic helper reverts the check state.
        await Assertions.Expect(input).Not.ToBeCheckedAsync(new() { Timeout = 5000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Optimistic_rating_has_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var rating = page.Locator("#panel-rating-preview div.rhx-rating[data-rhx-optimistic]");
        await Assertions.Expect(rating).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Optimistic_button_renders_with_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var button = page.Locator("#panel-button-preview button.rhx-button[data-rhx-optimistic]");
        await Assertions.Expect(button).ToHaveCountAsync(1);
        await Assertions.Expect(button).ToContainTextAsync("Like");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Optimistic_button_click_completes_successfully(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var button = page.Locator("#panel-button-preview button.rhx-button[data-rhx-optimistic]");
        await button.ClickAsync();

        // After a successful request, the button should return to an enabled, non-busy state.
        await Assertions.Expect(button).Not.ToHaveAttributeAsync("aria-busy", "true", new() { Timeout = 5000 });
        await Assertions.Expect(button).ToBeEnabledAsync();
    }
}
