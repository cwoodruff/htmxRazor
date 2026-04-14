using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class IconTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Icon";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Gallery_renders_up_to_twenty_four_icons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var icons = page.Locator("#panel-gallery-preview svg.rhx-icon");
        await Assertions.Expect(icons).ToHaveCountAsync(24);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Gallery_icons_are_decorative_with_aria_hidden(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var firstIcon = page.Locator("#panel-gallery-preview svg.rhx-icon").First;
        await Assertions.Expect(firstIcon).ToHaveAttributeAsync("aria-hidden", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sizes_example_renders_three_icons_including_modifiers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-sizes-preview svg.rhx-icon")
        ).ToHaveCountAsync(3);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview svg.rhx-icon.rhx-icon--small")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview svg.rhx-icon.rhx-icon--large")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Accessible_icons_expose_role_img_with_labels(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var icons = page.Locator("#panel-accessible-preview svg.rhx-icon");
        await Assertions.Expect(icons).ToHaveCountAsync(3);

        foreach (var label in new[] { "Settings", "Notifications", "User profile" })
        {
            var icon = page.Locator($"#panel-accessible-preview svg.rhx-icon[aria-label='{label}']");
            await Assertions.Expect(icon).ToHaveCountAsync(1);
            await Assertions.Expect(icon).ToHaveAttributeAsync("role", "img");
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Icons_include_inline_svg_path_content(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        // Any rendered icon should have at least one child element inside its SVG.
        var firstIcon = page.Locator("#panel-sizes-preview svg.rhx-icon").First;
        var childCount = await firstIcon.Locator("*").CountAsync();
        Assert.True(childCount > 0, "Expected svg icon to contain child elements");
    }
}
