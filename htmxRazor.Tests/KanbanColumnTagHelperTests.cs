using htmxRazor.Components.Organization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

public class KanbanColumnTagHelperTests : TagHelperTestBase
{
    private KanbanColumnTagHelper CreateHelper()
    {
        var helper = new KanbanColumnTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    [Fact]
    public async Task Renders_Div_With_Block_Class()
    {
        var helper = CreateHelper();
        helper.ColumnId = "todo";
        var context = CreateContext("rhx-kanban-column");
        var output = CreateOutput("rhx-kanban-column");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-kanban-column"));
    }

    [Fact]
    public async Task Sets_Column_Id_Attribute()
    {
        var helper = CreateHelper();
        helper.ColumnId = "in-progress";
        var context = CreateContext("rhx-kanban-column");
        var output = CreateOutput("rhx-kanban-column");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-kanban-column", "");
        AssertAttribute(output, "data-rhx-column-id", "in-progress");
    }

    [Fact]
    public async Task Renders_Title_In_Header()
    {
        var helper = CreateHelper();
        helper.ColumnId = "todo";
        helper.Title = "To Do";
        var context = CreateContext("rhx-kanban-column");
        var output = CreateOutput("rhx-kanban-column");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-kanban-column__header", content);
        Assert.Contains("rhx-kanban-column__title", content);
        Assert.Contains("To Do", content);
    }

    [Fact]
    public async Task Renders_Drop_Zone_Body()
    {
        var helper = CreateHelper();
        helper.ColumnId = "todo";
        var context = CreateContext("rhx-kanban-column");
        var output = CreateOutput("rhx-kanban-column");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("rhx-kanban-column__body", content);
        Assert.Contains("data-rhx-drop-zone", content);
    }

    [Fact]
    public async Task Sets_Max_Cards_Attribute()
    {
        var helper = CreateHelper();
        helper.ColumnId = "todo";
        helper.MaxCards = 5;
        var context = CreateContext("rhx-kanban-column");
        var output = CreateOutput("rhx-kanban-column");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-max-cards", "5");
    }

    [Fact]
    public async Task Omits_Max_Cards_When_Not_Set()
    {
        var helper = CreateHelper();
        helper.ColumnId = "todo";
        var context = CreateContext("rhx-kanban-column");
        var output = CreateOutput("rhx-kanban-column");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-max-cards");
    }

    [Fact]
    public async Task Sets_No_Drop_Attribute_When_Not_Droppable()
    {
        var helper = CreateHelper();
        helper.ColumnId = "archive";
        helper.Droppable = false;
        var context = CreateContext("rhx-kanban-column");
        var output = CreateOutput("rhx-kanban-column");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-no-drop", "");
    }

    [Fact]
    public async Task Omits_No_Drop_When_Droppable()
    {
        var helper = CreateHelper();
        helper.ColumnId = "todo";
        var context = CreateContext("rhx-kanban-column");
        var output = CreateOutput("rhx-kanban-column");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "data-rhx-no-drop");
    }
}
