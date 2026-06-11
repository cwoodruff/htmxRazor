using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class DatePickerModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-for", "ModelExpression", "-", "Binds DateOnly/DateTime for two-way model binding"),
        new("name", "string", "-", "Form field name for the hidden ISO (yyyy-MM-dd) value"),
        new("rhx-placeholder", "string", "-", "Placeholder for the text input"),
        new("rhx-min", "string", "-", "Earliest selectable date (ISO yyyy-MM-dd)"),
        new("rhx-max", "string", "-", "Latest selectable date (ISO yyyy-MM-dd)"),
        new("rhx-week-start", "string", "mon", "First day of the week: mon or sun"),
        new("rhx-format", "string", "-", ".NET format string for the visible display (default: culture short date)"),
        new("rhx-size", "string", "medium", "small, medium, large"),
        new("rhx-disabled", "bool", "false", "Disable the control"),
    };

    public string BasicCode => "<rhx-date-picker name=\"DueDate\" rhx-placeholder=\"Pick a date…\" rhx-week-start=\"mon\" />";
    public string MinMaxCode => "<rhx-date-picker name=\"d\" rhx-min=\"2026-01-01\" rhx-max=\"2026-12-31\" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/DatePicker"),
            new("Date Picker"),
        };
    }
}
