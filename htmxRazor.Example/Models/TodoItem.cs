using System.ComponentModel.DataAnnotations;

namespace htmxRazor.Example.Models;

public class TodoItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Task title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public TodoPriority Priority { get; set; } = TodoPriority.Medium;

    // ── 2.1 Advanced Inputs ──
    /// <summary>Optional due date (Date Picker, <c>rhx-date-picker</c>).</summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>Optional due time, only meaningful with <see cref="DueDate"/> (Time Picker, <c>rhx-time-picker</c>).</summary>
    public TimeOnly? DueTime { get; set; }

    /// <summary>Optional reminder instant (Date &amp; Time Picker, <c>rhx-datetime-picker</c>).</summary>
    public DateTime? ReminderAt { get; set; }

    /// <summary>Optional category (Radial Select wedge, <c>rhx-radial-select</c>). One of <see cref="TodoCategory.All"/>.</summary>
    public string? Category { get; set; }

    /// <summary>Optional item within the category (Radial Select cascade dropdown). One of the category's <see cref="TodoCategory.Items"/>.</summary>
    public string? CategoryItem { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    /// <summary>Combined due instant: the due date at its due time, or end-of-day when no time is set.</summary>
    public DateTime? DueAt => DueDate is { } d
        ? d.ToDateTime(DueTime ?? new TimeOnly(23, 59))
        : null;

    /// <summary>Incomplete and past its due instant.</summary>
    public bool IsOverdue => !IsCompleted && DueAt is { } d && d < DateTime.Now;
}

public enum TodoPriority
{
    Low,
    Medium,
    High
}

/// <summary>
/// Category metadata shared by the Radial Select wedges and the task-card tag, so colors and
/// icons stay in sync. Icon names are known to <c>IconRegistry</c>.
/// </summary>
public record TodoCategory(string Name, string Color, string Icon, string[] Items)
{
    public static readonly IReadOnlyList<TodoCategory> All = new[]
    {
        new TodoCategory("Work", "brand", "grid", new[] { "Email", "Meeting", "Report", "Review", "Call" }),
        new TodoCategory("Personal", "success", "heart", new[] { "Errand", "Appointment", "Family", "Hobby" }),
        new TodoCategory("Shopping", "warning", "star", new[] { "Groceries", "Household", "Gifts", "Clothing" }),
        new TodoCategory("Health", "danger", "check-circle", new[] { "Exercise", "Doctor", "Medication", "Meal" }),
    };

    public static TodoCategory? Find(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? null
            : All.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>The valid item name within this category (case-insensitive), or null.</summary>
    public string? FindItem(string? item) =>
        string.IsNullOrWhiteSpace(item)
            ? null
            : Items.FirstOrDefault(i => string.Equals(i, item, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// Represents an entry in the activity log displayed as a timeline.
/// </summary>
public class ActivityEntry
{
    public string Message { get; set; } = string.Empty;
    public string Variant { get; set; } = "neutral";
    public string Icon { get; set; } = "info";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
