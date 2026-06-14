using System.Text.RegularExpressions;
using htmxRazor.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Actions;

/// <summary>
/// Captures trigger content for a dropdown menu. The child (typically an
/// <c>&lt;rhx-button&gt;</c>) becomes the popover invoker: the native
/// <see href="https://developer.mozilla.org/docs/Web/API/Popover_API">Popover API</see>
/// attributes (<c>popovertarget</c>, <c>popovertargetaction</c>) plus
/// <c>aria-haspopup</c>, <c>aria-controls</c> and the CSS Anchor Positioning
/// <c>anchor-name</c> are injected directly onto the trigger button.
/// </summary>
/// <remarks>
/// <para>
/// This tag helper suppresses its own output — the content is rendered by the
/// parent <see cref="DropdownTagHelper"/> in the correct position. No JavaScript is
/// used: the browser provides open/close, light-dismiss (click outside) and Escape
/// for free via the Popover API.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;rhx-dropdown&gt;
///     &lt;rhx-dropdown-trigger&gt;
///         &lt;rhx-button rhx-variant="brand"&gt;Actions&lt;/rhx-button&gt;
///     &lt;/rhx-dropdown-trigger&gt;
///     &lt;rhx-dropdown-item&gt;Edit&lt;/rhx-dropdown-item&gt;
/// &lt;/rhx-dropdown&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-dropdown-trigger", ParentTag = "rhx-dropdown")]
public class DropdownTriggerTagHelper : TagHelper
{
    // Matches the opening tag of the first <button ...> or <a ...> in the trigger content.
    private static readonly Regex FirstControlTag =
        new("<(button|a)\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var slots = SlotRenderer.FromContext(context);
        if (slots == null)
        {
            // Not inside a dropdown — render as pass-through
            output.TagName = null;
            return;
        }

        var childContent = await output.GetChildContentAsync();

        // Read parent dropdown state
        var dropdown = context.Items.TryGetValue(typeof(DropdownTagHelper), out var dd)
            ? dd as DropdownTagHelper
            : null;
        var isDisabled = dropdown?.Disabled ?? false;

        // Read panel ID + anchor name set up by the parent dropdown.
        var panelId = context.Items.TryGetValue("DropdownPanelId", out var id)
            ? id as string
            : null;
        var anchorName = context.Items.TryGetValue("DropdownAnchorName", out var an)
            ? an as string
            : null;

        var triggerHtml = childContent.GetContent();

        // Build the attributes that turn the inner control into a popover invoker.
        var injected = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(panelId))
        {
            injected.Append($" popovertarget=\"{panelId}\"");
            injected.Append(" popovertargetaction=\"toggle\"");
            injected.Append($" aria-controls=\"{panelId}\"");
        }
        injected.Append(" aria-haspopup=\"menu\"");
        if (isDisabled)
        {
            injected.Append(" aria-disabled=\"true\"");
        }
        if (!string.IsNullOrEmpty(anchorName))
        {
            injected.Append($" style=\"anchor-name:{anchorName}\"");
        }

        // Inject the attributes into the opening tag of the first <button>/<a>.
        var match = FirstControlTag.Match(triggerHtml);
        string finalHtml;
        if (match.Success)
        {
            var insertAt = match.Index + match.Length;
            finalHtml = triggerHtml.Insert(insertAt, injected.ToString());
        }
        else
        {
            // Fallback: no recognizable control — wrap content in a button invoker.
            finalHtml = $"<button type=\"button\" class=\"rhx-dropdown__trigger\"{injected}>{triggerHtml}</button>";
        }

        // Register assembled HTML to trigger slot
        slots.SetHtml("trigger", finalHtml);

        output.SuppressOutput();
    }
}
