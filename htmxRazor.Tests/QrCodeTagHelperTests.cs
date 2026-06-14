using htmxRazor.Components.Utilities;
using Xunit;

namespace htmxRazor.Tests;

public class QrCodeTagHelperTests : TagHelperTestBase
{
    private QrCodeTagHelper CreateHelper() => new();

    private static string GetContent(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
        => output.Content.GetContent();

    private static QrCodeTagHelper Render(out Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output,
        Action<QrCodeTagHelper>? configure = null)
    {
        var helper = new QrCodeTagHelper();
        configure?.Invoke(helper);
        var context = CreateContext("rhx-qr-code");
        output = CreateOutput("rhx-qr-code");
        helper.ViewContext = CreateViewContext();
        helper.Process(context, output);
        return helper;
    }

    // ── Element rendering ──

    [Fact]
    public void Renders_Svg_Element()
    {
        Render(out var output);
        Assert.Equal("svg", output.TagName);
    }

    [Fact]
    public void Has_Block_Class()
    {
        Render(out var output);
        Assert.True(HasClass(output, "rhx-qr-code"));
    }

    [Fact]
    public void Has_Role_Img()
    {
        Render(out var output);
        AssertAttribute(output, "role", "img");
    }

    [Fact]
    public void Has_Svg_Namespace()
    {
        Render(out var output);
        AssertAttribute(output, "xmlns", "http://www.w3.org/2000/svg");
    }

    [Fact]
    public void No_Canvas_Data_Attributes()
    {
        Render(out var output, h => { h.Value = "https://example.com"; h.Fill = "#000"; h.Radius = 0.3; });
        AssertNoAttribute(output, "data-rhx-qr-code");
        AssertNoAttribute(output, "data-rhx-qr-value");
        AssertNoAttribute(output, "data-rhx-qr-size");
        AssertNoAttribute(output, "data-rhx-qr-fill");
        AssertNoAttribute(output, "data-rhx-qr-ec");
        AssertNoAttribute(output, "data-rhx-qr-radius");
    }

    // ── ViewBox / value ──

    [Fact]
    public void ViewBox_Matches_Module_Count()
    {
        Render(out var output, h => h.Value = "HELLO WORLD"); // 21x21, quiet zone 0
        AssertAttribute(output, "viewBox", "0 0 21 21");
    }

    [Fact]
    public void Renders_Module_Path_For_Value()
    {
        Render(out var output, h => h.Value = "https://example.com");
        var content = GetContent(output);
        Assert.Contains("<path", content);
        Assert.Contains("d=\"M", content);
    }

    [Fact]
    public void Empty_Value_Renders_Background_Only()
    {
        Render(out var output);
        var content = GetContent(output);
        Assert.Contains("<rect", content);
        Assert.DoesNotContain("<path", content);
    }

    // ── Size ──

    [Fact]
    public void Default_Size_128()
    {
        Render(out var output);
        AssertAttribute(output, "width", "128");
        AssertAttribute(output, "height", "128");
    }

    [Fact]
    public void Custom_Size()
    {
        Render(out var output, h => h.Size = 256);
        AssertAttribute(output, "width", "256");
        AssertAttribute(output, "height", "256");
    }

    // ── Colors ──

    [Fact]
    public void Default_Fill_And_Background_Are_Theme_Aware()
    {
        Render(out var output, h => h.Value = "HELLO WORLD");
        var content = GetContent(output);
        Assert.Contains("var(--rhx-color-surface)", content);
        Assert.Contains("var(--rhx-color-text)", content);
    }

    [Fact]
    public void Custom_Fill_And_Background()
    {
        Render(out var output, h => { h.Value = "HELLO WORLD"; h.Fill = "#1e40af"; h.Background = "#f0f9ff"; });
        var content = GetContent(output);
        Assert.Contains("fill=\"#f0f9ff\"", content); // background rect
        Assert.Contains("fill=\"#1e40af\"", content); // module path
    }

    // ── Label / Accessibility ──

    [Fact]
    public void Label_Sets_Aria_Label()
    {
        Render(out var output, h => h.Label = "Share link QR code");
        AssertAttribute(output, "aria-label", "Share link QR code");
    }

    [Fact]
    public void No_Label_Omits_Aria_Label()
    {
        Render(out var output);
        AssertNoAttribute(output, "aria-label");
    }

    // ── Radius ──

    [Fact]
    public void Default_Radius_Renders_Path()
    {
        Render(out var output, h => h.Value = "HELLO WORLD");
        var content = GetContent(output);
        Assert.Contains("<path", content);
    }

    [Fact]
    public void Custom_Radius_Renders_Rounded_Rects()
    {
        Render(out var output, h => { h.Value = "HELLO WORLD"; h.Radius = 0.5; });
        var content = GetContent(output);
        Assert.DoesNotContain("<path", content);
        Assert.Contains("rx=\"0.5\"", content);
    }

    // ── CSS class merging / Id ──

    [Fact]
    public void Custom_Css_Class_Is_Merged()
    {
        Render(out var output, h => h.CssClass = "my-qr");
        Assert.True(HasClass(output, "rhx-qr-code"));
        Assert.True(HasClass(output, "my-qr"));
    }

    [Fact]
    public void Id_Sets_Html_Attribute()
    {
        Render(out var output, h => h.Id = "share-qr");
        AssertAttribute(output, "id", "share-qr");
    }
}
