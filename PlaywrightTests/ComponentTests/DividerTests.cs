using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DividerTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Divider";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Horizontal_divider_renders_as_hr_element(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var divider = page.Locator("#panel-horizontal-preview hr.rhx-divider");
        await Assertions.Expect(divider).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Horizontal_divider_does_not_use_vertical_modifier(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-horizontal-preview hr.rhx-divider.rhx-divider--vertical")
        ).ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Vertical_divider_renders_as_div_with_vertical_modifier(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var divider = page.Locator("#panel-vertical-preview div.rhx-divider.rhx-divider--vertical");
        await Assertions.Expect(divider).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Vertical_divider_exposes_separator_role_and_orientation(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var divider = page.Locator("#panel-vertical-preview div.rhx-divider--vertical");
        await Assertions.Expect(divider).ToHaveAttributeAsync("role", "separator");
        await Assertions.Expect(divider).ToHaveAttributeAsync("aria-orientation", "vertical");
    }
}
