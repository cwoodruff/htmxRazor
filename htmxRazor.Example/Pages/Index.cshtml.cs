using htmxRazor.Example.Models;
using htmxRazor.Example.Services;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmxRazor.Example.Pages;

public class IndexModel : PageModel
{
    private readonly TodoService _todoService;

    public IndexModel(TodoService todoService)
    {
        _todoService = todoService;
    }

    public List<TodoItem> Todos { get; set; } = [];
    public string? Filter { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int TotalCount => _todoService.TotalCount;
    public int ActiveCount => _todoService.ActiveCount;
    public int CompletedCount => _todoService.CompletedCount;
    public int DueSoonCount => _todoService.DueSoonCount;
    public int ProgressPercent => TotalCount == 0 ? 0 : (int)Math.Round(100.0 * CompletedCount / TotalCount);
    public List<ActivityEntry> Activities => _todoService.GetRecentActivity();

    public void OnGet(string? filter, string? search, string? sort)
    {
        Filter = filter;
        Search = search;
        Sort = sort;
        Todos = _todoService.GetFiltered(filter, search, sort);
    }

    public IActionResult OnGetTodoList(string? filter, string? search, string? sort)
    {
        Filter = filter;
        Search = search;
        Sort = sort;
        Todos = _todoService.GetFiltered(filter, search, sort);
        return Partial("_TodoList", Todos);
    }

    /// <summary>Cascade endpoint for the category Radial Select: returns the listbox option set for
    /// the chosen wedge (multiple items per category). The active wedge is submitted via
    /// <c>rhx-category-name</c>; the selected item via the control's <c>name</c>. An optional
    /// <c>selected</c> marks one option pre-selected (used when re-opening a wedge on edit).</summary>
    public IActionResult OnGetCategoryItems(string? category, string? selected)
    {
        var cat = TodoCategory.Find(category);
        if (cat is null || cat.Items.Length == 0)
            return Content("<div class=\"rhx-radial-select__placeholder\">No items</div>", "text/html");

        var sel = cat.FindItem(selected);
        var html = string.Concat(cat.Items.Select(item =>
        {
            var v = System.Net.WebUtility.HtmlEncode(item);
            var isSel = string.Equals(item, sel, StringComparison.Ordinal) ? "true" : "false";
            return $"<div class=\"rhx-radial-select__option\" role=\"option\" data-value=\"{v}\" aria-selected=\"{isSel}\" tabindex=\"-1\">{v}</div>";
        }));
        return Content(html, "text/html");
    }

    /// <summary>Returns the populated edit form (server-rendered so the date/time/category controls
    /// bind their initial values via <c>rhx-value</c>/<c>rhx-default-category</c>).</summary>
    public IActionResult OnGetEditForm(int id)
    {
        var todo = _todoService.GetById(id);
        if (todo is null) return NotFound();
        return Partial("_EditForm", todo);
    }

    public IActionResult OnGetStats()
    {
        return Partial("_Stats", this);
    }

    public IActionResult OnGetProgress()
    {
        return Partial("_Progress", this);
    }

    public IActionResult OnGetActivityLog()
    {
        return Partial("_ActivityLog", _todoService.GetRecentActivity());
    }

    public IActionResult OnPostAdd(string title, TodoPriority priority, DateOnly? dueDate,
        TimeOnly? dueTime, DateTime? reminderAt, string? category, string? categoryItem, string? filter, string? sort)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            _todoService.Add(title.Trim(), priority, dueDate, dueTime, reminderAt, category, categoryItem);
            var due = dueDate is { } d ? $" (due {d:MMM d})" : "";
            _todoService.LogActivity($"Added \"{title.Trim()}\"{due}", "success", "plus");
            Response.HxToast($"Task \"{title.Trim()}\" added", "success");
        }

        Response.HxTrigger("todoChanged");
        Todos = _todoService.GetFiltered(filter, null, sort);
        return Partial("_TodoList", Todos);
    }

    public IActionResult OnPostToggle(int id, string? filter, string? sort)
    {
        var todo = _todoService.Toggle(id);
        if (todo is not null)
        {
            var action = todo.IsCompleted ? "completed" : "reopened";
            var variant = todo.IsCompleted ? "success" : "warning";
            var icon = todo.IsCompleted ? "check-circle" : "refresh";
            _todoService.LogActivity($"{char.ToUpper(action[0])}{action[1..]} \"{todo.Title}\"", variant, icon);
            Response.HxToast($"Task {action}", variant);
        }

        Response.HxTrigger("todoChanged");
        Todos = _todoService.GetFiltered(filter, null, sort);
        return Partial("_TodoList", Todos);
    }

    public IActionResult OnPutUpdate(int id, string title, TodoPriority priority, DateOnly? dueDate,
        TimeOnly? dueTime, DateTime? reminderAt, string? category, string? categoryItem, string? filter, string? sort)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            _todoService.Update(id, title.Trim(), priority, dueDate, dueTime, reminderAt, category, categoryItem);
            _todoService.LogActivity($"Updated \"{title.Trim()}\"", "brand", "edit");
            Response.HxToast("Task updated", "brand");
        }

        Response.HxTrigger("todoChanged");
        Todos = _todoService.GetFiltered(filter, null, sort);
        return Partial("_TodoList", Todos);
    }

    public IActionResult OnDeleteDelete(int id, string? filter, string? sort)
    {
        var todo = _todoService.GetById(id);
        _todoService.Delete(id);
        if (todo is not null)
        {
            _todoService.LogActivity($"Deleted \"{todo.Title}\"", "danger", "trash");
            Response.HxToast("Task deleted", "danger");
        }

        Response.HxTrigger("todoChanged");
        Todos = _todoService.GetFiltered(filter, null, sort);
        return Partial("_TodoList", Todos);
    }

    public IActionResult OnDeleteClearCompleted(string? filter, string? sort)
    {
        var count = _todoService.ClearCompleted();
        _todoService.LogActivity($"Cleared {count} completed task{(count != 1 ? "s" : "")}", "danger", "trash");
        Response.HxToast($"Cleared {count} completed task{(count != 1 ? "s" : "")}", "danger");

        Response.HxTrigger("todoChanged");
        Todos = _todoService.GetFiltered(filter, null, sort);
        return Partial("_TodoList", Todos);
    }
}
