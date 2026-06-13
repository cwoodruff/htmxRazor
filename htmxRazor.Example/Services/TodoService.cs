using htmxRazor.Example.Models;

namespace htmxRazor.Example.Services;

public class TodoService
{
    /// <summary>A task is "due soon" when it falls due within this many days from now.</summary>
    public const int DueSoonDays = 3;

    private static readonly List<TodoItem> _todos = new()
    {
        new() { Id = 1, Title = "Set up htmxRazor project", IsCompleted = true, Priority = TodoPriority.High, Category = "Work", CreatedAt = DateTime.UtcNow.AddDays(-3), CompletedAt = DateTime.UtcNow.AddDays(-2) },
        new() { Id = 2, Title = "Build the To-Do app UI", IsCompleted = false, Priority = TodoPriority.High, Category = "Work", DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)), DueTime = new TimeOnly(17, 0), CreatedAt = DateTime.UtcNow.AddDays(-2) },
        new() { Id = 3, Title = "Add dark mode support", IsCompleted = false, Priority = TodoPriority.Medium, Category = "Personal", DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)), CreatedAt = DateTime.UtcNow.AddDays(-1) },
        new() { Id = 4, Title = "Write documentation", IsCompleted = false, Priority = TodoPriority.Low, CreatedAt = DateTime.UtcNow },
    };

    private static int _nextId = 5;
    private static readonly object _lock = new();

    public List<TodoItem> GetAll() => _todos.OrderByDescending(t => t.CreatedAt).ToList();

    /// <summary>Incomplete and due within the next <see cref="DueSoonDays"/> days (not yet past).</summary>
    private static bool IsDueSoon(TodoItem t)
    {
        if (t.IsCompleted || t.DueAt is not { } due) return false;
        var now = DateTime.Now;
        return due >= now && due <= now.AddDays(DueSoonDays);
    }

    public List<TodoItem> GetFiltered(string? filter, string? search, string? sort = null)
    {
        var query = _todos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

        query = filter switch
        {
            "active" => query.Where(t => !t.IsCompleted),
            "completed" => query.Where(t => t.IsCompleted),
            "duesoon" => query.Where(IsDueSoon),
            _ => query
        };

        // "due" sorts by due instant ascending with no-due-date tasks last; default keeps newest-first.
        query = sort == "due"
            ? query.OrderBy(t => t.DueAt ?? DateTime.MaxValue).ThenByDescending(t => t.CreatedAt)
            : query.OrderByDescending(t => t.CreatedAt);

        return query.ToList();
    }

    public TodoItem? GetById(int id) => _todos.FirstOrDefault(t => t.Id == id);

    public TodoItem Add(string title, TodoPriority priority, DateOnly? dueDate = null,
        TimeOnly? dueTime = null, DateTime? reminderAt = null, string? category = null)
    {
        lock (_lock)
        {
            var todo = new TodoItem
            {
                Id = _nextId++,
                Title = title,
                Priority = priority,
                DueDate = dueDate,
                DueTime = dueTime,
                ReminderAt = reminderAt,
                Category = TodoCategory.Find(category)?.Name,
                CreatedAt = DateTime.UtcNow
            };
            _todos.Add(todo);
            return todo;
        }
    }

    public TodoItem? Toggle(int id)
    {
        var todo = GetById(id);
        if (todo is null) return null;

        todo.IsCompleted = !todo.IsCompleted;
        todo.CompletedAt = todo.IsCompleted ? DateTime.UtcNow : null;
        return todo;
    }

    public TodoItem? Update(int id, string title, TodoPriority priority, DateOnly? dueDate = null,
        TimeOnly? dueTime = null, DateTime? reminderAt = null, string? category = null)
    {
        var todo = GetById(id);
        if (todo is null) return null;

        todo.Title = title;
        todo.Priority = priority;
        todo.DueDate = dueDate;
        todo.DueTime = dueTime;
        todo.ReminderAt = reminderAt;
        todo.Category = TodoCategory.Find(category)?.Name;
        return todo;
    }

    public bool Delete(int id)
    {
        var todo = GetById(id);
        if (todo is null) return false;
        return _todos.Remove(todo);
    }

    public int ClearCompleted()
    {
        var count = _todos.RemoveAll(t => t.IsCompleted);
        return count;
    }

    public int TotalCount => _todos.Count;
    public int ActiveCount => _todos.Count(t => !t.IsCompleted);
    public int CompletedCount => _todos.Count(t => t.IsCompleted);
    public int DueSoonCount => _todos.Count(IsDueSoon);

    // ── Activity Log ──

    private static readonly List<ActivityEntry> _activities = new()
    {
        new() { Message = "Project created", Variant = "brand", Icon = "check-circle", Timestamp = DateTime.UtcNow.AddDays(-3) },
    };

    public List<ActivityEntry> GetRecentActivity(int count = 8) =>
        _activities.OrderByDescending(a => a.Timestamp).Take(count).ToList();

    public void LogActivity(string message, string variant = "neutral", string icon = "info")
    {
        _activities.Add(new ActivityEntry
        {
            Message = message,
            Variant = variant,
            Icon = icon,
            Timestamp = DateTime.UtcNow
        });
    }
}
