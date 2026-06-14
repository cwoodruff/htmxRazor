using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Navigation;

/// <summary>
/// Renders a tab panel inside an <c>&lt;rhx-tab-group&gt;</c>. The panel is linked
/// to its associated tab via <c>rhx-name</c>, which generates matching IDs:
/// <c>id="panel-{name}"</c> with <c>aria-labelledby="tab-{name}"</c>.
/// </summary>
/// <remarks>
/// <para>
/// Inactive panels are hidden with the <c>hidden</c> attribute. JavaScript manages
/// visibility when tabs are activated. Panels support htmx attributes for lazy-loading
/// content when the associated tab is first activated.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-tab-panel rhx-name="general" rhx-active&gt;
///     &lt;p&gt;General settings content here.&lt;/p&gt;
/// &lt;/rhx-tab-panel&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-tab-panel", ParentTag = "rhx-tab-group")]
public class TabPanelTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "tab-panel";

    /// <summary>
    /// The name of this panel. Used to generate the panel's id (<c>panel-{name}</c>)
    /// and link back to the tab via <c>aria-labelledby="tab-{name}"</c>.
    /// Must match the <c>rhx-panel</c> value of the corresponding <c>&lt;rhx-tab&gt;</c>.
    /// </summary>
    [HtmlAttributeName("rhx-name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Whether this panel is initially visible (its associated tab is active).
    /// </summary>
    [HtmlAttributeName("rhx-active")]
    public bool Active { get; set; }

    /// <summary>
    /// Creates a new TabPanelTagHelper with URL generation support.
    /// </summary>
    public TabPanelTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var panelId = $"panel-{Name}";

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass("active"), Active);

        if (!string.IsNullOrWhiteSpace(CssClass))
            css.Add(CssClass);

        output.Attributes.SetAttribute("class", css.Build());
        output.Attributes.SetAttribute("id", panelId);
        output.Attributes.SetAttribute("role", "tabpanel");
        output.Attributes.SetAttribute("tabindex", "0");

        // Visibility is controlled by the group's generated <style> (CSS :has() on the
        // checked tab radio). No `hidden` attribute and no JS.

        RenderHtmxAttributes(output);
    }
}
