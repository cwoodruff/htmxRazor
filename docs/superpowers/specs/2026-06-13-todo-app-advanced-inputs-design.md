# To Do App — 2.1 Advanced Inputs Integration

**Date:** 2026-06-13
**Status:** Approved (design)
**Scope:** `htmxRazor.Example` project only

## Goal

Showcase all four components from the 2.1 "Advanced Inputs" release inside the
example To Do web app, each in a natural home, plus the surfacing features
(overdue badge, "Due Soon" filter, due-date sort) that the new data makes possible.

| Component | Tag | Home in the app |
|---|---|---|
| Date Picker | `<rhx-date-picker>` | Task **due date** (optional) |
| Time Picker | `<rhx-time-picker>` | Task **due time** (optional, pairs with due date) |
| Date & Time Picker | `<rhx-datetime-picker>` | Task **reminder** (optional) |
| Radial Select | `<rhx-radial-select>` + `<rhx-radial-option>` | Task **category** (Work / Personal / Shopping / Health) |

Priority remains the existing `<rhx-select>` — the Radial Select is a
category→items cascade, not a flat value picker, so it powers a new Category
concept rather than replacing Priority.

## Model — `Models/TodoItem.cs`

Add to `TodoItem`:

```csharp
public DateOnly? DueDate    { get; set; }   // Date Picker     → ISO yyyy-MM-dd
public TimeOnly? DueTime    { get; set; }   // Time Picker     → ISO HH:mm
public DateTime? ReminderAt { get; set; }   // DateTime Picker → yyyy-MM-ddTHH:mm
public string?   Category   { get; set; }   // Radial Select   → wedge value
```

Computed helper for overdue/sort logic:

```csharp
// Combined due instant; date with optional time (end-of-day if no time given).
public DateTime? DueAt => DueDate is null
    ? null
    : DueDate.Value.ToDateTime(DueTime ?? new TimeOnly(23, 59));

public bool IsOverdue => !IsCompleted && DueAt is { } d && d < DateTime.Now;
```

`Label` is intentionally **not** stored (Category-only decision). The cascade
dropdown still must function for the control to be usable, so each wedge's
endpoint returns a single confirming option echoing the category; its `name`
value is ignored server-side.

## Categories (Radial wedges)

| Value | Color token | Icon (IconRegistry) |
|---|---|---|
| Work | brand | grid |
| Personal | success | heart |
| Shopping | warning | star |
| Health | danger | check-circle |

Icons are limited to names confirmed present in `IconRegistry`. If a chosen
icon is absent at build time, fall back to `info`.

## Service — `Services/TodoService.cs`

- `Add(...)` and `Update(...)` gain `DateOnly? dueDate, TimeOnly? dueTime,
  DateTime? reminderAt, string? category` parameters and assign them.
- `GetFiltered(string? filter, string? search, string? sort)`:
  - New filter branch `"duesoon"` → `!IsCompleted && DueAt` within the next 3 days
    (inclusive of overdue-but-incomplete? **No** — Due Soon = due today..+3 days,
    not yet past; overdue tasks are surfaced via the badge, not this tab).
  - `sort`: `"due"` orders by `DueAt` ascending with nulls last, then by
    `CreatedAt` desc as a tiebreak; default (`"created"`/null) keeps the
    current `CreatedAt` desc.
- New counter `DueSoonCount` (incomplete, `DueAt` within next 3 days, not past).
- Seed data: give 1–2 of the existing seed todos a `DueDate`/`Category` so the
  new UI is non-empty on first load (e.g. "Build the To-Do app UI" due in 2 days,
  Category Work).

## Cascade handler — `Pages/Index.cshtml.cs`

```
OnGetCategoryItems(string? category)
  → returns a single listbox option fragment echoing the category, e.g.:
    <div class="rhx-radial-select__option" role="option"
         data-value="{category}" aria-selected="false" tabindex="-1">{category}</div>
  → unknown/empty category → "No category" placeholder fragment.
```

