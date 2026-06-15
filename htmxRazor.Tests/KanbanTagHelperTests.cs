using htmxRazor.Components.Organization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

public class KanbanTagHelperTests : TagHelperTestBase
{
    private KanbanTagHelper CreateHelper()
    {
        var helper = new KanbanTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    [Fact]
    public async Task Renders_Div_With_Block_Class()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-kanban");
        var output = CreateOutput("rhx-kanban");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-kanban"));
    }

    [Fact]
    public async Task Has_No_Js_Data_Hook()
    {
        // The board is pure layout now — no JS, so no data-rhx-kanban hook.
        var helper = CreateHelper();
        var context = CreateContext("rhx-kanban");
        var output = CreateOutput("rhx-kanban");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-kanban");
    }

    [Fact]
    public async Task Sets_Role_Group()
    {
        var helper = CreateHelper();
        var context = CreateContext("rhx-kanban");
        var output = CreateOutput("rhx-kanban");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "role", "group");
    }

    [Fact]
    public async Task Custom_Css_Class_Appended()
    {
        var helper = CreateHelper();
        helper.CssClass = "my-board";
        var context = CreateContext("rhx-kanban");
        var output = CreateOutput("rhx-kanban");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-kanban"));
        Assert.True(HasClass(output, "my-board"));
    }
}
