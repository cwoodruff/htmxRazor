using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class TimelineTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Timeline";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_timeline_renders_four_items_as_list(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var timeline = page.Locator("#panel-basic-preview div.rhx-timeline");
        await Assertions.Expect(timeline).ToHaveAttributeAsync("role", "list");

        var items = page.Locator("#panel-basic-preview div.rhx-timeline-item");
        await Assertions.Expect(items).ToHaveCountAsync(4);
        await Assertions.Expect(items.First).ToHaveAttributeAsync("role", "listitem");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Active_item_has_aria_current_step(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var active = page.Locator("#panel-basic-preview div.rhx-timeline-item.rhx-timeline-item--active");
        await Assertions.Expect(active).ToHaveCountAsync(1);
        await Assertions.Expect(active).ToHaveAttributeAsync("aria-current", "step");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Items_have_label_and_body_regions(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var first = page.Locator("#panel-basic-preview div.rhx-timeline-item").First;
        await Assertions.Expect(first.Locator(".rhx-timeline-item__label")).ToHaveTextAsync("March 10, 2026");
        await Assertions.Expect(first.Locator(".rhx-timeline-item__body")).ToContainTextAsync("Order placed");
        // Connector (line + dot + line)
        await Assertions.Expect(first.Locator(".rhx-timeline-item__connector")).ToHaveCountAsync(1);
        await Assertions.Expect(first.Locator(".rhx-timeline-item__dot")).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Variants_panel_renders_modifier_classes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        foreach (var variant in new[] { "success", "brand", "warning", "danger" })
        {
            await Assertions.Expect(
                page.Locator($"#panel-variants-preview div.rhx-timeline-item.rhx-timeline-item--{variant}")
            ).ToHaveCountAsync(1);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Horizontal_timeline_applies_layout_modifier(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var timeline = page.Locator("#panel-horiz-preview div.rhx-timeline.rhx-timeline--horizontal");
        await Assertions.Expect(timeline).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-horiz-preview div.rhx-timeline-item")
        ).ToHaveCountAsync(3);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Alternate_timeline_applies_align_modifier(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var timeline = page.Locator("#panel-alt-preview div.rhx-timeline.rhx-timeline--alternate");
        await Assertions.Expect(timeline).ToHaveCountAsync(1);
    }
}
