using htmxRazor.Infrastructure;
using htmxRazor.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Actions;

/// <summary>
/// Renders a dropdown menu container. Uses a slot-based trigger mechanism:
/// the <c>&lt;rhx-dropdown-trigger&gt;</c> child provides the trigger button,
/// and <c>&lt;rhx-dropdown-item&gt;</c> children populate the menu panel.
/// </summary>
/// <remarks>
/// <para>
/// The component is JavaScript-free. Open/close, light-dismiss (click outside) and
/// Escape are provided by the native
/// <see href="https://developer.mozilla.org/docs/Web/API/Popover_API">Popover API</see>:
/// the trigger button carries <c>popovertarget</c>/<c>popovertargetaction="toggle"</c>
/// and the menu carries the <c>popover</c> attribute. Placement uses
/// <see href="https://developer.mozilla.org/docs/Web/CSS/anchor-name">CSS Anchor Positioning</see>
/// (the trigger sets <c>anchor-name</c>, the menu sets <c>position-anchor</c> +
/// <c>position-area</c>), with a stacked-below fallback when anchor positioning is unsupported.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-dropdown&gt;
///     &lt;rhx-dropdown-trigger&gt;
///         &lt;rhx-button rhx-variant="brand"&gt;Actions &amp;#9662;&lt;/rhx-button&gt;
///     &lt;/rhx-dropdown-trigger&gt;
///     &lt;rhx-dropdown-item&gt;Edit&lt;/rhx-dropdown-item&gt;
///     &lt;rhx-dropdown-divider /&gt;
///     &lt;rhx-dropdown-item&gt;Delete&lt;/rhx-dropdown-item&gt;
/// &lt;/rhx-dropdown&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-dropdown")]
public class DropdownTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "dropdown";

    /// <summary>
    /// Whether the dropdown panel is initially open. When true the menu renders with the
    /// <c>open</c> attribute on its popover so it is shown on load (manual popover).
    /// </summary>
    [HtmlAttributeName("rhx-open")]
    public bool Open { get; set; }

    /// <summary>
    /// The preferred placement of the dropdown panel relative to the trigger.
    /// Options: bottom-start (default), bottom-end, bottom, top-start, top-end, top.
    /// Maps to a CSS <c>position-area</c>; the browser flips automatically via
    /// <c>position-try-fallbacks</c> when the panel would overflow the viewport.
    /// </summary>
    [HtmlAttributeName("rhx-placement")]
    public string Placement { get; set; } = "bottom-start";

    /// <summary>
    /// Whether the dropdown is disabled. Dims the trigger and prevents opening.
    /// </summary>
    [HtmlAttributeName("rhx-disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Accessible label for the dropdown menu panel.
    /// </summary>
    [HtmlAttributeName("aria-label")]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Creates a new DropdownTagHelper with URL generation support.
    /// </summary>
    public DropdownTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <summary>
    /// Maps an <c>rhx-placement</c> value to a CSS <c>position-area</c> value.
    /// </summary>
    private static string PlacementToPositionArea(string placement) => placement switch
    {
        "bottom-end" => "bottom span-left",
        "bottom" => "bottom",
        "top-start" => "top span-right",
        "top-end" => "top span-left",
        "top" => "top",
        _ => "bottom span-right", // bottom-start (default)
    };

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // ── Set up slot renderer and context ──
        var slots = SlotRenderer.CreateForContext(context);
        context.Items[typeof(DropdownTagHelper)] = this;

        // ── Generate unique panel ID + anchor name ──
        var panelId = !string.IsNullOrWhiteSpace(Id)
            ? $"{Id}-panel"
            : $"rhx-dropdown-{context.UniqueId}";
        var anchorName = $"--rhx-dropdown-anchor-{context.UniqueId}";

        context.Items["DropdownPanelId"] = panelId;
        context.Items["DropdownAnchorName"] = anchorName;

        var placement = Placement.ToLowerInvariant();

        // ── Process children (populates trigger slot + item/divider content) ──
        var childContent = await output.GetChildContentAsync();

        // ── Render outer container ──
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass("disabled"), Disabled);
        ApplyBaseAttributes(output, css);

        // Placement is exposed for styling/testing hooks (no JS reads it).
        output.Attributes.SetAttribute("data-rhx-placement", placement);

        // ── htmx (rare on container but supported) ──
        RenderHtmxAttributes(output);

        // ── Assemble inner HTML ──
        output.Content.Clear();

        // Trigger from slot (carries popovertarget + anchor-name)
        if (slots.Has("trigger"))
        {
            output.Content.AppendHtml(slots.Get("trigger")!);
        }

        // ── Menu (popover) ──
        // An "auto" popover gives native light-dismiss (outside click) + Escape, opened by the
        // trigger's popovertarget invoker. NOTE: a popover cannot be shown on initial load
        // declaratively (the `open` attribute applies to <dialog>/<details>, not popovers), so
        // there is no JS-free "initially open" dropdown — `rhx-open` is accepted but no-ops.
        var positionArea = PlacementToPositionArea(placement);

        var menuStyle =
            $"position-anchor:{anchorName};position-area:{positionArea};";

        var menu = new System.Text.StringBuilder();
        menu.Append($"<div class=\"{GetElementClass("panel")}\"");
        menu.Append($" id=\"{panelId}\"");
        menu.Append(" popover=\"auto\"");
        menu.Append(" role=\"menu\"");
        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            menu.Append($" aria-label=\"{AriaLabel}\"");
        }
        menu.Append($" data-rhx-placement=\"{placement}\"");
        menu.Append($" style=\"{menuStyle}\"");
        menu.Append('>');

        output.Content.AppendHtml(menu.ToString());
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</div>");
    }
}
