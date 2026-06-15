using System.Net;
using System.Text;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Theming;

/// <summary>
/// A zero-JavaScript dark-mode toggle. Renders a visually-hidden checkbox
/// (<c>.rhx-theme-controller</c>) inside a styled <c>&lt;label&gt;</c>; the dark token overrides
/// apply via CSS <c>html:has(.rhx-theme-controller:checked)</c>, so toggling is instant with no
/// script. The choice is persisted in an <c>rhx-theme</c> cookie via an htmx POST to
/// <c>/_rhx/theme</c>, and the checkbox's initial <c>checked</c> state is rendered from that
/// cookie server-side (no flash of the wrong theme).
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-theme-toggle /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-theme-toggle")]
public class ThemeToggleTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "theme-toggle";

    /// <summary>Accessible label for the toggle. Default: "Dark mode".</summary>
    [HtmlAttributeName("aria-label")]
    public string AriaLabel { get; set; } = "Dark mode";

    /// <summary>Creates a new <see cref="ThemeToggleTagHelper"/> instance.</summary>
    public ThemeToggleTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // The cookie wins; with no cookie yet, fall back to the configured DefaultTheme.
        bool isDark;
        if (ViewContext?.HttpContext?.Request.Cookies.TryGetValue("rhx-theme", out var t) == true)
        {
            isDark = string.Equals(t, "dark", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            var options = ViewContext?.HttpContext?.RequestServices?
                .GetService(typeof(Configuration.htmxRazorOptions)) as Configuration.htmxRazorOptions;
            isDark = string.Equals(options?.DefaultTheme, "dark", StringComparison.OrdinalIgnoreCase);
        }

        output.TagName = "label";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder();
        ApplyBaseAttributes(output, css);
        output.Attributes.SetAttribute("title", AriaLabel);

        var sb = new StringBuilder();
        // The sr-only checkbox is the single source of truth; CSS :has() applies the dark tokens,
        // and the htmx POST persists the choice to the rhx-theme cookie.
        sb.Append("<input type=\"checkbox\" class=\"rhx-theme-controller rhx-sr-only\"");
        if (isDark) sb.Append(" checked");
        sb.Append(" hx-post=\"/_rhx/theme\" hx-trigger=\"change\" hx-swap=\"none\"");
        sb.Append($" aria-label=\"{Enc(AriaLabel)}\" />");

        // Visual switch (sun/moon), purely decorative.
        sb.Append($"<span class=\"{GetElementClass("track")}\" aria-hidden=\"true\">");
        sb.Append($"<span class=\"{GetElementClass("thumb")}\"></span>");
        sb.Append("</span>");

        output.Content.SetHtmlContent(sb.ToString());
    }

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? "") ?? "";
}
