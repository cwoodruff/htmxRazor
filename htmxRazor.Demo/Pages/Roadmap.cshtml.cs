using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmxRazor.Demo.Pages;

public class RoadmapModel : PageModel
{
    public List<ReleaseInfo> Planned { get; } = new()
    {
        new("v2.1", "Advanced Inputs", "milestone/1", new[]
        {
            "Radial Select — pie category picker (rectangular trigger + circular SVG wedges) that drives a cascading htmx-loaded dropdown.",
            "Date / Time Picker family — the most-requested missing input, with model binding and validation.",
            "Playwright E2E re-enabled and stabilized in CI.",
        }),
        new("v2.2", "Playground & DX Completion", "milestone/2", new[]
        {
            "Component Playground coverage for every Forms and Actions component.",
            "JetBrains plugin brought to parity with the 97-snippet VS Code extension.",
        }),
        new("v2.3", "Data & Visualization", "milestone/3", new[]
        {
            "SVG bar, line, and area charts built on the design-token system.",
            "Expandable and tree rows for the Data Table with htmx lazy expansion.",
            "Optional virtualized table body for large datasets.",
        }),
        new("v2.4", "Accessibility & Internationalization", "milestone/4", new[]
        {
            "WCAG 2.2 contrast audit of every token pair (light + dark).",
            "RTL support via logical CSS properties and mirrored affordances.",
            "Localizable built-in strings; infrastructure-cleanup backlog cleared.",
        }),
    };

    public List<ReleaseInfo> Shipped { get; } = new()
    {
        new("v2.0", "Platform & DX", "", new[]
        {
            "Theme Builder, Component Playground, SignalR connector, CSS Anchor Positioning, VS Code snippets, Kanban board.",
        }),
        new("v1.4", "Real-time, Wizard & Patterns", "", new[]
        {
            "SSE Stream, multi-step Wizard, Response-Aware Form, Timeline, Optimistic UI, Load More.",
        }),
        new("v1.3", "Data Table, Accessibility & Modern CSS", "", new[]
        {
            "Data Table, Command Palette, Skip Nav & Landmarks, focus management, container queries.",
        }),
        new("v1.2", "Notifications, Pagination & Quick Wins", "", new[]
        {
            "Toast system, Pagination, ARIA Live Region, CSS cascade layers, View Transitions.",
        }),
    };

    public void OnGet()
    {
    }

    public record ReleaseInfo(string Version, string Theme, string MilestonePath, string[] Items);
}
