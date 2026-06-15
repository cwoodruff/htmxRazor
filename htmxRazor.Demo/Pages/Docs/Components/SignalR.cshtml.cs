using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class SignalRModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("hub-url", "string", "-", "The SSE endpoint URL to connect to (sse-connect)"),
        new("page", "string", "-", "Razor Page path for endpoint URL generation"),
        new("page-handler", "string", "-", "Page handler for endpoint URL generation"),
        new("rhx-method", "string", "-", "Named SSE event to swap from (sse-swap)"),
        new("rhx-swap", "string", "innerHTML", "How received HTML is swapped: innerHTML, beforeend, outerHTML, etc."),
        new("rhx-target", "string", "self", "CSS selector for the swap target element"),
        new("rhx-connection-state", "bool", "false", "Adds the --has-state modifier for styling a state indicator"),
    };

    public string ClockCode => @"<!-- Listens for the ""ServerClock"" SSE event and replaces its content -->
<rhx-signalr hub-url=""/demo-sse/clock"" rhx-method=""ServerClock"">
    <rhx-spinner rhx-size=""small"" />
</rhx-signalr>";

    public string ChatCode => @"<!-- Appends each broadcast ""ReceiveMessage"" event -->
<rhx-signalr hub-url=""/demo-sse/chat"" rhx-method=""ReceiveMessage""
             rhx-swap=""beforeend"" rhx-target=""#chat-messages"">
</rhx-signalr>
<div id=""chat-messages""></div>

<!-- Sending is a plain htmx POST; the server broadcasts over SSE -->
<form hx-post=""/demo-sse/chat-send"" hx-swap=""none"">
    <input name=""message"" />
    <button type=""submit"">Send</button>
</form>";

    public string HubCode => @"// Minimal-API SSE endpoint (.NET 10) — named events, no SignalR.
app.MapGet(""/events"", (CancellationToken ct) =>
    TypedResults.ServerSentEvents(Stream(ct), eventType: ""ServerClock""));

static async IAsyncEnumerable<string> Stream(
    [EnumeratorCancellation] CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        yield return $""<span>{DateTime.Now:HH:mm:ss}</span>"";
        await Task.Delay(1000, ct);
    }
}";

    public string SetupCode => @"<!-- Load the htmx SSE extension once (the htmx ecosystem; no custom JS) -->
<script src=""https://unpkg.com/htmx-ext-sse@@2.2.2/sse.js"" defer></script>";

    public void OnGet()
    {
        ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Patterns", "/Patterns"),
            new("SignalR")
        };
    }
}
