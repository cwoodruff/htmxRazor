using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class DateTimePickerModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-for", "ModelExpression", "-", "Binds DateTime for two-way model binding"),
        new("name", "string", "-", "Form field name for the hidden ISO (yyyy-MM-ddTHH:mm) value"),
        new("rhx-placeholder", "string", "-", "Placeholder for the text input"),
        new("rhx-min", "string", "-", "Earliest selectable date (ISO yyyy-MM-dd); bounds the calendar only"),
        new("rhx-max", "string", "-", "Latest selectable date (ISO yyyy-MM-dd); bounds the calendar only"),
        new("rhx-week-start", "string", "mon", "First day of the week: mon or sun"),
        new("rhx-step", "int", "30", "Minutes between time options"),
        new("rhx-12hour", "bool", "true", "12-hour vs 24-hour time display"),
        new("rhx-date-format", "string", "-", ".NET date format for display (default: culture short date)"),
        new("rhx-time-format", "string", "-", ".NET time format for display (overrides 12/24h)"),
        new("rhx-size", "string", "medium", "small, medium, large"),
        new("rhx-disabled", "bool", "false", "Disable the control"),
    };

    public string BasicCode => "<rhx-datetime-picker name=\"StartsAt\" rhx-placeholder=\"Pick date & time…\" rhx-step=\"30\" rhx-week-start=\"mon\" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/DateTimePicker"),
            new("Date & Time Picker"),
        };
    }
}
