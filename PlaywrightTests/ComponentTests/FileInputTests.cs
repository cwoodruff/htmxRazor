using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class FileInputTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/FileInput";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Basic_file_input_renders_dropzone_and_native_input(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview [data-rhx-file-input]");
        await Assertions.Expect(wrapper).ToHaveCountAsync(1);
        await Assertions.Expect(wrapper.Locator("label.rhx-file-input__dropzone")).ToBeVisibleAsync();

        var native = wrapper.Locator("input[type='file']");
        await Assertions.Expect(native).ToHaveAttributeAsync("name", "file");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Images_only_input_has_accept_filter(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-images-preview [data-rhx-file-input] input[type='file']");
        await Assertions.Expect(native).ToHaveAttributeAsync("accept", "image/*");
        await Assertions.Expect(
            page.Locator("#panel-images-preview [data-rhx-file-input] .rhx-file-input__hint")
        ).ToContainTextAsync("JPG");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Multiple_file_input_has_multiple_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-multiple-preview [data-rhx-file-input] input[type='file']");
        await Assertions.Expect(native).ToHaveAttributeAsync("multiple", "");
        await Assertions.Expect(native).ToHaveAttributeAsync("accept", ".pdf,.doc,.docx");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Max_size_input_exposes_data_attribute(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-maxsize-preview [data-rhx-file-input]");
        await Assertions.Expect(wrapper).ToHaveAttributeAsync("data-rhx-max-size", "5242880");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Sizes_render_three_variants(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var inputs = page.Locator("#panel-sizes-preview [data-rhx-file-input]");
        await Assertions.Expect(inputs).ToHaveCountAsync(3);

        await Assertions.Expect(
            page.Locator("#panel-sizes-preview .rhx-file-input--small")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-sizes-preview .rhx-file-input--large")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Disabled_file_input_has_disabled_native(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var native = page.Locator("#panel-states-preview [data-rhx-file-input] input[type='file']");
        await Assertions.Expect(native).ToBeDisabledAsync();
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Selecting_file_triggers_file_list_update(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var wrapper = page.Locator("#panel-basic-preview [data-rhx-file-input]");
        var native = wrapper.Locator("input[type='file']");

        await native.SetInputFilesAsync(new[]
        {
            new FilePayload
            {
                Name = "hello.txt",
                MimeType = "text/plain",
                Buffer = System.Text.Encoding.UTF8.GetBytes("hello world"),
            }
        });

        // File list container is aria-live and should update with file name
        var list = wrapper.Locator(".rhx-file-input__file-list");
        await Assertions.Expect(list).ToContainTextAsync("hello.txt", new() { Timeout = 5000 });
    }
}
