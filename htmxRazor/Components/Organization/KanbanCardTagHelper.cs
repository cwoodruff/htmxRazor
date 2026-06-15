using System.Net;
using System.Text;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Organization;

/// <summary>
/// A card within a <c>&lt;rhx-kanban-column&gt;</c>. Cards move between columns via htmx
/// "move" buttons (◀ / ▶) — no JavaScript and no HTML drag-and-drop. Each button fires the
/// card's configured htmx request with <c>hx-vals</c> carrying the card id and a direction
/// (<c>prev</c>/<c>next</c>); the server resolves the adjacent column and re-renders the board.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-kanban-card rhx-card-id="task-1" rhx-variant="brand"
///                  hx-post="/Board?handler=Move"
///                  hx-target="#board" hx-swap="innerHTML"&gt;
///     Design the homepage
/// &lt;/rhx-kanban-card&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-kanban-card")]
public class KanbanCardTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "kanban-card";

    /// <summary>Unique identifier for this card. Sent with the move request as <c>cardId</c>.</summary>
    [HtmlAttributeName("rhx-card-id")]
    public string CardId { get; set; } = "";

    /// <summary>
    /// Whether to render the move buttons. Default: true. (Retained name for source
    /// compatibility; HTML drag-and-drop is replaced by htmx move buttons.)
    /// </summary>
    [HtmlAttributeName("rhx-draggable")]
    public bool Draggable { get; set; } = true;

    /// <summary>
    /// Optional color variant for visual categorization: brand, success, warning, danger.
    /// </summary>
    [HtmlAttributeName("rhx-variant")]
    public string? Variant { get; set; }

    /// <summary>
    /// Creates a new <see cref="KanbanCardTagHelper"/> instance.
    /// </summary>
    public KanbanCardTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(Variant!), !string.IsNullOrWhiteSpace(Variant));
        ApplyBaseAttributes(output, css);

        output.Attributes.SetAttribute("data-rhx-card-id", CardId);

        var childContent = await output.GetChildContentAsync();

        output.Content.Clear();
        output.Content.AppendHtml($"<div class=\"{GetElementClass("content")}\">");
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</div>");

        // Move buttons (htmx, zero JS) — only when a request verb is configured.
        var (verbAttr, verbVal) = ResolveVerb();
        if (Draggable && verbAttr != null && !string.IsNullOrWhiteSpace(verbVal))
        {
            output.Content.AppendHtml($"<div class=\"{GetElementClass("actions")}\">");
            output.Content.AppendHtml(MoveButton(verbAttr, verbVal!, "prev", "Move to previous column", "◀"));
            output.Content.AppendHtml(MoveButton(verbAttr, verbVal!, "next", "Move to next column", "▶"));
            output.Content.AppendHtml("</div>");
        }
    }

    private (string? attr, string? value) ResolveVerb()
    {
        if (HxPost != null) return ("hx-post", HxPost.Length == 0 ? GenerateRouteUrl() : HxPost);
        if (HxPut != null) return ("hx-put", HxPut.Length == 0 ? GenerateRouteUrl() : HxPut);
        if (HxPatch != null) return ("hx-patch", HxPatch.Length == 0 ? GenerateRouteUrl() : HxPatch);
        if (HxDelete != null) return ("hx-delete", HxDelete.Length == 0 ? GenerateRouteUrl() : HxDelete);
        if (HxGet != null) return ("hx-get", HxGet.Length == 0 ? GenerateRouteUrl() : HxGet);
        return (null, null);
    }

    private string MoveButton(string verbAttr, string verbVal, string direction, string label, string glyph)
    {
        var sb = new StringBuilder();
        sb.Append($"<button type=\"button\" class=\"{GetElementClass("move")}\"");
        sb.Append($" {verbAttr}=\"{Enc(verbVal)}\"");
        if (!string.IsNullOrWhiteSpace(HxTarget)) sb.Append($" hx-target=\"{Enc(HxTarget)}\"");
        if (!string.IsNullOrWhiteSpace(HxSwap)) sb.Append($" hx-swap=\"{Enc(HxSwap)}\"");
        if (!string.IsNullOrWhiteSpace(HxConfirm)) sb.Append($" hx-confirm=\"{Enc(HxConfirm)}\"");
        if (!string.IsNullOrWhiteSpace(HxIndicator)) sb.Append($" hx-indicator=\"{Enc(HxIndicator)}\"");
        // Single-quoted attribute so the JSON's double quotes are literal.
        sb.Append($" hx-vals='{{\"cardId\":\"{Enc(CardId)}\",\"direction\":\"{direction}\"}}'");
        sb.Append($" aria-label=\"{Enc(label)}\">{glyph}</button>");
        return sb.ToString();
    }

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? "") ?? "";
}
