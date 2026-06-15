using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Patterns;

/// <summary>
/// Declarative wrapper for SignalR hub connections. Renders a container that
/// connects to a SignalR hub and swaps received HTML fragments into the DOM
/// using htmx's swap API.
/// </summary>
/// <remarks>
/// Requires the <c>@microsoft/signalr</c> client library to be loaded.
/// The component renders data attributes that the companion <c>rhx-signalr.js</c>
/// reads to establish connections, listen for methods, and swap content.
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-signalr hub-url="/chatHub" rhx-method="ReceiveMessage"
///              rhx-swap="beforeend" rhx-target="#messages"&gt;
///     &lt;rhx-spinner /&gt;
/// &lt;/rhx-signalr&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-signalr")]
public class SignalRTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "signalr";

    // ──────────────────────────────────────────────
    //  Connection properties
    // ──────────────────────────────────────────────

    /// <summary>
    /// The SignalR hub URL to connect to.
    /// </summary>
    [HtmlAttributeName("hub-url")]
    public string? HubUrl { get; set; }

    /// <summary>
    /// The Razor Page path for hub URL generation.
    /// </summary>
    [HtmlAttributeName("page")]
    public string? Page { get; set; }

    /// <summary>
    /// The page handler name for hub URL generation.
    /// </summary>
    [HtmlAttributeName("page-handler")]
    public string? PageHandler { get; set; }

    /// <summary>
    /// Route parameter values for URL generation. Bound from route-* attributes.
    /// </summary>
    [HtmlAttributeName("route-", DictionaryAttributePrefix = "route-")]
    public Dictionary<string, string> RouteValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // ──────────────────────────────────────────────
    //  Hub behavior properties
    // ──────────────────────────────────────────────

    /// <summary>
    /// The hub method name to listen for. The handler should send HTML content.
    /// </summary>
    [HtmlAttributeName("rhx-method")]
    public string? Method { get; set; }

    /// <summary>
    /// How the received HTML is swapped into the target. Default: "innerHTML".
    /// </summary>
    [HtmlAttributeName("rhx-swap")]
    public string SignalRSwap { get; set; } = "innerHTML";

    /// <summary>
    /// CSS selector for the swap target element. When omitted, the container itself is the target.
    /// </summary>
    [HtmlAttributeName("rhx-target")]
    public string? SignalRTarget { get; set; }

    /// <summary>
    /// Whether to automatically reconnect on connection loss. Default: true.
    /// </summary>
    [HtmlAttributeName("rhx-reconnect")]
    public bool Reconnect { get; set; } = true;

    /// <summary>
    /// Force a specific transport: "websockets", "sse", or "longpolling".
    /// When omitted, SignalR negotiates automatically.
    /// </summary>
    [HtmlAttributeName("rhx-transport")]
    public string? Transport { get; set; }

    /// <summary>
    /// Comma-separated SignalR group names to join on connection.
    /// Requires the hub to implement a JoinGroup method.
    /// </summary>
    [HtmlAttributeName("rhx-groups")]
    public string? Groups { get; set; }

    /// <summary>
    /// Whether to show a visual connection state indicator. Default: false.
    /// </summary>
    [HtmlAttributeName("rhx-connection-state")]
    public bool ConnectionState { get; set; }

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="SignalRTagHelper"/> instance.
    /// </summary>
    public SignalRTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    // ──────────────────────────────────────────────
    //  Rendering
    // ──────────────────────────────────────────────

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Bridge convenience properties to base class for URL generation
        HxPage = Page;
        HxHandler = PageHandler;
        foreach (var kv in RouteValues)
            HxRouteValues[kv.Key] = kv.Value;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass("has-state"), ConnectionState);
        ApplyBaseAttributes(output, css);

        // Wire the htmx SSE extension (server→client push). The sse.js extension does the work —
        // no custom JavaScript. SSE auto-reconnects natively, so rhx-reconnect/transport/groups
        // (SignalR-specific) are retained as no-ops for source compatibility.
        output.Attributes.SetAttribute("hx-ext", "sse");

        // Determine the SSE endpoint URL (hub-url or generated route).
        var url = HubUrl;
        if (string.IsNullOrWhiteSpace(url))
            url = GenerateRouteUrl();

        if (!string.IsNullOrWhiteSpace(url))
            output.Attributes.SetAttribute("sse-connect", url);

        // rhx-method is the named SSE event to swap from.
        if (!string.IsNullOrWhiteSpace(Method))
            output.Attributes.SetAttribute("sse-swap", Method);

        output.Attributes.SetAttribute("hx-swap", SignalRSwap);

        if (!string.IsNullOrWhiteSpace(SignalRTarget))
            output.Attributes.SetAttribute("hx-target", SignalRTarget);

        // Accessibility — real-time content announced to screen readers.
        output.Attributes.SetAttribute("aria-live", "polite");
        output.Attributes.SetAttribute("aria-atomic", "false");

        var childContent = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(childContent);
    }
}
