using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class DateRangePickerModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("rhx-start-name", "string", "-", "Form field name for the hidden start date (ISO yyyy-MM-dd)"),
        new("rhx-end-name", "string", "-", "Form field name for the hidden end date (ISO yyyy-MM-dd)"),
        new("rhx-start-value", "string", "-", "Initial start date (ISO yyyy-MM-dd)"),
        new("rhx-end-value", "string", "-", "Initial end date (ISO yyyy-MM-dd)"),
        new("rhx-presets", "string", "-", "Comma list: today, yesterday, last7, last30, thismonth, lastmonth"),
        new("rhx-min", "string", "-", "Earliest selectable date (ISO yyyy-MM-dd)"),
        new("rhx-max", "string", "-", "Latest selectable date (ISO yyyy-MM-dd)"),
        new("rhx-week-start", "string", "mon", "First day of the week: mon or sun"),
        new("rhx-format", "string", "-", ".NET date format for display (default: culture short date)"),
        new("rhx-size", "string", "medium", "small, medium, large"),
        new("rhx-disabled", "bool", "false", "Disable the control"),
    };

    public string BasicCode => "<rhx-date-range-picker rhx-start-name=\"From\" rhx-end-name=\"To\"\n                       rhx-presets=\"today,last7,thismonth,last30\" rhx-placeholder=\"Pick a range…\" />";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Components", "/Docs/Components/DateRangePicker"),
            new("Date Range Picker"),
        };
    }
}
