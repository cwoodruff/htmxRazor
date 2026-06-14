using System.Net;
using htmxRazor.Infrastructure;
using htmxRazor.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Overlays;

/// <summary>
/// Renders a slide-out drawer panel from any edge of the viewport using the native
/// HTML <c>&lt;dialog&gt;</c> element. The browser provides the modal backdrop, focus
/// trapping, light-dismiss, and ESC-to-close with zero JavaScript.
/// </summary>
/// <remarks>
/// <para>
/// The drawer is a modal <c>&lt;dialog&gt;</c>. Open it with an invoker button using the
/// native command attributes: <c>&lt;button command="show-modal" commandfor="drawer-id"&gt;</c>.
/// Close it with a button inside the drawer using <c>command="close" commandfor="drawer-id"</c>
/// (or rely on the built-in header close button, backdrop click, or the Escape key).
/// </para>
/// <para>
/// Child tag helpers (<c>&lt;rhx-drawer-footer&gt;</c>) register content into slots.
/// Remaining child content becomes the drawer body.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-button command="show-modal" commandfor="nav-drawer"&gt;Open&lt;/rhx-button&gt;
/// &lt;rhx-drawer id="nav-drawer" rhx-label="Navigation" rhx-placement="start"&gt;
///     &lt;nav&gt;...&lt;/nav&gt;
///     &lt;rhx-drawer-footer&gt;
///         &lt;rhx-button command="close" commandfor="nav-drawer"&gt;Close&lt;/rhx-button&gt;
///     &lt;/rhx-drawer-footer&gt;
/// &lt;/rhx-drawer&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-drawer")]
public class DrawerTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "drawer";

    /// <summary>
    /// Whether the drawer is initially open. Renders the <c>open</c> attribute on the
    /// <c>&lt;dialog&gt;</c>. Note: a server-rendered <c>open</c> dialog is non-modal
    /// (no backdrop / focus trap) until reopened via the <c>show-modal</c> command.
    /// </summary>
    [HtmlAttributeName("rhx-open")]
    public bool Open { get; set; }

    /// <summary>
    /// The title text displayed in the drawer header.
    /// </summary>
    [HtmlAttributeName("rhx-label")]
    public string? Label { get; set; }

    /// <summary>
    /// The edge from which the drawer slides: start, end, top, bottom. Default: end.
    /// </summary>
    [HtmlAttributeName("rhx-placement")]
    public string Placement { get; set; } = "end";

    /// <summary>
    /// When true, the drawer is positioned relative to its parent element
    /// instead of the viewport.
    /// </summary>
    [HtmlAttributeName("rhx-contained")]
    public bool Contained { get; set; }

    /// <summary>
    /// When true, the default header with title and close button is not rendered.
    /// </summary>
    [HtmlAttributeName("rhx-no-header")]
    public bool NoHeader { get; set; }

    /// <summary>
    /// Creates a new DrawerTagHelper with URL generation support.
    /// </summary>
    public DrawerTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var slots = SlotRenderer.CreateForContext(context);
        var childContent = await output.GetChildContentAsync();

        // The drawer is a native modal <dialog>. The browser provides the backdrop,
        // focus trap, light-dismiss, and Escape-to-close — no JavaScript required.
        output.TagName = "dialog";
        output.TagMode = TagMode.StartTagAndEndTag;

        var placement = Placement.ToLowerInvariant();
        var css = CreateCssBuilder()
            .Add(GetModifierClass(placement))
            .AddIf(GetModifierClass("open"), Open)
            .AddIf(GetModifierClass("contained"), Contained);
        ApplyBaseAttributes(output, css);

        // Kept for CSS placement hooks and test/selectors.
        output.Attributes.SetAttribute("data-rhx-placement", placement);

        if (Open)
            output.Attributes.SetAttribute("open", "open");

        if (!string.IsNullOrWhiteSpace(Label))
            output.Attributes.SetAttribute("aria-label", Label);

        RenderHtmxAttributes(output);

        // Assemble inner HTML — the <dialog> itself is the panel; no overlay
        // element is needed (the native ::backdrop handles that).
        output.Content.Clear();

        // Header
        if (!NoHeader)
        {
            output.Content.AppendHtml($"<header class=\"{GetElementClass("header")}\">");
            if (!string.IsNullOrWhiteSpace(Label))
            {
                output.Content.AppendHtml(
                    $"<h2 class=\"{GetElementClass("title")}\">{Enc(Label)}</h2>");
            }

            // Native invoker command closes the owning dialog with no JavaScript.
            var commandfor = string.IsNullOrWhiteSpace(Id) ? "" : $" commandfor=\"{Enc(Id)}\"";
            output.Content.AppendHtml(
                $"<button class=\"{GetElementClass("close")}\" type=\"button\" " +
                $"command=\"close\"{commandfor} aria-label=\"Close\">" +
                "&times;</button>");
            output.Content.AppendHtml("</header>");
        }

        // Body
        output.Content.AppendHtml($"<div class=\"{GetElementClass("body")}\">");
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</div>");

        // Footer
        if (slots.Has("footer"))
        {
            output.Content.AppendHtml($"<footer class=\"{GetElementClass("footer")}\">");
            output.Content.AppendHtml(slots.Get("footer")!);
            output.Content.AppendHtml("</footer>");
        }
    }

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? "") ?? "";
}
