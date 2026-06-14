using htmxRazor.Components.Feedback;
using Xunit;

namespace htmxRazor.Tests;

public class TooltipTagHelperTests : TagHelperTestBase
{
    private TooltipTagHelper CreateHelper() => new();

    private static async Task<string> RenderAsync(TooltipTagHelper helper, string child = "<button>Save</button>")
    {
        var ctx = CreateContext("rhx-tooltip");
        var output = CreateOutput("rhx-tooltip", childContent: child);
        await helper.ProcessAsync(ctx, output);
        return output.Content.GetContent();
    }

    [Fact]
    public async Task Renders_Span_Element()
    {
        var helper = CreateHelper();
        helper.Content = "Save your changes";
        var output = CreateOutput("rhx-tooltip", childContent: "<button>Save</button>");
        await helper.ProcessAsync(CreateContext("rhx-tooltip"), output);
        Assert.Equal("span", output.TagName);
    }

    [Fact]
    public async Task Has_Wrapper_Class_And_No_Js_Hooks()
    {
        var helper = CreateHelper();
        helper.Content = "Save your changes";
        var output = CreateOutput("rhx-tooltip", childContent: "<button>Save</button>");
        await helper.ProcessAsync(CreateContext("rhx-tooltip"), output);

        Assert.True(HasClass(output, "rhx-tooltip-wrap"));
        AssertNoAttribute(output, "data-rhx-tooltip");
        AssertNoAttribute(output, "data-rhx-tooltip-trigger");
    }

    [Fact]
    public async Task Renders_Bubble_With_Content_And_Role()
    {
        var helper = CreateHelper();
        helper.Content = "Save your changes";
        var content = await RenderAsync(helper);

        Assert.Contains("class=\"rhx-tooltip\"", content);
        Assert.Contains("role=\"tooltip\"", content);
        Assert.Contains("Save your changes", content);
    }

    [Fact]
    public async Task Default_Placement_Is_Top()
    {
        var helper = CreateHelper();
        helper.Content = "x";
        var output = CreateOutput("rhx-tooltip", childContent: "<button>Save</button>");
        await helper.ProcessAsync(CreateContext("rhx-tooltip"), output);
        Assert.True(HasClass(output, "rhx-tooltip-wrap--top"));
    }

    [Theory]
    [InlineData("top")]
    [InlineData("bottom")]
    [InlineData("left")]
    [InlineData("right")]
    public async Task Custom_Placement_Modifier(string placement)
    {
        var helper = CreateHelper();
        helper.Content = "x";
        helper.Placement = placement;
        var output = CreateOutput("rhx-tooltip", childContent: "<button>Save</button>");
        await helper.ProcessAsync(CreateContext("rhx-tooltip"), output);
        Assert.True(HasClass(output, $"rhx-tooltip-wrap--{placement}"));
    }

    [Fact]
    public async Task Placement_Is_Case_Insensitive()
    {
        var helper = CreateHelper();
        helper.Content = "x";
        helper.Placement = "Bottom";
        var output = CreateOutput("rhx-tooltip", childContent: "<button>Save</button>");
        await helper.ProcessAsync(CreateContext("rhx-tooltip"), output);
        Assert.True(HasClass(output, "rhx-tooltip-wrap--bottom"));
    }

    [Fact]
    public async Task Disabled_Hides_Bubble()
    {
        var helper = CreateHelper();
        helper.Content = "x";
        helper.Disabled = true;
        var output = CreateOutput("rhx-tooltip", childContent: "<button>Save</button>");
        await helper.ProcessAsync(CreateContext("rhx-tooltip"), output);

        Assert.True(HasClass(output, "rhx-tooltip-wrap--disabled"));
        Assert.DoesNotContain("class=\"rhx-tooltip\"", output.Content.GetContent());
    }

    [Fact]
    public async Task Preserves_Child_Content()
    {
        var helper = CreateHelper();
        helper.Content = "x";
        var content = await RenderAsync(helper, "<button>Save</button>");
        Assert.Contains("Save", content);
    }

    [Fact]
    public async Task Content_Is_Html_Encoded()
    {
        var helper = CreateHelper();
        helper.Content = "<b>bold</b>";
        var content = await RenderAsync(helper);
        Assert.DoesNotContain("<b>bold</b>", content);
        Assert.Contains("&lt;b&gt;", content);
    }

    [Fact]
    public async Task Uses_StartTagAndEndTag_Mode()
    {
        var helper = CreateHelper();
        helper.Content = "x";
        var output = CreateOutput("rhx-tooltip", childContent: "<button>Save</button>");
        await helper.ProcessAsync(CreateContext("rhx-tooltip"), output);
        Assert.Equal(Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, output.TagMode);
    }
}
