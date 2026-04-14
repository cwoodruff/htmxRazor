using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class TextareaTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Textarea";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_textarea_renders_label_and_native_textarea(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview div.rhx-textarea");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);

        await Assertions.Expect(
            wrapper.Locator("label.rhx-textarea__label")
        ).ToHaveTextAsync("Notes");

        var native = wrapper.Locator("textarea");
        await Assertions.Expect(native).ToHaveAttributeAsync("name", "notes");
        await Assertions.Expect(native).ToHaveAttributeAsync("placeholder", "Enter your notes...");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Typing_multiline_content_updates_value(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-basic-preview textarea[name='notes']");
        await native.FillAsync("Line 1\nLine 2\nLine 3");
        await Assertions.Expect(native).ToHaveValueAsync("Line 1\nLine 2\nLine 3");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Hint_example_sets_maxlength_and_hint_text(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-hint-preview textarea[name='bio']");
        await Assertions.Expect(native).ToHaveAttributeAsync("maxlength", "500");
        await Assertions.Expect(native).ToHaveAttributeAsync("rows", "5");

        var describedBy = await native.GetAttributeAsync("aria-describedby");
        Assert.False(string.IsNullOrEmpty(describedBy));
        var hint = page.Locator($"#panel-hint-preview #{describedBy}");
        await Assertions.Expect(hint).ToContainTextAsync("Max 500 characters");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Maxlength_prevents_typing_beyond_limit(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-hint-preview textarea[name='bio']");
        var longText = new string('x', 600);
        await native.FillAsync(longText);

        var value = await native.InputValueAsync();
        Assert.True(value.Length <= 500, $"Expected length <= 500 but was {value.Length}");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_textarea_is_not_interactive(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var disabled = page.Locator("#panel-states-preview textarea[name='ta-dis']");
        await Assertions.Expect(disabled).ToBeDisabledAsync();
        await Assertions.Expect(disabled).ToHaveValueAsync("Cannot edit");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Readonly_textarea_has_readonly_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var ro = page.Locator("#panel-states-preview textarea[name='ta-ro']");
        await Assertions.Expect(ro).ToHaveAttributeAsync("readonly", "");
        await Assertions.Expect(ro).ToHaveValueAsync("This content is read-only");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sizes_render_small_and_large_modifiers(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-sizes-preview div.rhx-textarea.rhx-textarea--small")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview div.rhx-textarea.rhx-textarea--large")
        ).ToHaveCountAsync(1);
    }
}
