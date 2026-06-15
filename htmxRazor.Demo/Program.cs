using System.Collections.Concurrent;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using htmxRazor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddhtmxRazor(options =>
{
    options.DefaultTheme = "light";
    options.IncludeHtmxScript = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UsehtmxRazor();
app.UseRouting();
app.MapRazorPages();

// ── Server-Sent Events endpoints for the <rhx-signalr> demo (htmx SSE extension) ──
// Replaces the old SignalR hub: server→client push with zero custom client JavaScript.

// Server clock: pushes a timestamp fragment as a named "ServerClock" SSE event.
app.MapGet("/demo-sse/clock", (CancellationToken ct) =>
    TypedResults.ServerSentEvents(ClockStream(ct), eventType: "ServerClock"));

// Chat: subscribers receive broadcast "ReceiveMessage" events; senders POST to /chat-send.
app.MapGet("/demo-sse/chat", (CancellationToken ct) =>
    TypedResults.ServerSentEvents(ChatStream(ct), eventType: "ReceiveMessage"));

app.MapPost("/demo-sse/chat-send", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var user = form["user"].ToString();
    if (string.IsNullOrWhiteSpace(user)) user = "Guest";
    var message = form["message"].ToString();
    if (!string.IsNullOrWhiteSpace(message))
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        var html =
            "<div style=\"padding: var(--rhx-space-xs) 0; border-bottom: 1px solid var(--rhx-color-border-muted);\">"
            + $"<strong>{WebUtility.HtmlEncode(user)}</strong> "
            + $"<span style=\"color: var(--rhx-color-text-muted); font-size: var(--rhx-font-size-xs);\">{time}</span>"
            + $"<div>{WebUtility.HtmlEncode(message)}</div></div>";
        ChatBroadcast.Publish(html);
    }
    return Results.NoContent();
});

app.Run();

static async IAsyncEnumerable<string> ClockStream([EnumeratorCancellation] CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        var time = DateTime.Now.ToString("HH:mm:ss.ff");
        yield return $"<span style=\"font-family: var(--rhx-font-family-mono); font-size: var(--rhx-font-size-lg);\">{time}</span>";
        try { await Task.Delay(1000, ct); }
        catch (TaskCanceledException) { yield break; }
    }
}

static async IAsyncEnumerable<string> ChatStream([EnumeratorCancellation] CancellationToken ct)
{
    var (id, reader) = ChatBroadcast.Subscribe();
    try
    {
        await foreach (var msg in reader.ReadAllAsync(ct))
            yield return msg;
    }
    finally
    {
        ChatBroadcast.Unsubscribe(id);
    }
}

/// <summary>In-memory fan-out for the SSE chat demo (one channel per connected subscriber).</summary>
static class ChatBroadcast
{
    private static readonly ConcurrentDictionary<Guid, Channel<string>> Subscribers = new();

    public static (Guid Id, ChannelReader<string> Reader) Subscribe()
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<string>();
        Subscribers[id] = channel;
        return (id, channel.Reader);
    }

    public static void Unsubscribe(Guid id) => Subscribers.TryRemove(id, out _);

    public static void Publish(string html)
    {
        foreach (var channel in Subscribers.Values)
            channel.Writer.TryWrite(html);
    }
}

public partial class Program;
