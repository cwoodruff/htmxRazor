using Microsoft.AspNetCore.Mvc.RazorPages;
using htmxRazor.Components.Navigation;
using htmxRazor.Demo.Models;

namespace htmxRazor.Demo.Pages.Docs.Components;

public class SignalRModel : PageModel
{
    public List<ComponentProperty> Properties { get; } = new()
    {
        new("hub-url", "string", "-", "The SignalR hub URL to connect to"),
        new("page", "string", "-", "Razor Page path for hub URL generation"),
        new("page-handler", "string", "-", "Page handler for hub URL generation"),
        new("rhx-method", "string", "-", "Hub method name to listen for"),
        new("rhx-swap", "string", "innerHTML", "How received HTML is swapped: innerHTML, beforeend, outerHTML, etc."),
        new("rhx-target", "string", "self", "CSS selector for the swap target element"),
        new("rhx-reconnect", "bool", "true", "Automatically reconnect on connection loss"),
        new("rhx-transport", "string", "auto", "Force transport: websockets, sse, or longpolling"),
        new("rhx-groups", "string", "-", "Comma-separated group names to join on connect"),
        new("rhx-connection-state", "bool", "false", "Show visual connection state indicator"),
    };

    public string ClockCode => @"<!-- Server pushes a timestamp every 2 seconds -->
<rhx-signalr hub-url=""/demoHub"" rhx-method=""ServerClock""
             rhx-connection-state=""true"">
    <rhx-spinner rhx-size=""small"" />
</rhx-signalr>";

    public string ChatCode => @"<!-- Messages appended as they arrive -->
<rhx-signalr hub-url=""/demoHub"" rhx-method=""ReceiveMessage""
             rhx-swap=""beforeend"" rhx-target=""#chat-messages""
             rhx-connection-state=""true"">
</rhx-signalr>
<div id=""chat-messages""></div>";

    public string HubCode => @"public class ChatHub : Hub
{
    public Task JoinGroup(string groupName)
        => Groups.AddToGroupAsync(Context.ConnectionId, groupName);

    public async Task SendMessage(string user, string message)
    {
        var html = $""<div><strong>{user}</strong>: {message}</div>"";
        await Clients.All.SendAsync(""ReceiveMessage"", html);
    }
}";

    public string ExtensionsCode => @"// From a controller or background service via IHubContext:
await hubContext.SendHtmlAsync(""ReceiveMessage"",
    ""<div>Broadcast to everyone</div>"");

await hubContext.SendHtmlToGroupAsync(""room1"",
    ""ReceiveMessage"", ""<div>Group-only message</div>"");

await hubContext.SendHtmlToConnectionAsync(connectionId,
    ""ReceiveMessage"", ""<div>Private message</div>"");";

    public string SetupCode => @"// Program.cs
builder.Services.AddSignalR();

var app = builder.Build();
app.MapHub<ChatHub>(""/chatHub"");";

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
