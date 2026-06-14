using System.Net;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Imagery;

/// <summary>
/// Renders a GIF or APNG. With a <c>rhx-poster</c> still image it stays paused (showing the
/// poster) and plays on hover/focus — a pure CSS interaction, no JavaScript. Without a poster
/// the animation plays continuously. (A GIF/APNG cannot be frozen mid-frame without a script,
/// so the JS-free "pause" is the static poster.)
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-animated-image rhx-src="animation.gif" rhx-alt="Demo" rhx-poster="first-frame.png" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-animated-image")]
public class AnimatedImageTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "animated-image";

    /// <summary>
    /// The source URL of the animated image (GIF, APNG, WebP).
    /// </summary>
    [HtmlAttributeName("rhx-src")]
    public string Src { get; set; } = "";

    /// <summary>
    /// Alt text for the image.
    /// </summary>
    [HtmlAttributeName("rhx-alt")]
    public string Alt { get; set; } = "";

    /// <summary>
    /// Optional static poster image (e.g. the first frame). When set, the control shows the
    /// poster at rest and reveals the animation on hover/focus — no JavaScript required.
    /// </summary>
    [HtmlAttributeName("rhx-poster")]
    public string? Poster { get; set; }

    /// <summary>
    /// Creates a new AnimatedImageTagHelper with URL generation support.
    /// </summary>
    public AnimatedImageTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var hasPoster = !string.IsNullOrWhiteSpace(Poster);

        var css = CreateCssBuilder();
        if (hasPoster)
            css.Add(GetModifierClass("hoverable"));
        ApplyBaseAttributes(output, css);

        RenderHtmxAttributes(output);

        output.Content.Clear();

        if (hasPoster)
        {
            // Focusable so keyboard users can play it too; the group reads as a single image.
            output.Attributes.SetAttribute("tabindex", "0");
            output.Attributes.SetAttribute("role", "img");
            if (!string.IsNullOrEmpty(Alt))
                AriaAttributeHelper.AriaLabel(output, Alt);

            output.Content.AppendHtml(
                $"<img class=\"{GetElementClass("poster")}\" src=\"{Enc(Poster)}\" alt=\"\" aria-hidden=\"true\" />");
            output.Content.AppendHtml(
                $"<img class=\"{GetElementClass("img")}\" src=\"{Enc(Src)}\" alt=\"\" aria-hidden=\"true\" />");

            var playIcon = IconRegistry.Get("play") ?? "";
            output.Content.AppendHtml(
                $"<span class=\"{GetElementClass("badge")}\" aria-hidden=\"true\">" +
                $"<svg class=\"rhx-icon rhx-icon--small\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{playIcon}</svg>" +
                "</span>");
        }
        else
        {
            output.Content.AppendHtml(
                $"<img class=\"{GetElementClass("img")}\" src=\"{Enc(Src)}\" alt=\"{Enc(Alt)}\" />");
        }
    }

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? "") ?? "";
}
