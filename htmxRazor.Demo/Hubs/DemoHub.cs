using Microsoft.AspNetCore.SignalR;

namespace htmxRazor.Demo.Hubs;

/// <summary>
/// Demo SignalR hub for the <c>&lt;rhx-signalr&gt;</c> component documentation.
/// </summary>
public class DemoHub : Hub
{
    /// <summary>
    /// Joins the caller to a named group.
    /// Required for group-based messaging with <c>rhx-groups</c>.
    /// </summary>
    public Task JoinGroup(string groupName)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Sends a chat message to all connected clients as an HTML fragment.
    /// </summary>
    public async Task SendMessage(string user, string message)
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        var html = $"<div style=\"padding: var(--rhx-space-xs) 0; border-bottom: 1px solid var(--rhx-color-border-muted);\">"
                 + $"<strong>{System.Net.WebUtility.HtmlEncode(user)}</strong> "
                 + $"<span style=\"color: var(--rhx-color-text-muted); font-size: var(--rhx-font-size-xs);\">{time}</span>"
                 + $"<div>{System.Net.WebUtility.HtmlEncode(message)}</div></div>";
        await Clients.All.SendAsync("ReceiveMessage", html);
    }
}

/// <summary>
/// Background service that sends a server timestamp to all connected clients every 2 seconds.
/// Used by the SignalR demo page's "Server Clock" example.
/// </summary>
public class DemoClockService : BackgroundService
{
    private readonly IHubContext<DemoHub> _hubContext;

    public DemoClockService(IHubContext<DemoHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(2000, stoppingToken);
            var time = DateTime.Now.ToString("HH:mm:ss.ff");
            var html = $"<span style=\"font-family: var(--rhx-font-family-mono); font-size: var(--rhx-font-size-lg);\">{time}</span>";
            await _hubContext.Clients.All.SendAsync("ServerClock", html, stoppingToken);
        }
    }
}
