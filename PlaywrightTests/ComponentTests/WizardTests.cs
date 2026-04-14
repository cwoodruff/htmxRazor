using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class WizardTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Wizard";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_wizard_renders_three_step_stepper(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wizard = page.Locator("#panel-basic-preview div.rhx-wizard");
        await Assertions.Expect(wizard).ToHaveCountAsync(1);
        await Assertions.Expect(wizard).ToHaveAttributeAsync("role", "group");

        var steps = wizard.Locator(".rhx-wizard__step");
        await Assertions.Expect(steps).ToHaveCountAsync(3);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Wizard_starts_on_first_step(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wizard = page.Locator("#panel-basic-preview div.rhx-wizard");
        var current = wizard.Locator(".rhx-wizard__step[aria-current='step']");
        await Assertions.Expect(current).ToHaveCountAsync(1);
        await Assertions.Expect(current).ToContainTextAsync("Account");

        var panel = wizard.Locator(".rhx-wizard__panel");
        await Assertions.Expect(panel).ToContainTextAsync("Create Account");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Next_button_advances_to_second_step(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var previewPanel = page.Locator("#panel-basic-preview");
        var nextBtn = previewPanel.Locator("button.rhx-wizard__next");
        await nextBtn.ClickAsync();

        var current = previewPanel.Locator(".rhx-wizard__step[aria-current='step']");
        await Assertions.Expect(current).ToContainTextAsync("Profile", new() { Timeout = 5000 });

        await Assertions.Expect(
            previewPanel.Locator(".rhx-wizard__panel")
        ).ToContainTextAsync("Your Profile");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Prev_button_appears_after_advancing(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var previewPanel = page.Locator("#panel-basic-preview");

        // No Prev button on step 1
        await Assertions.Expect(previewPanel.Locator("button.rhx-wizard__prev")).ToHaveCountAsync(0);

        await previewPanel.Locator("button.rhx-wizard__next").ClickAsync();

        // After going to step 2, Prev button should appear
        await Assertions.Expect(
            previewPanel.Locator("button.rhx-wizard__prev")
        ).ToHaveCountAsync(1, new() { Timeout = 5000 });
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Last_step_shows_Submit_label(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var previewPanel = page.Locator("#panel-basic-preview");

        // Advance twice: step 1 -> 2 -> 3
        await previewPanel.Locator("button.rhx-wizard__next").ClickAsync();
        await Assertions.Expect(
            previewPanel.Locator(".rhx-wizard__step[aria-current='step']")
        ).ToContainTextAsync("Profile", new() { Timeout = 5000 });

        await previewPanel.Locator("button.rhx-wizard__next").ClickAsync();
        await Assertions.Expect(
            previewPanel.Locator(".rhx-wizard__step[aria-current='step']")
        ).ToContainTextAsync("Confirm", new() { Timeout = 5000 });

        await Assertions.Expect(
            previewPanel.Locator("button.rhx-wizard__next")
        ).ToHaveTextAsync("Submit");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Vertical_layout_applies_modifier_class(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wizard = page.Locator("#panel-vert-preview div.rhx-wizard.rhx-wizard--vertical");
        await Assertions.Expect(wizard).ToHaveCountAsync(1);

        var current = wizard.Locator(".rhx-wizard__step--current");
        await Assertions.Expect(current).ToContainTextAsync("Details");
    }
}
