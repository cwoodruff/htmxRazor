using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class KanbanModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-column-id", "string", "-", "Unique column identifier"),
        new("rhx-title", "string", "-", "Column header display text"),
        new("rhx-max-cards", "int", "-", "Work-in-progress limit (visual indicator when exceeded)"),
        new("rhx-card-id", "string", "-", "Unique card identifier (sent as cardId with the move request)"),
        new("rhx-draggable", "bool", "true", "Whether to render the htmx move buttons on the card"),
        new("rhx-variant", "string", "-", "Card color variant: brand, success, warning, danger"),
    };

    public string BasicCode => @"<rhx-kanban>
    <rhx-kanban-column rhx-column-id=""todo"" rhx-title=""To Do"">
        <rhx-kanban-card rhx-card-id=""1""
                         hx-post=""/Board?handler=Move""
                         hx-target=""#board"" hx-swap=""innerHTML"">
            Design homepage
        </rhx-kanban-card>
    </rhx-kanban-column>
    <rhx-kanban-column rhx-column-id=""doing"" rhx-title=""In Progress""
                       rhx-max-cards=""3"" />
    <rhx-kanban-column rhx-column-id=""done"" rhx-title=""Done"" />
</rhx-kanban>";

    public string WipCode => @"<!-- Column shows a warning when card count >= max -->
<rhx-kanban-column rhx-column-id=""doing""
                   rhx-title=""In Progress""
                   rhx-max-cards=""3"">
    ...
</rhx-kanban-column>";

    public string ServerCode => @"// The move buttons send cardId + direction (""prev""/""next"").
public IActionResult OnPostMove(string cardId, string direction)
{
    _service.MoveCard(cardId, direction);   // resolve the adjacent column server-side
    return Partial(""_BoardPartial"", _service.GetTasks());
}";

    // Thread-safe in-memory task store for the demo
    private static readonly Lock _lock = new();
    private static List<KanbanTask> _tasks = new()
    {
        new("1", "todo", 0, "Design homepage layout", "brand"),
        new("2", "todo", 1, "Write API documentation", null),
        new("3", "todo", 2, "Set up CI/CD pipeline", "success"),
        new("4", "doing", 0, "Implement auth flow", "warning"),
        new("5", "doing", 1, "Review pull requests", null),
        new("6", "done", 0, "Database schema design", "success"),
    };

    public List<KanbanTask> Tasks
    {
        get { lock (_lock) { return _tasks.OrderBy(t => t.Position).ToList(); } }
    }

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/Kanban"),
            new("Kanban")
        };
    }

    private static readonly string[] ColumnOrder = { "todo", "doing", "done" };

    // The htmx move buttons send cardId + direction ("prev"/"next"); the server resolves the
    // adjacent column and re-renders the board. No drag-and-drop, no client JavaScript.
    public IActionResult OnPostMove(string cardId, string direction)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == cardId);
            if (task is null)
                return Partial("_KanbanPartial", Tasks);

            var fromIndex = Array.IndexOf(ColumnOrder, task.Column);
            if (fromIndex < 0)
                return Partial("_KanbanPartial", Tasks);

            var toIndex = Math.Clamp(
                direction == "prev" ? fromIndex - 1 : fromIndex + 1, 0, ColumnOrder.Length - 1);
            if (toIndex == fromIndex)
                return Partial("_KanbanPartial", Tasks);

            var targetColumn = ColumnOrder[toIndex];
            var append = _tasks.Count(t => t.Column == targetColumn);

            _tasks.Remove(task);
            _tasks.Add(task with { Column = targetColumn, Position = append });
        }

        return Partial("_KanbanPartial", Tasks);
    }

    public IActionResult OnPostReset()
    {
        lock (_lock)
        {
            _tasks = new()
            {
                new("1", "todo", 0, "Design homepage layout", "brand"),
                new("2", "todo", 1, "Write API documentation", null),
                new("3", "todo", 2, "Set up CI/CD pipeline", "success"),
                new("4", "doing", 0, "Implement auth flow", "warning"),
                new("5", "doing", 1, "Review pull requests", null),
                new("6", "done", 0, "Database schema design", "success"),
            };
        }

        return Partial("_KanbanPartial", Tasks);
    }
}

public record KanbanTask(string Id, string Column, int Position, string Title, string? Variant);
