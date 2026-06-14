using System.Net;
using System.Text;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Navigation;

/// <summary>
/// Renders a tab button inside an <c>&lt;rhx-tab-group&gt;</c>. The tab suppresses
/// its own output and registers a <c>&lt;button role="tab"&gt;</c> into the parent's
/// tablist nav. Links to its associated panel via <c>rhx-panel</c>.
/// </summary>
/// <remarks>
/// <para>
/// htmx attributes (hx-get, hx-target, etc.) are rendered directly on the button
/// element, enabling lazy-loaded tab panels. When a tab with hx-get is clicked,
/// the content is fetched and swapped into the target panel.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-tab rhx-panel="settings" hx-get="/Settings/Content"
///           hx-target="#panel-settings" hx-swap="innerHTML"&gt;
///     Settings
/// &lt;/rhx-tab&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-tab", ParentTag = "rhx-tab-group")]
public class TabTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "tab";

    /// <summary>
    /// The name of the associated panel. Used to generate matching IDs:
    /// the tab gets <c>id="tab-{panel}"</c> and links to <c>id="panel-{panel}"</c>.
    /// </summary>
    [HtmlAttributeName("rhx-panel")]
    public string Panel { get; set; } = "";

    /// <summary>
    /// Whether this tab is initially active/selected.
    /// </summary>
    [HtmlAttributeName("rhx-active")]
    public bool Active { get; set; }

    /// <summary>
    /// Whether this tab can be closed. Adds a close button (×) that removes
    /// the tab and its panel from the DOM.
    /// </summary>
    [HtmlAttributeName("rhx-closable")]
    public bool Closable { get; set; }

    /// <summary>
    /// Whether this tab is disabled. Disabled tabs cannot be activated.
    /// </summary>
    [HtmlAttributeName("rhx-disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Creates a new TabTagHelper with URL generation support.
    /// </summary>
    public TabTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContent = await output.GetChildContentAsync();

        // Get parent's shared state
        if (!context.Items.TryGetValue("RhxTabsNav", out var navObj) || navObj is not List<string> tabs)
        {
            output.TagName = null;
            return;
        }
        var groupName = context.Items.TryGetValue("RhxTabsGroupName", out var gn) ? gn as string : null;
        var counter = context.Items.TryGetValue("RhxTabsCounter", out var c) ? c as int[] : null;
        var panelMap = context.Items.TryGetValue("RhxTabsPanelMap", out var pm)
            ? pm as List<(string, string)> : null;
        if (groupName is null || counter is null || panelMap is null)
        {
            output.SuppressOutput();
            return;
        }

        // Per-tab unique radio id (multiple tabs may target the same panel — e.g. htmx filters).
        var index = counter[0]++;
        var radioId = $"{groupName}-t{index}";
        panelMap.Add((radioId, Panel));

        // Label CSS classes (the visible tab is the <label>).
        var css = new CssClassBuilder(GetBlockClass())
            .AddIf(GetModifierClass("active"), Active)
            .AddIf(GetModifierClass("disabled"), Disabled);
        if (!string.IsNullOrWhiteSpace(CssClass))
            css.Add(CssClass);

        // A native radio drives selection (keyboard + mutual exclusivity, no JS). htmx
        // attributes go on the radio so they fire on `change`.
        var sb = new StringBuilder();
        sb.Append($"<input type=\"radio\" class=\"{GetElementClass("radio")} rhx-sr-only\"");
        sb.Append($" name=\"{Enc(groupName)}\" id=\"{Enc(radioId)}\" value=\"{Enc(Panel)}\"");
        if (Active) sb.Append(" checked");
        if (Disabled) sb.Append(" disabled");
        sb.Append(BuildHtmxAttributeString());
        sb.Append(" />");

        sb.Append($"<label class=\"{css.Build()}\" for=\"{Enc(radioId)}\">");
        sb.Append($"<span class=\"{GetElementClass("label")}\">");
        sb.Append(childContent.GetContent());
        sb.Append("</span>");
        sb.Append("</label>");

        tabs.Add(sb.ToString());
        output.SuppressOutput();
    }

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? "") ?? "";

    private string BuildHtmxAttributeString()
    {
        var sb = new StringBuilder();

        void VerbAttr(string name, string? value)
        {
            if (value == null) return;
            if (value.Length == 0)
            {
                var url = GenerateRouteUrl();
                if (!string.IsNullOrWhiteSpace(url))
                    sb.Append($" {name}=\"{Enc(url)}\"");
            }
            else
            {
                sb.Append($" {name}=\"{Enc(value)}\"");
            }
        }

        void Attr(string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                sb.Append($" {name}=\"{Enc(value)}\"");
        }

        VerbAttr("hx-get", HxGet);
        VerbAttr("hx-post", HxPost);
        VerbAttr("hx-put", HxPut);
        VerbAttr("hx-patch", HxPatch);
        VerbAttr("hx-delete", HxDelete);
        Attr("hx-target", HxTarget);
        Attr("hx-swap", HxSwap);
        Attr("hx-trigger", HxTrigger);
        Attr("hx-indicator", HxIndicator);
        Attr("hx-confirm", HxConfirm);
        Attr("hx-push-url", HxPushUrl);
        Attr("hx-boost", HxBoost);
        Attr("hx-vals", HxVals);
        Attr("hx-headers", HxHeaders);
        Attr("hx-disabled-elt", HxDisabledElt);
        Attr("hx-encoding", HxEncoding);
        Attr("hx-ext", HxExt);
        Attr("hx-include", HxInclude);
        Attr("hx-params", HxParams);
        Attr("hx-select", HxSelect);
        Attr("hx-select-oob", HxSelectOob);
        Attr("hx-swap-oob", HxSwapOob);
        Attr("hx-sync", HxSync);

        return sb.ToString();
    }
}
