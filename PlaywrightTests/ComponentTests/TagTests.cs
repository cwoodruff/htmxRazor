using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class TagTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Tag";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Variants_render_five_tags_with_modifier_classes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var tags = page.Locator("#panel-variants-preview span.rhx-tag");
        await Assertions.Expect(tags).ToHaveCountAsync(5);

        foreach (var variant in new[] { "neutral", "brand", "success", "warning", "danger" })
        {
            await Assertions.Expect(
                page.Locator($"#panel-variants-preview span.rhx-tag.rhx-tag--{variant}")
            ).ToHaveCountAsync(1);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Tag_content_is_wrapped_in_label_element(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var labels = page.Locator("#panel-variants-preview span.rhx-tag span.rhx-tag__label");
        await Assertions.Expect(labels).ToHaveCountAsync(5);
        await Assertions.Expect(labels.First).ToHaveTextAsync("Neutral");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sizes_example_uses_small_and_large_modifiers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var tags = page.Locator("#panel-sizes-preview span.rhx-tag");
        await Assertions.Expect(tags).ToHaveCountAsync(3);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview span.rhx-tag.rhx-tag--small")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview span.rhx-tag.rhx-tag--large")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Pill_example_renders_three_pill_tags(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var pills = page.Locator("#panel-pill-preview span.rhx-tag.rhx-tag--pill");
        await Assertions.Expect(pills).ToHaveCountAsync(3);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Removable_tags_render_remove_buttons_with_aria_label(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var tags = page.Locator("#panel-removable-preview span.rhx-tag.rhx-tag--removable");
        await Assertions.Expect(tags).ToHaveCountAsync(3);

        var buttons = page.Locator("#panel-removable-preview span.rhx-tag button.rhx-tag__remove");
        await Assertions.Expect(buttons).ToHaveCountAsync(3);
        await Assertions.Expect(buttons.First).ToHaveAttributeAsync("aria-label", "Remove");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Htmx_delete_removes_tag_when_close_button_clicked(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var tag = page.Locator("#panel-htmx-preview span.rhx-tag");
        await Assertions.Expect(tag).ToHaveCountAsync(1);

        await page.Locator("#panel-htmx-preview span.rhx-tag button.rhx-tag__remove").ClickAsync();

        await Assertions.Expect(page.Locator("#panel-htmx-preview span.rhx-tag"))
            .ToHaveCountAsync(0, new() { Timeout = 5000 });
    }
}
