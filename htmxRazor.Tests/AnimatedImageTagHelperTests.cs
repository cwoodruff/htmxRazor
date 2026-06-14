using Xunit;
using htmxRazor.Components.Imagery;

namespace htmxRazor.Tests;

public class AnimatedImageTagHelperTests : TagHelperTestBase
{
    private AnimatedImageTagHelper CreateHelper()
    {
        var helper = new AnimatedImageTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    // ══════════════════════════════════════════════
    //  Structure
    // ══════════════════════════════════════════════

    [Fact]
    public void Renders_Div_Element()
    {
        var helper = CreateHelper();
        helper.Src = "animation.gif";
        var context = CreateContext("rhx-animated-image");
        var output = CreateOutput("rhx-animated-image");

        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Has_Block_Class()
    {
        var helper = CreateHelper();
        helper.Src = "animation.gif";
        var context = CreateContext("rhx-animated-image");
        var output = CreateOutput("rhx-animated-image");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-animated-image"));
    }

    // ══════════════════════════════════════════════
    //  No poster — plays continuously, just the image
    // ══════════════════════════════════════════════

    [Fact]
    public void Without_Poster_Renders_Single_Img()
    {
        var helper = CreateHelper();
        helper.Src = "animation.gif";
        helper.Alt = "Spinner";
        var context = CreateContext("rhx-animated-image");
        var output = CreateOutput("rhx-animated-image");

        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-animated-image__img", content);
        Assert.Contains("src=\"animation.gif\"", content);
        Assert.Contains("alt=\"Spinner\"", content);
    }

    [Fact]
    public void Without_Poster_Is_Not_Hoverable()
    {
        var helper = CreateHelper();
        helper.Src = "animation.gif";
        var context = CreateContext("rhx-animated-image");
        var output = CreateOutput("rhx-animated-image");

        helper.Process(context, output);

        Assert.False(HasClass(output, "rhx-animated-image--hoverable"));
        AssertNoAttribute(output, "tabindex");
        Assert.DoesNotContain("rhx-animated-image__poster", output.Content.GetContent());
    }

    // ══════════════════════════════════════════════
    //  Poster — hover/focus to play (CSS only)
    // ══════════════════════════════════════════════

    [Fact]
    public void With_Poster_Renders_Poster_And_Animation()
    {
        var helper = CreateHelper();
        helper.Src = "animation.gif";
        helper.Poster = "first-frame.png";
        var context = CreateContext("rhx-animated-image");
        var output = CreateOutput("rhx-animated-image");

        helper.Process(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-animated-image__poster", content);
        Assert.Contains("src=\"first-frame.png\"", content);
        Assert.Contains("rhx-animated-image__img", content);
        Assert.Contains("src=\"animation.gif\"", content);
        Assert.Contains("rhx-animated-image__badge", content);
    }

    [Fact]
    public void With_Poster_Is_Hoverable_And_Focusable()
    {
        var helper = CreateHelper();
        helper.Src = "animation.gif";
        helper.Poster = "first-frame.png";
        helper.Alt = "Demo";
        var context = CreateContext("rhx-animated-image");
        var output = CreateOutput("rhx-animated-image");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-animated-image--hoverable"));
        AssertAttribute(output, "tabindex", "0");
        AssertAttribute(output, "role", "img");
        AssertAttribute(output, "aria-label", "Demo");
    }

    // ══════════════════════════════════════════════
    //  htmx
    // ══════════════════════════════════════════════

    [Fact]
    public void Renders_Htmx_Attributes()
    {
        var helper = CreateHelper();
        helper.Src = "animation.gif";
        helper.HxGet = "/api/frame";
        var context = CreateContext("rhx-animated-image");
        var output = CreateOutput("rhx-animated-image");

        helper.Process(context, output);

        AssertAttribute(output, "hx-get", "/api/frame");
    }
}
