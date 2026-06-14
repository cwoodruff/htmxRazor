using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Renders a native <c>&lt;option&gt;</c> for use inside <c>&lt;rhx-select&gt;</c>
/// (a native <c>&lt;select&gt;</c>) or <c>&lt;rhx-combobox&gt;</c> (a native
/// <c>&lt;datalist&gt;</c>). The parent component records, via the tag helper context,
/// which values are currently selected so the option can mark itself <c>selected</c>.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-select name="country"&gt;
///     &lt;rhx-option value="us"&gt;United States&lt;/rhx-option&gt;
///     &lt;rhx-option value="ca"&gt;Canada&lt;/rhx-option&gt;
///     &lt;rhx-option value="mx" rhx-disabled="true"&gt;Mexico&lt;/rhx-option&gt;
/// &lt;/rhx-select&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-option")]
public class OptionTagHelper : TagHelper
{
    /// <summary>The value submitted with the form when this option is selected.</summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>Whether this option is disabled and cannot be selected.</summary>
    [HtmlAttributeName("rhx-disabled")]
    public bool Disabled { get; set; }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContent = await output.GetChildContentAsync();
        var text = childContent.GetContent().Trim();

        // "select" → native <select> option (supports selected/disabled);
        // "combobox" → native <datalist> option (suggestion only).
        var prefix = context.Items.TryGetValue("OptionClassPrefix", out var p)
            ? p as string ?? "select"
            : "select";
        var selectedValues = context.Items.TryGetValue("SelectedValues", out var sv)
            ? sv as HashSet<string>
            : null;

        output.TagName = "option";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (prefix == "combobox")
        {
            // Native <datalist> suggestion: the input submits its literal text, so the
            // option's value IS the display text (native autocomplete has no hidden code).
            output.Attributes.SetAttribute("value", text);
        }
        else
        {
            // Native <select> option: value/label are distinct; convey selection + disabled.
            var value = Value ?? text;
            output.Attributes.SetAttribute("value", value);
            if (selectedValues?.Contains(value) == true)
                output.Attributes.SetAttribute("selected", "selected");
            if (Disabled)
                output.Attributes.SetAttribute("disabled", "disabled");
        }

        // Content from GetContent() is already HTML-safe (Razor encodes expressions)
        output.Content.SetHtmlContent(text);
    }
}
