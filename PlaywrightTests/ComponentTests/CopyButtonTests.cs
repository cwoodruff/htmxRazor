using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class CopyButtonTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/CopyButton";

    // Override navigator.clipboard.writeText so tests behave identically across
    // Chromium / Firefox / WebKit even when clipboard permissions are unset.
    private const string StubClipboardScript = @"
        Object.defineProperty(navigator, 'clipboard', {
            configurable: true,
            value: {
                writeText: (text) => {
                    window.__rhxLastCopiedText = text;
                    return Promise.resolve();
                }
            }
        });
    ";

    private async Task<IPage> OpenWithClipboardStubAsync(string browserName)
    {
        var page = await OpenAsync(browserName, Path);
        await page.EvaluateAsync(StubClipboardScript);
        return page;
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Static_value_button_renders_with_copy_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var btn = page.Locator("#panel-static-preview button.rhx-copy-button").First;
        await Assertions.Expect(btn).ToHaveAttributeAsync("type", "button");
        await Assertions.Expect(btn).ToHaveAttributeAsync("data-rhx-copy-value", "dotnet add package htmxRazor");
        await Assertions.Expect(btn).ToHaveAttributeAsync("aria-label", "Copy");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Clicking_static_button_invokes_clipboard_and_toggles_success(string browserName)
    {
        var page = await OpenWithClipboardStubAsync(browserName);

        var btn = page.Locator("#panel-static-preview button.rhx-copy-button").First;
        await btn.ClickAsync();

        var copied = await page.EvaluateAsync<string>("() => window.__rhxLastCopiedText");
        Assert.Equal("dotnet add package htmxRazor", copied);

        await Assertions.Expect(btn).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("rhx-copy-button--success"));
        await Assertions.Expect(btn).ToHaveAttributeAsync("aria-label", "Copied!");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task From_element_button_copies_referenced_text(string browserName)
    {
        var page = await OpenWithClipboardStubAsync(browserName);

        var btn = page.Locator("#panel-from-preview button.rhx-copy-button").First;
        await Assertions.Expect(btn).ToHaveAttributeAsync("data-rhx-copy-from", "#install-cmd code");

        await btn.ClickAsync();
        var copied = await page.EvaluateAsync<string>("() => window.__rhxLastCopiedText");
        Assert.Equal("npm install htmxRazor-client", copied);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_labels_are_applied_to_aria_and_feedback(string browserName)
    {
        var page = await OpenWithClipboardStubAsync(browserName);

        var btn = page.Locator("#panel-labels-preview button.rhx-copy-button").First;
        await Assertions.Expect(btn).ToHaveAttributeAsync("aria-label", "Copy URL");
        await Assertions.Expect(btn).ToHaveAttributeAsync("data-rhx-copy-success-label", "URL Copied!");

        await btn.ClickAsync();
        await Assertions.Expect(btn).ToHaveAttributeAsync("aria-label", "URL Copied!");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Long_feedback_variant_sets_custom_duration(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var btn = page.Locator("#panel-feedback-preview button.rhx-copy-button").First;
        await Assertions.Expect(btn).ToHaveAttributeAsync("data-rhx-copy-duration", "5000");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_button_is_not_clickable(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var btn = page.Locator("#panel-disabled-preview button.rhx-copy-button").First;
        await Assertions.Expect(btn).ToBeDisabledAsync();
        await Assertions.Expect(btn).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Rhx_copy_event_fires_on_successful_copy(string browserName)
    {
        var page = await OpenWithClipboardStubAsync(browserName);

        var btn = page.Locator("#panel-static-preview button.rhx-copy-button").First;
        await btn.EvaluateAsync(
            "el => { window.__rhxCopyEventText = null; el.addEventListener('rhx:copy', e => { window.__rhxCopyEventText = e.detail.text; }); }");

        await btn.ClickAsync();
        var text = await page.EvaluateAsync<string>("() => window.__rhxCopyEventText");
        Assert.Equal("dotnet add package htmxRazor", text);
    }
}
