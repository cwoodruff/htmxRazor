using Microsoft.AspNetCore.SignalR;

namespace htmxRazor.Infrastructure;

/// <summary>
/// Extension methods for <see cref="IHubContext{THub}"/> to send HTML fragments
/// to connected clients for use with the <c>&lt;rhx-signalr&gt;</c> Tag Helper.
/// </summary>
public static class HtmxSignalRExtensions
{
    /// <summary>
    /// Sends an HTML fragment to all connected clients on the specified method.
    /// </summary>
    /// <typeparam name="THub">The hub type.</typeparam>
    /// <param name="hubContext">The hub context.</param>
    /// <param name="method">The client method to invoke (matches <c>rhx-method</c>).</param>
    /// <param name="htmlContent">The HTML content to send.</param>
    public static Task SendHtmlAsync<THub>(
        this IHubContext<THub> hubContext,
        string method,
        string htmlContent) where THub : Hub
    {
        return hubContext.Clients.All.SendAsync(method, htmlContent);
    }

    /// <summary>
    /// Sends an HTML fragment to all clients in the specified group.
    /// </summary>
    /// <typeparam name="THub">The hub type.</typeparam>
    /// <param name="hubContext">The hub context.</param>
    /// <param name="groupName">The SignalR group name.</param>
    /// <param name="method">The client method to invoke.</param>
    /// <param name="htmlContent">The HTML content to send.</param>
    public static Task SendHtmlToGroupAsync<THub>(
        this IHubContext<THub> hubContext,
        string groupName,
        string method,
        string htmlContent) where THub : Hub
    {
        return hubContext.Clients.Group(groupName).SendAsync(method, htmlContent);
    }

    /// <summary>
    /// Sends an HTML fragment to a specific connection.
    /// </summary>
    /// <typeparam name="THub">The hub type.</typeparam>
    /// <param name="hubContext">The hub context.</param>
    /// <param name="connectionId">The target connection ID.</param>
    /// <param name="method">The client method to invoke.</param>
    /// <param name="htmlContent">The HTML content to send.</param>
    public static Task SendHtmlToConnectionAsync<THub>(
        this IHubContext<THub> hubContext,
        string connectionId,
        string method,
        string htmlContent) where THub : Hub
    {
        return hubContext.Clients.Client(connectionId).SendAsync(method, htmlContent);
    }

    /// <summary>
    /// Sends an HTML fragment to all clients except the specified connections.
    /// </summary>
    /// <typeparam name="THub">The hub type.</typeparam>
    /// <param name="hubContext">The hub context.</param>
    /// <param name="excludedConnectionIds">Connection IDs to exclude.</param>
    /// <param name="method">The client method to invoke.</param>
    /// <param name="htmlContent">The HTML content to send.</param>
    public static Task SendHtmlToOthersAsync<THub>(
        this IHubContext<THub> hubContext,
        IReadOnlyList<string> excludedConnectionIds,
        string method,
        string htmlContent) where THub : Hub
    {
        return hubContext.Clients.AllExcept(excludedConnectionIds).SendAsync(method, htmlContent);
    }
}
