using htmxRazor.Components.Utilities;
using Xunit;

namespace htmxRazor.Tests;

public class PopupTagHelperTests : TagHelperTestBase
{
    private PopupTagHelper CreateHelper()
    {
        return new PopupTagHelper { ViewContext = CreateViewContext() };
    }

    // ══════════════════════════════════════════════
    //  Structure
    // ══════════════════════════════════════════════

    [Fact]
    public void Renders_Div_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Has_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-popup"));
    }

    [Fact]
    public void No_Data_Hooks()
    {
        var helper = CreateHelper();
        helper.Anchor = "#trigger";
        helper.Arrow = true;
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        AssertNoAttribute(output, "data-rhx-popup");
        AssertNoAttribute(output, "data-rhx-anchor");
        AssertNoAttribute(output, "data-rhx-placement");
        AssertNoAttribute(output, "data-rhx-distance");
        AssertNoAttribute(output, "data-rhx-skidding");
        AssertNoAttribute(output, "data-rhx-strategy");
        AssertNoAttribute(output, "data-rhx-no-flip");
        AssertNoAttribute(output, "data-rhx-no-shift");
        AssertNoAttribute(output, "data-rhx-arrow");
        AssertNoAttribute(output, "data-rhx-arrow-padding");
    }

    [Fact]
    public void Custom_Class_Merged()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-popup";
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-popup"));
        Assert.True(HasClass(output, "my-popup"));
    }

    // ══════════════════════════════════════════════
    //  Default state (inactive)
    // ══════════════════════════════════════════════

    [Fact]
    public void Default_No_Active_Modifier()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        Assert.False(HasClass(output, "rhx-popup--active"));
    }

    [Fact]
    public void Default_No_Hidden_Attributes()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        // Visibility is driven entirely by the --active class / CSS now.
        AssertNoAttribute(output, "hidden");
        AssertNoAttribute(output, "aria-hidden");
    }

    // ══════════════════════════════════════════════
    //  Active state
    // ══════════════════════════════════════════════

    [Fact]
    public void Active_Has_Modifier()
    {
        var helper = CreateHelper();
        helper.Active = true;
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-popup--active"));
    }

    // ══════════════════════════════════════════════
    //  Anchor positioning style
    // ══════════════════════════════════════════════

    [Fact]
    public void Emits_Position_Anchor_Style_From_Id()
    {
        var helper = CreateHelper();
        helper.Id = "my-popup";
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.NotNull(style);
        Assert.Contains("position-anchor:--my-popup", style);
    }

    [Fact]
    public void Emits_Position_Try_Fallbacks()
    {
        var helper = CreateHelper();
        helper.Id = "my-popup";
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.Contains("position-try-fallbacks:flip-block,flip-inline", style);
    }

    [Fact]
    public void Generates_Id_When_Absent()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        var id = output.Attributes["id"]?.Value?.ToString();
        Assert.False(string.IsNullOrWhiteSpace(id));

        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.Contains($"position-anchor:--{id}", style);
    }

    // ══════════════════════════════════════════════
    //  Placement → position-area
    // ══════════════════════════════════════════════

    [Fact]
    public void Default_Placement_Maps_To_Bottom_Span_Right()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.Contains("position-area:bottom span-right", style);
    }

    [Theory]
    [InlineData("top", "top")]
    [InlineData("top-start", "top span-right")]
    [InlineData("top-end", "top span-left")]
    [InlineData("bottom", "bottom")]
    [InlineData("bottom-start", "bottom span-right")]
    [InlineData("bottom-end", "bottom span-left")]
    [InlineData("left", "left")]
    [InlineData("left-start", "left span-bottom")]
    [InlineData("left-end", "left span-top")]
    [InlineData("right", "right")]
    [InlineData("right-start", "right span-bottom")]
    [InlineData("right-end", "right span-top")]
    public void Placement_Maps_To_Position_Area(string placement, string expectedArea)
    {
        var helper = CreateHelper();
        helper.Placement = placement;
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        var style = output.Attributes["style"]?.Value?.ToString();
        Assert.Contains($"position-area:{expectedArea}", style);
    }

    // ══════════════════════════════════════════════
    //  Arrow
    // ══════════════════════════════════════════════

    [Fact]
    public void Default_No_Arrow_Element()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        Assert.DoesNotContain("rhx-popup__arrow", output.PostContent.GetContent());
    }

    [Fact]
    public void Arrow_Adds_Element()
    {
        var helper = CreateHelper();
        helper.Arrow = true;
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        Assert.Contains("rhx-popup__arrow", output.PostContent.GetContent());
    }

    // ══════════════════════════════════════════════
    //  htmx
    // ══════════════════════════════════════════════

    [Fact]
    public void Htmx_Attributes_Rendered()
    {
        var helper = CreateHelper();
        helper.HxGet = "/api/data";
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        AssertAttribute(output, "hx-get", "/api/data");
    }

    // ══════════════════════════════════════════════
    //  Id
    // ══════════════════════════════════════════════

    [Fact]
    public void Custom_Id()
    {
        var helper = CreateHelper();
        helper.Id = "my-popup";
        var context = CreateContext("rhx-popup");
        var output = CreateOutput("rhx-popup");

        helper.Process(context, output);

        AssertAttribute(output, "id", "my-popup");
    }
}
