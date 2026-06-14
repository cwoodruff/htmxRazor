using htmxRazor.Components.Forms;
using Xunit;

namespace htmxRazor.Tests;

public class OptimisticRatingTests : TagHelperTestBase
{
    private RatingTagHelper CreateHelper()
    {
        var helper = new RatingTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    [Fact]
    public void Optimistic_True_Adds_Data_Attribute()
    {
        var helper = CreateHelper();
        helper.Optimistic = true;

        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.Process(context, output);

        Assert.True(output.Attributes.TryGetAttribute("data-rhx-optimistic", out _));
    }

    [Fact]
    public void Optimistic_False_No_Data_Attribute()
    {
        var helper = CreateHelper();
        helper.Optimistic = false;

        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.Process(context, output);

        Assert.False(output.Attributes.TryGetAttribute("data-rhx-optimistic", out _));
    }

    [Fact]
    public void Optimistic_Preserves_Rating_Behavior()
    {
        var helper = CreateHelper();
        helper.Optimistic = true;
        helper.Max = 5;

        var context = CreateContext("rhx-rating");
        var output = CreateOutput("rhx-rating");

        helper.Process(context, output);

        Assert.True(HasClass(output, "rhx-rating"));
        Assert.True(output.Attributes.TryGetAttribute("data-rhx-optimistic", out _));
        // Interactive ratings render a radiogroup of native radios (JS-free).
        Assert.Contains("role=\"radiogroup\"", output.Content.GetContent());
    }
}
