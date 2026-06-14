using System.Net;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Feedback;

/// <summary>
/// Renders a callout/alert box with variant-appropriate icon, optional close button,
/// and optional auto-dismiss. Uses <c>role="alert"</c> for accessibility.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-callout rhx-variant="success"&gt;Item saved successfully!&lt;/rhx-callout&gt;
///
/// &lt;rhx-callout rhx-variant="danger" rhx-closable="true" rhx-duration="5000"&gt;
///     An error occurred while processing your request.
/// &lt;/rhx-callout&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-callout")]
public class CalloutTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "callout";

    // ──────────────────────────────────────────────
    //  Callout-specific properties
    // ──────────────────────────────────────────────

    /// <summary>
    /// The color variant of the callout.
    /// Options: neutral, brand, success, warning, danger.
    /// Default: neutral.
    /// </summary>
    [HtmlAttributeName("rhx-variant")]
    public string Variant { get; set; } = "neutral";

    /// <summary>
    /// Whether the callout is visible. Default: true.
    /// </summary>
    [HtmlAttributeName("rhx-open")]
    public bool Open { get; set; } = true;

    /// <summary>
    /// Whether to show a close button. Default: false.
    /// </summary>
    [HtmlAttributeName("rhx-closable")]
    public bool Closable { get; set; }

    /// <summary>
    /// Custom icon name. When not specified, an appropriate icon is auto-selected
    /// based on the variant (info-circle for neutral/brand, check-circle for success,
    /// exclamation-triangle for warning, exclamation-circle for danger).
    /// </summary>
    [HtmlAttributeName("rhx-icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Auto-close duration in milliseconds. 0 = no auto-close. Default: 0.
    /// </summary>
    [HtmlAttributeName("rhx-duration")]
    public int Duration { get; set; }

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    public CalloutTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    // ──────────────────────────────────────────────
    //  Rendering
    // ──────────────────────────────────────────────

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var variant = Variant.ToLowerInvariant();

        // ── CSS classes ──
        var css = CreateCssBuilder()
            .Add(GetModifierClass(variant));

        ApplyBaseAttributes(output, css);

        output.Attributes.SetAttribute("role", "alert");

        if (!Open)
            output.Attributes.SetAttribute("hidden", "hidden");

        // Auto-dismiss: fade out after the duration via a delayed CSS animation (no JS).
        if (Duration > 0)
        {
            var anim = $"animation: rhx-callout-auto-dismiss 300ms ease {Duration}ms forwards";
            var existing = output.Attributes["style"]?.Value?.ToString();
            output.Attributes.SetAttribute("style",
                string.IsNullOrWhiteSpace(existing) ? anim : existing!.TrimEnd(';', ' ') + "; " + anim);
        }

        // ── Build inner content ──
        var childContent = await output.GetChildContentAsync();
        output.Content.Clear();

        // Icon
        var iconSvg = GetIconSvg(variant);
        output.Content.AppendHtml($"<span class=\"{GetElementClass("icon")}\" aria-hidden=\"true\">{iconSvg}</span>");

        // Content wrapper
        output.Content.AppendHtml($"<div class=\"{GetElementClass("content")}\">");
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</div>");

        // Close control — a focusable checkbox + label; CSS (:has(:checked)) hides the callout.
        // No JavaScript: dismissing is a pure CSS state toggle.
        if (Closable)
        {
            var dismissId = "rhx-callout-dismiss-" + context.UniqueId;
            output.Content.AppendHtml(
                $"<input type=\"checkbox\" id=\"{Enc(dismissId)}\" class=\"{GetElementClass("dismiss")} rhx-sr-only\" aria-label=\"Close\" />");
            output.Content.AppendHtml(
                $"<label class=\"{GetElementClass("close")}\" for=\"{Enc(dismissId)}\" title=\"Close\" aria-hidden=\"true\">" +
                "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
                "<line x1=\"18\" y1=\"6\" x2=\"6\" y2=\"18\"></line><line x1=\"6\" y1=\"6\" x2=\"18\" y2=\"18\"></line>" +
                "</svg></label>");
        }

        // ── htmx attributes ──
        RenderHtmxAttributes(output);
    }

    // ──────────────────────────────────────────────
    //  Icon resolution
    // ──────────────────────────────────────────────

    private string GetIconSvg(string variant)
    {
        if (!string.IsNullOrEmpty(Icon))
            return GetNamedIconSvg(Icon);

        return variant switch
        {
            "success" => CheckCircleSvg,
            "warning" => ExclamationTriangleSvg,
            "danger" => ExclamationCircleSvg,
            _ => InfoCircleSvg // neutral, brand
        };
    }

    private static string GetNamedIconSvg(string name) => name switch
    {
        "check-circle" => CheckCircleSvg,
        "exclamation-triangle" => ExclamationTriangleSvg,
        "exclamation-circle" => ExclamationCircleSvg,
        "info-circle" => InfoCircleSvg,
        _ => InfoCircleSvg
    };

    private const string InfoCircleSvg =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"20\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<circle cx=\"12\" cy=\"12\" r=\"10\"></circle><line x1=\"12\" y1=\"16\" x2=\"12\" y2=\"12\"></line><line x1=\"12\" y1=\"8\" x2=\"12.01\" y2=\"8\"></line>" +
        "</svg>";

    private const string CheckCircleSvg =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"20\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<path d=\"M22 11.08V12a10 10 0 1 1-5.93-9.14\"></path><polyline points=\"22 4 12 14.01 9 11.01\"></polyline>" +
        "</svg>";

    private const string ExclamationTriangleSvg =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"20\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<path d=\"M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z\"></path><line x1=\"12\" y1=\"9\" x2=\"12\" y2=\"13\"></line><line x1=\"12\" y1=\"17\" x2=\"12.01\" y2=\"17\"></line>" +
        "</svg>";

    private const string ExclamationCircleSvg =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"20\" height=\"20\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<circle cx=\"12\" cy=\"12\" r=\"10\"></circle><line x1=\"12\" y1=\"8\" x2=\"12\" y2=\"12\"></line><line x1=\"12\" y1=\"16\" x2=\"12.01\" y2=\"16\"></line>" +
        "</svg>";

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? "") ?? "";
}
