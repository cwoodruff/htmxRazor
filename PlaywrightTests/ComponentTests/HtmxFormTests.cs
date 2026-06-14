using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class HtmxFormTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/HtmxForm";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_form_renders_as_form_element_with_htmx_post(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var form = page.Locator("#panel-basic-preview form.rhx-htmx-form");
        await Assertions.Expect(form).ToHaveCountAsync(1);
        await Assertions.Expect(form).ToHaveAttributeAsync("hx-post", new System.Text.RegularExpressions.Regex(".*HtmxForm.*Contact.*"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_form_configures_error_targets(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var form = page.Locator("#panel-basic-preview form.rhx-htmx-form");
        await Assertions.Expect(form).ToHaveAttributeAsync("hx-target-422", "#contact-errors");
        await Assertions.Expect(form).ToHaveAttributeAsync("hx-target-4*", "#contact-errors");
        await Assertions.Expect(form).ToHaveAttributeAsync("hx-target-5*", "#contact-errors");
        await Assertions.Expect(form).ToHaveAttributeAsync("hx-ext", new System.Text.RegularExpressions.Regex("response-targets"));
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_form_renders_input_fields(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var form = page.Locator("#panel-basic-preview form.rhx-htmx-form");
        await Assertions.Expect(form.Locator("input[name='Name']")).ToHaveCountAsync(1);
        await Assertions.Expect(form.Locator("input[name='Email']")).ToHaveCountAsync(1);
        await Assertions.Expect(form.Locator("button[type='submit']")).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Submitting_valid_form_swaps_success_callout(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var form = page.Locator("#panel-basic-preview form.rhx-htmx-form");
        await form.Locator("input[name='Name']").FillAsync("Ada Lovelace");
        await form.Locator("input[name='Email']").FillAsync("ada@example.com");
        await form.Locator("button[type='submit']").ClickAsync();

        // hx-target="this" + innerHTML swap replaces the form's content with the callout
        await Assertions.Expect(form).ToContainTextAsync("Thanks, Ada Lovelace", new() { Timeout = 5000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Submitting_empty_form_routes_error_to_target(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var form = page.Locator("#panel-basic-preview form.rhx-htmx-form");
        // Bypass native required-attr validation so the request reaches the server
        // and exercises the 422 + hx-target-422 routing path.
        await form.EvaluateAsync("f => { f.noValidate = true; }");
        await form.Locator("input[name='Email']").FillAsync("nobody@example.com");
        await form.Locator("button[type='submit']").ClickAsync();

        // Error goes to #contact-errors — form should NOT be replaced with success content
        var errors = page.Locator("#contact-errors");
        await Assertions.Expect(errors).Not.ToBeEmptyAsync(new() { Timeout = 5000 });
        await Assertions.Expect(form).Not.ToContainTextAsync("Thanks,");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Reset_form_resets_on_success_via_hx_on(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var form = page.Locator("#panel-reset-preview form.rhx-htmx-form");
        // Reset-on-success is now htmx's own hx-on (no custom JS).
        await Assertions.Expect(form).ToHaveAttributeAsync(
            "hx-on::after-request", "if(event.detail.successful)this.reset()");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Form_disables_submit_button_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var form = page.Locator("#panel-basic-preview form.rhx-htmx-form");
        await Assertions.Expect(form).ToHaveAttributeAsync("hx-disabled-elt", "find button[type='submit']");
    }
}
