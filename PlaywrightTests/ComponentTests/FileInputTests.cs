using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

// rhx-file-input renders a native <input type="file"> (no JS). The browser supplies the file
// chooser and selected-file display.
public sealed class FileInputTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/FileInput";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_renders_native_file_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-basic-preview input.rhx-file-input__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("type", "file");
        await Assertions.Expect(input).ToHaveAttributeAsync("name", "file");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Images_only_input_has_accept_filter(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-images-preview input.rhx-file-input__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("accept", "image/*");
        await Assertions.Expect(
            page.Locator("#panel-images-preview .rhx-file-input__hint")).ToContainTextAsync("JPG");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Multiple_file_input_has_multiple_and_accept(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-multiple-preview input.rhx-file-input__control");
        await Assertions.Expect(input).ToHaveAttributeAsync("multiple", "");
        await Assertions.Expect(input).ToHaveAttributeAsync("accept", ".pdf,.doc,.docx");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sizes_render_three_variants(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(page.Locator("#panel-sizes-preview .rhx-file-input")).ToHaveCountAsync(3);
        await Assertions.Expect(page.Locator("#panel-sizes-preview .rhx-file-input--small")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#panel-sizes-preview .rhx-file-input--large")).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_file_input_is_disabled(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-states-preview input.rhx-file-input__control");
        await Assertions.Expect(input).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Selecting_a_file_sets_the_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var input = page.Locator("#panel-basic-preview input.rhx-file-input__control");
        await input.SetInputFilesAsync(new[]
        {
            new FilePayload
            {
                Name = "hello.txt",
                MimeType = "text/plain",
                Buffer = System.Text.Encoding.UTF8.GetBytes("hello world"),
            }
        });

        // The native input exposes the chosen file's name in its value (C:\fakepath\hello.txt).
        var value = await input.InputValueAsync();
        Assert.Contains("hello.txt", value);
    }
}
