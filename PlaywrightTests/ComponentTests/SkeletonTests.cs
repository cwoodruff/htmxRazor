using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class SkeletonTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Skeleton";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Effects_example_renders_three_skeletons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var skeletons = page.Locator("#panel-effects-preview div.rhx-skeleton");
        await Assertions.Expect(skeletons).ToHaveCountAsync(3);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Effects_use_pulse_and_sheen_modifiers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-effects-preview div.rhx-skeleton.rhx-skeleton--pulse")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-effects-preview div.rhx-skeleton.rhx-skeleton--sheen")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Shapes_example_uses_rectangle_and_circle_modifiers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var skeletons = page.Locator("#panel-shapes-preview div.rhx-skeleton");
        await Assertions.Expect(skeletons).ToHaveCountAsync(3);

        await Assertions.Expect(
            page.Locator("#panel-shapes-preview div.rhx-skeleton.rhx-skeleton--rectangle")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-shapes-preview div.rhx-skeleton.rhx-skeleton--circle")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Skeletons_are_hidden_from_assistive_technology(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var first = page.Locator("#panel-effects-preview div.rhx-skeleton").First;
        await Assertions.Expect(first).ToHaveAttributeAsync("aria-hidden", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Skeleton_applies_inline_width_and_height_styles(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var first = page.Locator("#panel-effects-preview div.rhx-skeleton").First;
        var style = await first.GetAttributeAsync("style");
        Assert.NotNull(style);
        Assert.Contains("width:", style);
        Assert.Contains("height:", style);
        Assert.Contains("200px", style);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Placeholder_example_renders_five_skeleton_shapes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var skeletons = page.Locator("#panel-placeholder-preview div.rhx-skeleton");
        await Assertions.Expect(skeletons).ToHaveCountAsync(5);
    }
}
