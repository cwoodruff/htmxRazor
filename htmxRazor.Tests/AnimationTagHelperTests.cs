using htmxRazor.Components.Utilities;
using Xunit;

namespace htmxRazor.Tests;

public class AnimationTagHelperTests : TagHelperTestBase
{
    private AnimationTagHelper CreateHelper()
    {
        return new AnimationTagHelper { ViewContext = CreateViewContext() };
    }

    private static string Style(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
        => output.Attributes.TryGetAttribute("style", out var a) ? a.Value?.ToString() ?? "" : "";

    // ══════════════════════════════════════════════
    //  Structure
    // ══════════════════════════════════════════════

    [Fact]
    public void Renders_Div_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-animation"));
    }

    [Fact]
    public void Custom_Class_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-anim";
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-animation"));
        Assert.True(HasClass(output, "my-anim"));
    }

    // ══════════════════════════════════════════════
    //  Server-emitted CSS animation (no JS)
    // ══════════════════════════════════════════════

    [Fact]
    public void Emits_Animation_Style()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.Contains("animation:", Style(output));
    }

    [Fact]
    public void Default_Style_Uses_Defaults()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.Equal("animation: rhx-fadeIn 300ms ease 0ms 1 normal both", Style(output));
    }

    [Fact]
    public void Custom_Animation_Name()
    {
        var helper = CreateHelper();
        helper.Name = "slideInLeft";
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.Contains("animation: rhx-slideInLeft ", Style(output));
    }

    [Fact]
    public void Custom_Duration()
    {
        var helper = CreateHelper();
        helper.Duration = 500;
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.Contains("rhx-fadeIn 500ms ", Style(output));
    }

    [Fact]
    public void Custom_Delay()
    {
        var helper = CreateHelper();
        helper.Delay = 200;
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.Contains("ease 200ms ", Style(output));
    }

    [Fact]
    public void Custom_Direction()
    {
        var helper = CreateHelper();
        helper.Direction = "reverse";
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.Contains(" reverse ", Style(output));
    }

    [Fact]
    public void Custom_Easing()
    {
        var helper = CreateHelper();
        helper.Easing = "ease-in-out";
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.Contains(" ease-in-out ", Style(output));
    }

    [Fact]
    public void Custom_Iterations()
    {
        var helper = CreateHelper();
        helper.Iterations = "infinite";
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.Contains(" infinite ", Style(output));
    }

    [Fact]
    public void Custom_Fill()
    {
        var helper = CreateHelper();
        helper.Fill = "forwards";
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.EndsWith(" forwards", Style(output));
    }

    [Fact]
    public void Default_Play_Adds_Playing_Modifier()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-animation--playing"));
    }

    [Fact]
    public void Paused_Sets_Play_State_Paused()
    {
        var helper = CreateHelper();
        helper.Play = false;
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.Contains("animation-play-state: paused", Style(output));
    }

    [Fact]
    public void Paused_No_Playing_Modifier()
    {
        var helper = CreateHelper();
        helper.Play = false;
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.False(HasClass(output, "rhx-animation--playing"));
    }

    // ══════════════════════════════════════════════
    //  htmx
    // ══════════════════════════════════════════════

    [Fact]
    public void Htmx_Attributes_Rendered()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/content";
        helper.HxTarget = "#result";
        helper.HxSwap = "innerHTML";
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        AssertAttribute(output, "hx-get", "/api/content");
        AssertAttribute(output, "hx-target", "#result");
        AssertAttribute(output, "hx-swap", "innerHTML");
    }

    // ══════════════════════════════════════════════
    //  Id and hidden
    // ══════════════════════════════════════════════

    [Fact]
    public void Custom_Id()
    {
        var helper = CreateHelper();
        helper.Id = "my-animation";
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        AssertAttribute(output, "id", "my-animation");
    }

    [Fact]
    public void Hidden_Attribute()
    {
        var helper = CreateHelper();
        helper.Hidden = true;
        var context = CreateContext("rhx-animation");
        var output = CreateOutput("rhx-animation");

        helper.Process(context, output);

        Assert.True(output.Attributes.TryGetAttribute("hidden", out _));
    }
}
