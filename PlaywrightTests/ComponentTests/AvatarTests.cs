using Microsoft.Playwright;
using PlaywrightTests.Infrastructure;

namespace PlaywrightTests.ComponentTests;

public sealed class AvatarTests(DemoAppFactory app) : ComponentTestBase(app)
{
    private const string Path = "/Docs/Components/Avatar";

    [Theory, MemberData(nameof(Browsers))]
    public async Task Image_avatars_render_three_with_img_elements(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var avatars = page.Locator("#panel-images-preview div.rhx-avatar");
        await Assertions.Expect(avatars).ToHaveCountAsync(3);

        var images = page.Locator("#panel-images-preview div.rhx-avatar img.rhx-avatar__image");
        await Assertions.Expect(images).ToHaveCountAsync(3);

        await Assertions.Expect(avatars.First).ToHaveAttributeAsync("aria-label", "Alice");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Initials_avatars_render_six_entries(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var avatars = page.Locator("#panel-initials-preview div.rhx-avatar");
        await Assertions.Expect(avatars).ToHaveCountAsync(6);

        var initials = page.Locator("#panel-initials-preview div.rhx-avatar span.rhx-avatar__initials");
        await Assertions.Expect(initials).ToHaveCountAsync(6);
        await Assertions.Expect(initials.First).ToHaveTextAsync("JD");
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Shapes_render_circle_rounded_and_square_variants(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        foreach (var shape in new[] { "circle", "rounded", "square" })
        {
            await Assertions.Expect(
                page.Locator($"#panel-shapes-preview div.rhx-avatar.rhx-avatar--{shape}")
            ).ToHaveCountAsync(1);
        }
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Small_and_large_images_use_size_modifier_classes(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        await Assertions.Expect(
            page.Locator("#panel-images-preview div.rhx-avatar.rhx-avatar--small")
        ).ToHaveCountAsync(1);
        await Assertions.Expect(
            page.Locator("#panel-images-preview div.rhx-avatar.rhx-avatar--large")
        ).ToHaveCountAsync(1);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task Custom_size_avatar_uses_css_variable_style(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var avatar = page.Locator("#panel-custom-size-preview div.rhx-avatar").First;
        await Assertions.Expect(avatar).ToHaveCountAsync(1);
        var style = await avatar.GetAttributeAsync("style");
        Assert.NotNull(style);
        Assert.Contains("--rhx-avatar-size", style);
        Assert.Contains("5rem", style);
    }

    [Theory, MemberData(nameof(Browsers))]
    public async Task All_avatars_expose_role_img(string browserName)
    {
        var page = await OpenAsync(browserName, Path);

        var avatar = page.Locator("#panel-initials-preview div.rhx-avatar").First;
        await Assertions.Expect(avatar).ToHaveAttributeAsync("role", "img");
    }
}
