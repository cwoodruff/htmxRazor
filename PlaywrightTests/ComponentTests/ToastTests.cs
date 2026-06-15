using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// Toasts are JS-free: server pushes a rendered toast via an htmx out-of-band swap into the
// container; CSS handles auto-dismiss (delayed animation) and close (:has(:checked) on a checkbox).
public sealed class ToastTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Toast";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Toast_container_exists_in_layout(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#rhx-toasts");
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
    public async Task Close_control_hides_the_toast_via_css(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        // Static variant toasts have rhx-duration="0" (no auto-dismiss) — deterministic.
        var toast = page.Locator("#panel-variants-preview div.rhx-toast.rhx-toast--neutral");
        await Assertions.Expect(toast).ToBeVisibleAsync();

        // Clicking the close <label> checks the sr-only checkbox → CSS :has(:checked) hides it.
        await toast.Locator("label.rhx-toast__close").ClickAsync();
        await Assertions.Expect(toast).Not.ToBeVisibleAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Trigger_button_pushes_toast_via_oob_swap(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#rhx-toasts");
        await page.Locator(
            "#panel-trigger-preview button.rhx-button", new() { HasTextString = "Success Toast" }).ClickAsync();

        var successToast = container.Locator("div.rhx-toast.rhx-toast--success");
        await Assertions.Expect(successToast).ToHaveCountAsync(1, new() { Timeout = 5000 });
        await Assertions.Expect(successToast).ToContainTextAsync("Item saved successfully!");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Error_trigger_pushes_danger_toast(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var container = page.Locator("#rhx-toasts");
        await page.Locator(
            "#panel-trigger-preview button.rhx-button", new() { HasTextString = "Error Toast" }).ClickAsync();

        await Assertions.Expect(
            container.Locator("div.rhx-toast.rhx-toast--danger")
        ).ToHaveCountAsync(1, new() { Timeout = 5000 });
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
            container.Locator("div.rhx-toast")
        ).ToHaveCountAsync(2, new() { Timeout = 5000 });
    }
}
