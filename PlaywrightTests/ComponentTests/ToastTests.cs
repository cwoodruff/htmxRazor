using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class ToastTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Toast";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Toast_container_exists_in_layout(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#rhx-toasts[data-rhx-toast-container]");
        await Assertions.Expect(container).ToHaveCountAsync(1);
        await Assertions.Expect(container).ToHaveAttributeAsync("role", "status");
        await Assertions.Expect(container).ToHaveAttributeAsync("aria-live", "polite");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Static_variant_examples_render_all_five(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        foreach (var variant in new[] { "neutral", "brand", "success", "warning", "danger" })
        {
            await Assertions.Expect(
                page.Locator($"#panel-variants-preview div.rhx-toast.rhx-toast--{variant}")
            ).ToHaveCountAsync(1);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_success_trigger_creates_toast_in_container(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#rhx-toasts");
        await Assertions.Expect(container.Locator("[data-rhx-toast]")).ToHaveCountAsync(0);

        var successBtn = page.Locator(
            "#panel-trigger-preview button.rhx-button", new() { HasTextString = "Success Toast" });
        await successBtn.ClickAsync();

        var successToast = container.Locator("div.rhx-toast.rhx-toast--success");
        await Assertions.Expect(successToast).ToHaveCountAsync(1, new() { Timeout = 5000 });
        await Assertions.Expect(successToast).ToContainTextAsync("Item saved successfully!");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Error_trigger_creates_danger_toast(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#rhx-toasts");
        var errorBtn = page.Locator(
            "#panel-trigger-preview button.rhx-button", new() { HasTextString = "Error Toast" });
        await errorBtn.ClickAsync();

        var dangerToast = container.Locator("div.rhx-toast.rhx-toast--danger");
        await Assertions.Expect(dangerToast).ToHaveCountAsync(1, new() { Timeout = 5000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Toast_close_button_dismisses_it(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#rhx-toasts");

        await page.Locator(
            "#panel-trigger-preview button.rhx-button", new() { HasTextString = "Brand Toast" }).ClickAsync();

        var toast = container.Locator("div.rhx-toast.rhx-toast--brand");
        await Assertions.Expect(toast).ToHaveCountAsync(1, new() { Timeout = 5000 });

        await toast.Locator("button.rhx-toast__close").ClickAsync();

        // The close animation removes the node; wait for it to go away.
        await Assertions.Expect(
            container.Locator("div.rhx-toast.rhx-toast--brand")
        ).ToHaveCountAsync(0, new() { Timeout = 5000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Multiple_triggers_stack_multiple_toasts(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#rhx-toasts");

        await page.Locator(
            "#panel-trigger-preview button.rhx-button", new() { HasTextString = "Success Toast" }).ClickAsync();
        await page.Locator(
            "#panel-trigger-preview button.rhx-button", new() { HasTextString = "Warning Toast" }).ClickAsync();

        await Assertions.Expect(
            container.Locator("[data-rhx-toast]")
        ).ToHaveCountAsync(2, new() { Timeout = 5000 });
    }
}
