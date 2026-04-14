using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class DialogTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Dialog";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_dialog_is_closed_by_default(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var dialog = page.Locator("#demo-dialog");
        await Assertions.Expect(dialog).ToHaveCountAsync(1);
        // Native <dialog> without [open] is not rendered
        await Assertions.Expect(dialog).Not.ToHaveAttributeAsync("open", "");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_open_trigger_opens_dialog(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var trigger = page.Locator("#panel-basic-preview [data-rhx-dialog-open='demo-dialog']");
        await trigger.ClickAsync();

        var dialog = page.Locator("#demo-dialog");
        await Assertions.Expect(dialog).ToHaveAttributeAsync("open", new System.Text.RegularExpressions.Regex(".*"));
        await Assertions.Expect(dialog).ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Close_button_closes_dialog(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview [data-rhx-dialog-open='demo-dialog']").ClickAsync();
        var dialog = page.Locator("#demo-dialog");
        await Assertions.Expect(dialog).ToBeVisibleAsync();

        await dialog.Locator(".rhx-dialog__close").ClickAsync();
        await Assertions.Expect(dialog).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Escape_key_closes_dialog(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview [data-rhx-dialog-open='demo-dialog']").ClickAsync();
        var dialog = page.Locator("#demo-dialog");
        await Assertions.Expect(dialog).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(dialog).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Dialog_uses_aria_labelled_title(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-basic-preview [data-rhx-dialog-open='demo-dialog']").ClickAsync();
        var dialog = page.Locator("#demo-dialog");
        await Assertions.Expect(dialog.Locator(".rhx-dialog__title")).ToContainTextAsync("Example Dialog");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Noheader_dialog_omits_header(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-noheader-preview [data-rhx-dialog-open='noheader-dialog']").ClickAsync();
        var dialog = page.Locator("#noheader-dialog");
        await Assertions.Expect(dialog).ToBeVisibleAsync();
        await Assertions.Expect(dialog.Locator(".rhx-dialog__header")).ToHaveCountAsync(0);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Size_variants_apply_modifier_classes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var small = page.Locator("#size-sm-dialog");
        var medium = page.Locator("#size-md-dialog");
        var large = page.Locator("#size-lg-dialog");
        var full = page.Locator("#size-full-dialog");

        await Assertions.Expect(small).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-dialog--small"));
        await Assertions.Expect(medium).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-dialog--medium"));
        await Assertions.Expect(large).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-dialog--large"));
        await Assertions.Expect(full).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-dialog--full"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Footer_dialog_shows_footer_slot(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await page.Locator("#panel-footer-preview [data-rhx-dialog-open='footer-dialog']").ClickAsync();
        var dialog = page.Locator("#footer-dialog");
        await Assertions.Expect(dialog).ToBeVisibleAsync();

        var footer = dialog.Locator(".rhx-dialog__footer");
        await Assertions.Expect(footer).ToBeVisibleAsync();
        await Assertions.Expect(footer.Locator("button", new() { HasTextString = "Save Changes" })).ToHaveCountAsync(1);
    }
}
