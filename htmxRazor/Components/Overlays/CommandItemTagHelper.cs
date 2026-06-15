using System.Net;
using htmxRazor.Components.Imagery;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Overlays;

/// <summary>
/// Renders a single result item in a command palette. Can navigate to a URL on selection
/// or fire a custom event.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-command-item rhx-value="settings" rhx-href="/settings"
///     rhx-description="Manage your preferences" rhx-shortcut="⌘,"&gt;
///     Settings
/// &lt;/rhx-command-item&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-command-item")]
public class CommandItemTagHelper : htmxRazorTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "command-palette";

    /// <summary>Item value passed on selection.</summary>
    [HtmlAttributeName("rhx-value")]
    public string? Value { get; set; }

    /// <summary>Navigation URL on select.</summary>
    [HtmlAttributeName("rhx-href")]
    public string? Href { get; set; }

    /// <summary>Optional leading icon name.</summary>
    [HtmlAttributeName("rhx-icon")]
    public string? Icon { get; set; }

    /// <summary>Secondary description text.</summary>
    [HtmlAttributeName("rhx-description")]
    public string? Description { get; set; }

    /// <summary>Display keyboard shortcut hint.</summary>
    [HtmlAttributeName("rhx-shortcut")]
    public string? ShortcutHint { get; set; }

    /// <summary>Disabled state.</summary>
    [HtmlAttributeName("rhx-disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Creates a new CommandItemTagHelper with URL generation support.
    /// </summary>
    public CommandItemTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContent = await output.GetChildContentAsync();

        // A command item navigates (native <a href>) or fires an action (native <button> + htmx).
        // No JavaScript: clicking a link navigates; keyboard focus/activation are native.
        var isLink = !string.IsNullOrWhiteSpace(Href);
        output.TagName = isLink ? "a" : "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        var css = new CssClassBuilder(GetElementClass("item"))
            .AddIf($"{GetElementClass("item")}--disabled", Disabled);

        if (!string.IsNullOrWhiteSpace(CssClass))
            css.Add(CssClass);

        output.Attributes.SetAttribute("class", css.Build());
        output.Attributes.SetAttribute("role", "option");

        if (isLink)
        {
            output.Attributes.SetAttribute("href", Href!);
        }
        else
        {
            output.Attributes.SetAttribute("type", "button");
            if (Disabled) output.Attributes.SetAttribute("disabled", "disabled");
        }

        if (!string.IsNullOrWhiteSpace(Value))
            output.Attributes.SetAttribute("data-rhx-value", Value);
        if (Disabled)
            output.Attributes.SetAttribute("aria-disabled", "true");

        RenderHtmxAttributes(output);

        // Build inner content
        output.Content.Clear();

        // Icon
        if (!string.IsNullOrWhiteSpace(Icon))
        {
            var iconSvg = IconRegistry.Get(Icon) ?? "";
            output.Content.AppendHtml(
                $"<span class=\"{GetElementClass("item-icon")}\" aria-hidden=\"true\">" +
                $"<svg class=\"rhx-icon rhx-icon--small\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{iconSvg}</svg>" +
                "</span>");
        }

        // Content (label + description)
        output.Content.AppendHtml($"<div class=\"{GetElementClass("item-content")}\">");
        output.Content.AppendHtml($"<span class=\"{GetElementClass("item-label")}\">");
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</span>");
        if (!string.IsNullOrWhiteSpace(Description))
        {
            output.Content.AppendHtml(
                $"<span class=\"{GetElementClass("item-description")}\">{Enc(Description)}</span>");
        }
        output.Content.AppendHtml("</div>");

        // Shortcut hint
        if (!string.IsNullOrWhiteSpace(ShortcutHint))
        {
            output.Content.AppendHtml(
                $"<kbd class=\"{GetElementClass("item-shortcut")}\">{Enc(ShortcutHint)}</kbd>");
        }
    }

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? "") ?? "";
}
