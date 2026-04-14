using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class CardTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Card";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_card_renders_header_body_and_footer_slots(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var card = page.Locator("#panel-basic-preview div.rhx-card").First;
        await Assertions.Expect(card).ToHaveCountAsync(1);

        await Assertions.Expect(card.Locator("div.rhx-card__header")).ToHaveCountAsync(1);
        await Assertions.Expect(card.Locator("div.rhx-card__body")).ToHaveCountAsync(1);
        await Assertions.Expect(card.Locator("div.rhx-card__footer")).ToHaveCountAsync(1);

        await Assertions.Expect(card.Locator("div.rhx-card__header h3")).ToHaveTextAsync("Product Name");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_card_footer_contains_two_buttons(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var footerButtons = page.Locator("#panel-basic-preview div.rhx-card div.rhx-card__footer button.rhx-button");
        await Assertions.Expect(footerButtons).ToHaveCountAsync(2);
        await Assertions.Expect(footerButtons.First).ToHaveTextAsync("Buy Now");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Image_card_renders_image_slot_with_alt_text(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var imageSlot = page.Locator("#panel-image-preview div.rhx-card div.rhx-card__image");
        await Assertions.Expect(imageSlot).ToHaveCountAsync(1);

        var img = imageSlot.Locator("img");
        await Assertions.Expect(img).ToHaveCountAsync(1);
        await Assertions.Expect(img).ToHaveAttributeAsync("alt", "Random photo");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Minimal_card_has_body_but_no_header_or_footer(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var card = page.Locator("#panel-minimal-preview div.rhx-card").First;
        await Assertions.Expect(card).ToHaveCountAsync(1);
        await Assertions.Expect(card.Locator("div.rhx-card__body")).ToHaveCountAsync(1);
        await Assertions.Expect(card.Locator("div.rhx-card__header")).ToHaveCountAsync(0);
        await Assertions.Expect(card.Locator("div.rhx-card__footer")).ToHaveCountAsync(0);
        await Assertions.Expect(card.Locator("div.rhx-card__image")).ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Grid_example_renders_three_cards(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var cards = page.Locator("#panel-grid-preview div.rhx-card");
        await Assertions.Expect(cards).ToHaveCountAsync(3);
        await Assertions.Expect(
            page.Locator("#panel-grid-preview div.rhx-card div.rhx-card__header h3")
        ).ToHaveCountAsync(3);
    }
}
