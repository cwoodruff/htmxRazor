using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class TimePickerModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-for", "ModelExpression", "-", "Binds TimeOnly/DateTime for two-way model binding"),
        new("name", "string", "-", "Form field name for the hidden ISO (HH:mm) value"),
        new("rhx-placeholder", "string", "-", "Placeholder for the text input"),
        new("rhx-min", "string", "-", "Earliest time, HH:mm"),
        new("rhx-max", "string", "-", "Latest time, HH:mm"),
        new("rhx-step", "int", "30", "Minutes between options"),
        new("rhx-12hour", "bool", "true", "12-hour (9:30 AM) vs 24-hour (09:30) display"),
        new("rhx-format", "string", "-", ".NET time format string for display (overrides 12/24h)"),
        new("rhx-size", "string", "medium", "small, medium, large"),
        new("rhx-disabled", "bool", "false", "Disable the control"),
    };

    public string BasicCode => "<rhx-time-picker name=\"StartTime\" rhx-placeholder=\"Pick a time…\" rhx-step=\"30\" />";
    public string RangeCode => "<rhx-time-picker name=\"slot\" rhx-min=\"09:00\" rhx-max=\"17:00\" rhx-step=\"15\" rhx-12hour=\"false\" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/TimePicker"),
            new("Time Picker"),
        };
    }
}
