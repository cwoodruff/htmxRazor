using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Response-aware form that auto-configures the htmx response-targets extension
/// (<c>hx-target-422</c>, <c>hx-target-4*</c>, <c>hx-target-5*</c>) and injects
/// error-handling behavior. Removes per-form boilerplate for htmx form submissions.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-htmx-form page="/Contact" page-handler="Submit"
///                 rhx-error-target="#errors"&gt;
///     &lt;rhx-input rhx-for="Name" /&gt;
///     &lt;rhx-button type="submit"&gt;Send&lt;/rhx-button&gt;
/// &lt;/rhx-htmx-form&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-htmx-form")]
public class HtmxFormTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "htmx-form";

    // ──────────────────────────────────────────────
    //  Route properties
    // ──────────────────────────────────────────────

    /// <summary>
    /// The Razor Page path for the form action URL.
    /// </summary>
    [HtmlAttributeName("page")]
    public string? Page { get; set; }

    /// <summary>
    /// The page handler name for form submission.
    /// </summary>
    [HtmlAttributeName("page-handler")]
    public string? PageHandler { get; set; }

    /// <summary>
    /// Route parameter values for URL generation. Bound from route-* attributes.
    /// </summary>
    [HtmlAttributeName("route-", DictionaryAttributePrefix = "route-")]
    public Dictionary<string, string> RouteValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // ──────────────────────────────────────────────
    //  Form behavior properties
    // ──────────────────────────────────────────────

    /// <summary>
    /// The HTTP method for the form. Default: "post".
    /// Accepted values: <c>post</c>, <c>put</c>, <c>patch</c>, <c>delete</c>.
    /// </summary>
    [HtmlAttributeName("rhx-method")]
    public string Method { get; set; } = "post";

    /// <summary>
    /// CSS selector for the element that receives 422 validation error responses.
    /// </summary>
    [HtmlAttributeName("rhx-target-422")]
    public string? Target422 { get; set; }

    /// <summary>
    /// CSS selector for the element that receives 4xx client error responses.
    /// </summary>
    [HtmlAttributeName("rhx-target-4xx")]
    public string? Target4xx { get; set; }

    /// <summary>
    /// CSS selector for the element that receives 5xx server error responses.
    /// </summary>
    [HtmlAttributeName("rhx-target-5xx")]
    public string? Target5xx { get; set; }

    /// <summary>
    /// Shorthand: sets all error targets (422, 4xx, 5xx) to the same CSS selector.
    /// Specific targets (<see cref="Target422"/>, etc.) override this value.
    /// </summary>
    [HtmlAttributeName("rhx-error-target")]
    public string? ErrorTarget { get; set; }

    /// <summary>
    /// Whether to reset the form on successful submission. Default: false.
    /// </summary>
    [HtmlAttributeName("rhx-reset-on-success")]
    public bool ResetOnSuccess { get; set; }

    /// <summary>
    /// Whether to disable submit buttons during form submission. Default: true.
    /// </summary>
    [HtmlAttributeName("rhx-disable-on-submit")]
    public bool DisableOnSubmit { get; set; } = true;

    /// <summary>
    /// CSS selector of the element to show as a loading indicator during submission.
    /// </summary>
    [HtmlAttributeName("rhx-indicator")]
    public string? Indicator { get; set; }

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="HtmxFormTagHelper"/> instance.
    /// </summary>
    public HtmxFormTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

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

        output.TagName = "form";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder();
        ApplyBaseAttributes(output, css);

        // Set the verb attribute based on method
        var url = GenerateRouteUrl();
        var verbAttr = $"hx-{Method.ToLowerInvariant()}";
        if (!string.IsNullOrWhiteSpace(url))
            output.Attributes.SetAttribute(verbAttr, url);

        // Default target and swap
        if (HxTarget == null)
            output.Attributes.SetAttribute("hx-target", "this");
        if (HxSwap == null)
            output.Attributes.SetAttribute("hx-swap", "innerHTML");

        // Merge "response-targets" into hx-ext
        var existingExt = HxExt;
        HxExt = string.IsNullOrWhiteSpace(existingExt)
            ? "response-targets"
            : $"response-targets,{existingExt}";

        // Error response targets
        var t422 = Target422 ?? ErrorTarget;
        var t4xx = Target4xx ?? ErrorTarget;
        var t5xx = Target5xx ?? ErrorTarget;

        if (!string.IsNullOrWhiteSpace(t422))
            output.Attributes.SetAttribute("hx-target-422", t422);
        if (!string.IsNullOrWhiteSpace(t4xx))
            output.Attributes.SetAttribute("hx-target-4*", t4xx);
        if (!string.IsNullOrWhiteSpace(t5xx))
            output.Attributes.SetAttribute("hx-target-5*", t5xx);

        // Disable submit buttons during request
        if (DisableOnSubmit)
            output.Attributes.SetAttribute("hx-disabled-elt", "find button[type='submit']");

        // Loading indicator
        if (!string.IsNullOrWhiteSpace(Indicator))
            HxIndicator = Indicator;

        // Reset on success via htmx's own hx-on (no custom JS).
        if (ResetOnSuccess)
            output.Attributes.SetAttribute(
                "hx-on::after-request", "if(event.detail.successful)this.reset()");

        RenderHtmxAttributes(output);

        // Render child content + error container. No `hidden` attribute: CSS hides the
        // container while :empty and styles it once htmx swaps an error message in. The
        // submitting state is htmx's built-in .htmx-request class.
        var childContent = await output.GetChildContentAsync();
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml(
            "<div class=\"rhx-htmx-form__error-container\" aria-live=\"polite\"></div>");
    }
}