Modelled on the demo's `OnGetItems` (`RadialSelect.cshtml.cs`), HTML-encoded.

## Page — `Pages/Index.cshtml`

### Add dialog (`add-todo-dialog`)
Add, after Priority:
- `<rhx-date-picker rhx-label="Due date" name="dueDate" rhx-min="@DateTime.Today.ToString("yyyy-MM-dd")">`
- `<rhx-time-picker rhx-label="Due time" name="dueTime">`
- `<rhx-datetime-picker rhx-label="Reminder" name="reminderAt">`
- `<rhx-radial-select name="categoryItem" rhx-category-name="category"
     rhx-placeholder="Pick a category…" aria-label="Task category">` with four
  `<rhx-radial-option>` wedges, each `hx-get="/?handler=CategoryItems&category=Work"` etc.

### Edit dialog (`edit-todo-dialog`)
Same four controls with `id="edit-*"`, populated by `openEditDialog`.

### Toolbar
Add a `<rhx-select name="sort">` (Newest / Due date) beside the search input,
wired with `hx-get="/?handler=TodoList"`, `hx-trigger="change"`,
`hx-target="#todo-list"`, `hx-include="[name='filter']"`.

### Filter tabs
Add a 4th tab **Due Soon** (`filter=duesoon`) after Completed with a
`<rhx-badge rhx-variant="warning">` showing `DueSoonCount`; add matching
`#badge-duesoon` OOB span in `_Stats.cshtml`.

### Edit dialog JS (`openEditDialog`)
Extend signature to receive due date/time, reminder, category and set:
- date picker: hidden ISO input + visible text input
- time picker: hidden ISO input + visible text input
- datetime picker: hidden ISO input + visible text input
- radial: set the active category (trigger echo + hidden category field), fire
  the cascade so the dropdown is populated.

Exact DOM hooks (hidden input selectors, trigger data-attributes) to be
confirmed against each control's rendered markup during implementation; this is
the fiddliest part and gets verified by reading the Tag Helper output.

## Surfacing — `Pages/_TodoList.cshtml`

In each task's `__meta` row, after the priority badge:
- Category tag: `<rhx-tag-item rhx-variant="{categoryColor}">` with the category
  icon + name, when `Category` is set.
- Due indicator:
  - `todo.IsOverdue` → `<rhx-badge rhx-variant="danger">⚠ Overdue</rhx-badge>`
  - else if `DueAt` set and future → neutral `Due {Mmm d}` (with time if `DueTime`).
- Reminder: small muted "🔔 {date}" hint when `ReminderAt` set (optional polish).

## Stats — `Pages/_Stats.cshtml`

Add `#badge-duesoon` OOB span alongside the existing all/active/completed OOB
spans so the new tab badge stays live on `todoChanged`.

## Handlers — `Pages/Index.cshtml.cs`

- `OnPostAdd` / `OnPutUpdate` accept and forward the new fields; activity-log
  messages may note the due date.
- `OnGetTodoList` accepts `sort` and threads it through `GetFiltered`.
- All existing `Response.HxTrigger("todoChanged")` / toast behavior preserved.

## Out of scope (YAGNI)

- Storing the radial **Label** (Category-only).
- Reminder notifications / background scheduling (display only).
- Persisting beyond the in-memory static list.
- Unit/E2E tests for the example project (it has none today).

## Verification

Build-only: `dotnet build htmxRazor.Example`. Report success; user will click
through interactively.

## Risk / fiddly areas

1. **Edit-dialog population** of compound controls — depends on each Tag
   Helper's rendered hidden-input + display-input structure. Mitigation: read
   the rendered markup of date/time/datetime/radial Tag Helpers before writing
   the JS.
2. **Model binding** of `DateOnly?`/`TimeOnly?` from the pickers' ISO hidden
   values — verify the hidden field `name` matches the handler parameter.
