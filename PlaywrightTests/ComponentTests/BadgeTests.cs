using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class BadgeTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Badge";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Variants_render_five_badges(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var badges = page.Locator("#panel-variants-preview span.rhx-badge");
        await Assertions.Expect(badges).ToHaveCountAsync(5);

        foreach (var variant in new[] { "neutral", "brand", "success", "warning", "danger" })
        {
            await Assertions.Expect(
                page.Locator($"#panel-variants-preview span.rhx-badge.rhx-badge--{variant}")
            ).ToHaveCountAsync(1);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Variant_badges_display_expected_text(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-variants-preview span.rhx-badge.rhx-badge--brand")
        ).ToHaveTextAsync("Brand");
        await Assertions.Expect(
            page.Locator("#panel-variants-preview span.rhx-badge.rhx-badge--danger")
        ).ToHaveTextAsync("Danger");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Pill_badges_render_four_with_pill_modifier(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var pillBadges = page.Locator("#panel-pill-preview span.rhx-badge.rhx-badge--pill");
        await Assertions.Expect(pillBadges).ToHaveCountAsync(4);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Pulse_badges_render_three_with_pulse_modifier(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var pulseBadges = page.Locator("#panel-pulse-preview span.rhx-badge.rhx-badge--pulse");
        await Assertions.Expect(pulseBadges).ToHaveCountAsync(3);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Pulse_example_combines_pill_and_pulse_modifiers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var combined = page.Locator("#panel-pulse-preview span.rhx-badge.rhx-badge--pill.rhx-badge--pulse");
        await Assertions.Expect(combined).ToHaveCountAsync(1);
        await Assertions.Expect(combined).ToHaveTextAsync("Online");
    }
}
