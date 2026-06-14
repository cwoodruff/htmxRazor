using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Utilities;

/// <summary>
/// Wraps child content and applies CSS animations when triggered.
/// Animations can be triggered on load or via JavaScript/htmx events.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-animation rhx-name="fadeIn"&gt;Hello&lt;/rhx-animation&gt;
/// &lt;rhx-animation rhx-name="slideInUp" rhx-duration="500" rhx-delay="200"&gt;Content&lt;/rhx-animation&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-animation")]
public class AnimationTagHelper : htmxRazorTagHelperBase
{
    protected override string BlockName => "animation";

    /// <summary>
    /// The predefined animation name (e.g., fadeIn, slideInLeft, bounceIn, zoomIn).
    /// </summary>
    [HtmlAttributeName("rhx-name")]
    public string Name { get; set; } = "fadeIn";

    /// <summary>
    /// Animation duration in milliseconds. Default: 300.
    /// </summary>
    [HtmlAttributeName("rhx-duration")]
    public int Duration { get; set; } = 300;

    /// <summary>
    /// Animation delay in milliseconds. Default: 0.
    /// </summary>
    [HtmlAttributeName("rhx-delay")]
    public int Delay { get; set; }

    /// <summary>
    /// Animation direction: normal, reverse, alternate, alternate-reverse.
    /// </summary>
    [HtmlAttributeName("rhx-direction")]
    public string? Direction { get; set; }

    /// <summary>
    /// Animation easing: ease, linear, ease-in, ease-out, ease-in-out, or a cubic-bezier value.
    /// </summary>
    [HtmlAttributeName("rhx-easing")]
    public string? Easing { get; set; }

    /// <summary>
    /// Number of iterations, or "infinite". Default: "1".
    /// </summary>
    [HtmlAttributeName("rhx-iterations")]
    public string? Iterations { get; set; }

    /// <summary>
    /// Animation fill mode: none, forwards, backwards, both. Default: both.
    /// </summary>
    [HtmlAttributeName("rhx-fill")]
    public string Fill { get; set; } = "both";

    /// <summary>
    /// Whether the animation should play immediately. Default: true.
    /// </summary>
    [HtmlAttributeName("rhx-play")]
    public bool Play { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = CreateCssBuilder()
            .AddIf(GetModifierClass("playing"), Play);
        ApplyBaseAttributes(output, css);

        // Emit the CSS animation shorthand directly so the animation runs on render
        // (and on htmx swap) with no JavaScript. The @keyframes (rhx-{name}) ship in CSS.
        var direction = string.IsNullOrWhiteSpace(Direction) ? "normal" : Direction.ToLowerInvariant();
        var easing = string.IsNullOrWhiteSpace(Easing) ? "ease" : Easing;
        var iterations = string.IsNullOrWhiteSpace(Iterations) ? "1" : Iterations;
        var fill = string.IsNullOrWhiteSpace(Fill) ? "both" : Fill.ToLowerInvariant();

        var animation = $"animation: rhx-{Name} {Duration}ms {easing} {Delay}ms {iterations} {direction} {fill}";
        if (!Play)
            animation += "; animation-play-state: paused";

        var existingStyle = output.Attributes["style"]?.Value?.ToString();
        var style = string.IsNullOrWhiteSpace(existingStyle)
            ? animation
            : existingStyle.TrimEnd(';', ' ') + "; " + animation;
        output.Attributes.SetAttribute("style", style);

        RenderHtmxAttributes(output);
    }
}
