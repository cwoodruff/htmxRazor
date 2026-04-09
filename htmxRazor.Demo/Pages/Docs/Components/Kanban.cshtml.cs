using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class KanbanModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-column-id", "string", "-", "Unique column identifier (sent with drop POST)"),
        new("rhx-title", "string", "-", "Column header display text"),
        new("rhx-max-cards", "int", "-", "Work-in-progress limit (visual indicator when exceeded)"),
        new("rhx-droppable", "bool", "true", "Whether cards can be dropped into this column"),
        new("rhx-card-id", "string", "-", "Unique card identifier (sent with drop POST)"),
        new("rhx-draggable", "bool", "true", "Whether this card can be dragged"),
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

    public string ServerCode => @"public IActionResult OnPostMove(
    string cardId, string sourceColumn,
    string targetColumn, int position)
{
    _service.MoveCard(cardId, targetColumn, position);
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

    public IActionResult OnPostMove(string cardId, string sourceColumn, string targetColumn, int position)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == cardId);
            if (task is null)
                return Partial("_KanbanPartial", Tasks);

            // Remove from old position
            _tasks.Remove(task);

            // Shift existing cards in the target column to make room
            var targetCards = _tasks
                .Where(t => t.Column == targetColumn)
                .OrderBy(t => t.Position)
                .ToList();

            for (var i = 0; i < targetCards.Count; i++)
            {
                var newPos = i >= position ? i + 1 : i;
                var idx = _tasks.IndexOf(targetCards[i]);
                _tasks[idx] = targetCards[i] with { Position = newPos };
            }

            // Insert the moved card at the requested position
            _tasks.Add(task with { Column = targetColumn, Position = position });
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
