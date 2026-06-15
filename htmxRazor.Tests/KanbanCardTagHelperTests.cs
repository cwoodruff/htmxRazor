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

        AssertAttribute(output, "data-rhx-card-id", "task-42");
    }

    [Fact]
    public async Task Renders_Htmx_Move_Buttons()
    {
        // With an hx verb configured, the card shows ◀/▶ move buttons that carry the card id
        // and direction in hx-vals (zero JS; no HTML drag-and-drop).
        var helper = CreateHelper();
        helper.CardId = "task-1";
        helper.HxPost = "/Board?handler=Move";
        helper.HxTarget = "#board";
        helper.HxSwap = "innerHTML";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        var content = output.Content.GetContent();
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(content, "rhx-kanban-card__move").Count);
        Assert.Contains("hx-post=\"/Board?handler=Move\"", content);
        Assert.Contains("\"cardId\":\"task-1\"", content);
        Assert.Contains("\"direction\":\"prev\"", content);
        Assert.Contains("\"direction\":\"next\"", content);
        // No drag-and-drop attributes.
        AssertNoAttribute(output, "draggable");
    }

    [Fact]
    public async Task No_Move_Buttons_Without_Htmx_Verb()
    {
        var helper = CreateHelper();
        helper.CardId = "task-1";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        Assert.DoesNotContain("rhx-kanban-card__move", output.Content.GetContent());
    }

    [Fact]
    public async Task Draggable_False_Suppresses_Move_Buttons()
    {
        var helper = CreateHelper();
        helper.CardId = "task-1";
        helper.Draggable = false;
        helper.HxPost = "/Board?handler=Move";
        var context = CreateContext("rhx-kanban-card");
        var output = CreateOutput("rhx-kanban-card");

        await helper.ProcessAsync(context, output);

        Assert.DoesNotContain("rhx-kanban-card__move", output.Content.GetContent());
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
