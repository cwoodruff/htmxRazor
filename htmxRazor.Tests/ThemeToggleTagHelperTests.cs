using htmxRazor.Components.Theming;
using Microsoft.AspNetCore.Mvc.Rendering;
using Xunit;

namespace htmxRazor.Tests;

// rhx-theme-toggle is a zero-JS dark-mode switch: a sr-only checkbox (.rhx-theme-controller)
// whose checked state drives CSS html:has(...) and is persisted to a cookie via htmx.
public class ThemeToggleTagHelperTests : TagHelperTestBase
{
    private ThemeToggleTagHelper CreateHelper(ViewContext vc) =>
        new(CreateUrlHelperFactory()) { ViewContext = vc };

    private string Render(ViewContext vc)
    {
        var helper = CreateHelper(vc);
        var context = CreateContext("rhx-theme-toggle");
        var output = CreateOutput("rhx-theme-toggle");
        helper.Process(context, output);
        return output.Content.GetContent();
    }

    [Fact]
    public void Renders_Label_With_Controller_Checkbox_And_Htmx_Persist()
    {
        var vc = CreateViewContext();
        var helper = CreateHelper(vc);
        var context = CreateContext("rhx-theme-toggle");
        var output = CreateOutput("rhx-theme-toggle");

        helper.Process(context, output);

        Assert.Equal("label", output.TagName);
        Assert.True(HasClass(output, "rhx-theme-toggle"));
        var html = output.Content.GetContent();
        Assert.Contains("type=\"checkbox\"", html);
        Assert.Contains("rhx-theme-controller", html);
        Assert.Contains("rhx-sr-only", html);
        // Persistence is a zero-JS htmx POST to the library cookie endpoint.
        Assert.Contains("hx-post=\"/_rhx/theme\"", html);
        Assert.Contains("hx-trigger=\"change\"", html);
        Assert.Contains("rhx-theme-toggle__track", html);
    }

    [Fact]
    public void Unchecked_When_No_Cookie()
    {
        var html = Render(CreateViewContext());
        Assert.DoesNotContain(" checked", html);
    }

    [Fact]
    public void Checked_When_Theme_Cookie_Is_Dark()
    {
        var vc = CreateViewContext();
        vc.HttpContext.Request.Headers["Cookie"] = "rhx-theme=dark";

        Assert.Contains(" checked", Render(vc));
    }

    [Fact]
    public void Unchecked_When_Theme_Cookie_Is_Light()
    {
        var vc = CreateViewContext();
        vc.HttpContext.Request.Headers["Cookie"] = "rhx-theme=light";

        Assert.DoesNotContain(" checked", Render(vc));
    }
}
