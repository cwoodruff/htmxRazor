using System.Net;
using System.Text;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Navigation;

/// <summary>
/// Renders a tabbed interface container. Contains <c>&lt;rhx-tab&gt;</c> children
/// (which register themselves into the tab nav) and <c>&lt;rhx-tab-panel&gt;</c>
/// children (which render into the body area).
/// </summary>
/// <remarks>
/// <para>
/// JavaScript (<c>rhx-tabs.js</c>) handles click activation, keyboard navigation
/// (arrow keys, Home/End), ARIA management, and auto/manual activation modes.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-tab-group aria-label="Settings"&gt;
///     &lt;rhx-tab rhx-panel="general" rhx-active&gt;General&lt;/rhx-tab&gt;
///     &lt;rhx-tab rhx-panel="advanced"&gt;Advanced&lt;/rhx-tab&gt;
///     &lt;rhx-tab-panel rhx-name="general" rhx-active&gt;General content&lt;/rhx-tab-panel&gt;
///     &lt;rhx-tab-panel rhx-name="advanced"&gt;Advanced content&lt;/rhx-tab-panel&gt;
/// &lt;/rhx-tab-group&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-tab-group")]
public class TabGroupTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "tab-group";

    /// <summary>
    /// The placement of the tab nav relative to the panel body.
    /// Options: top (default), bottom, start, end.
    /// </summary>
    [HtmlAttributeName("rhx-placement")]
    public string Placement { get; set; } = "top";

    /// <summary>
    /// The activation mode for tabs.
    /// "auto" (default): tabs activate on focus (arrow key navigation selects).
    /// "manual": tabs activate only on Enter/Space (arrow keys move focus only).
    /// </summary>
    [HtmlAttributeName("rhx-activation")]
    public string Activation { get; set; } = "auto";

    /// <summary>
    /// Accessible label for the tablist navigation region.
    /// </summary>
    [HtmlAttributeName("aria-label")]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Creates a new TabGroupTagHelper with URL generation support.
    /// </summary>
    public TabGroupTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // A per-group radio name keeps tabs mutually exclusive (one checked at a time).
        var groupName = "rhx-tabs-" + context.UniqueId;
        var groupId = string.IsNullOrWhiteSpace(Id) ? groupName : Id!;

        // Shared state for the child tabs.
        var tabs = new List<string>();                              // rendered radio+label HTML
        var panelMap = new List<(string RadioId, string Panel)>();  // for the show/hide <style>
        context.Items["RhxTabsNav"] = tabs;
        context.Items["RhxTabsGroupName"] = groupName;
        context.Items["RhxTabsPanelMap"] = panelMap;
        context.Items["RhxTabsCounter"] = new int[1];
        context.Items[typeof(TabGroupTagHelper)] = this;

        // Process children — tabs register their radio+label HTML, panels render normally
        var childContent = await output.GetChildContentAsync();

        // Render outer container
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var placement = Placement.ToLowerInvariant();

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(placement), placement != "top");
        ApplyBaseAttributes(output, css);
        output.Attributes.SetAttribute("id", groupId);

        RenderHtmxAttributes(output);

        // Assemble inner content
        output.Content.Clear();

        // Nav — a radiogroup of tab labels (native radios drive selection; no JS).
        var ariaLabelAttr = !string.IsNullOrWhiteSpace(AriaLabel)
            ? $" aria-label=\"{WebUtility.HtmlEncode(AriaLabel)}\""
            : "";

        output.Content.AppendHtml(
            $"<div class=\"{GetElementClass("nav")}\" role=\"tablist\"{ariaLabelAttr}>");

        foreach (var tab in tabs)
        {
            output.Content.AppendHtml(tab);
        }

        output.Content.AppendHtml("</div>");

        // Body
        output.Content.AppendHtml($"<div class=\"{GetElementClass("body")}\">");
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</div>");

        // Per-group <style>: hide all panels, show the one whose radio is checked. Pure CSS.
        var styleSb = new StringBuilder();
        styleSb.Append("<style>");
        styleSb.Append($"#{groupId} > .{GetElementClass("body")} > .rhx-tab-panel{{display:none}}");
        foreach (var (radioId, panel) in panelMap)
        {
            styleSb.Append(
                $"#{groupId}:has(#{radioId}:checked) #panel-{WebUtility.HtmlEncode(panel)}{{display:block}}");
        }
        styleSb.Append("</style>");
        output.Content.AppendHtml(styleSb.ToString());
    }
}
