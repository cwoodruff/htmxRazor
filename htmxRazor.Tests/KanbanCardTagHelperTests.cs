using htmxRazor.Components.Organization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace htmxRazor.Tests;

public class KanbanCardTagHelperTests : TagHelperTestBase
{
    private KanbanCardTagHelper CreateHelper()
    {
        var helper = new KanbanCardTagHelper(CreateUrlHelperFactory());
        helper.ViewContext = CreateViewContext();
        return helper;
    }

    [Fact]
    public async Task Renders_Div_With_Block_Class()
    {
        var helper = CreateHelper();
        helper.CardId = "task-1";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        Assert.Equal("div", output.TagName);
        Assert.True(HasClass(output, "rhx-kanban-card"));
    }

    [Fact]
    public async Task Sets_Card_Id_Attribute()
    {
        var helper = CreateHelper();
        helper.CardId = "task-42";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "data-rhx-kanban-card", "");
        AssertAttribute(output, "data-rhx-card-id", "task-42");
    }

    [Fact]
    public async Task Sets_Draggable_And_Tabindex()
    {
        var helper = CreateHelper();
        helper.CardId = "task-1";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        AssertAttribute(output, "draggable", "true");
        AssertAttribute(output, "tabindex", "0");
    }

    [Fact]
    public async Task Omits_Draggable_When_Disabled()
    {
        var helper = CreateHelper();
        helper.CardId = "task-1";
        helper.Draggable = false;
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        AssertNoAttribute(output, "draggable");
        AssertNoAttribute(output, "tabindex");
    }

    [Fact]
    public async Task Adds_Variant_Modifier_Class()
    {
        var helper = CreateHelper();
        helper.CardId = "task-1";
        helper.Variant = "brand";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-kanban-card--brand"));
    }

    [Fact]
    public async Task Adds_Success_Variant()
    {
        var helper = CreateHelper();
        helper.CardId = "task-1";
        helper.Variant = "success";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-kanban-card--success"));
    }

    [Fact]
    public async Task Omits_Variant_When_Not_Set()
    {
        var helper = CreateHelper();
        helper.CardId = "task-1";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        Assert.False(HasClass(output, "rhx-kanban-card--brand"));
        Assert.False(HasClass(output, "rhx-kanban-card--success"));
    }

    [Fact]
    public async Task Preserves_Child_Content()
    {
        var helper = CreateHelper();
        helper.CardId = "task-1";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card", childContent: "<strong>My Task</strong>");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Contains("My Task", content);
    }

    [Fact]
    public async Task Custom_Css_Class_Appended()
    {
        var helper = CreateHelper();
        helper.CardId = "task-1";
        helper.CssClass = "priority-high";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        Assert.True(HasClass(output, "rhx-kanban-card"));
        Assert.True(HasClass(output, "priority-high"));
    }
}
