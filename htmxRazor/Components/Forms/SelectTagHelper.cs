using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Renders a custom-styled select control with button trigger, listbox panel,
/// and hidden input for form submission. Supports single and multi-select,
/// enum auto-generation, SelectListItem binding, and htmx integration.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-select rhx-for="Country" rhx-placeholder="Choose a country" /&gt;
///
/// &lt;rhx-select name="priority" rhx-label="Priority" rhx-items="Model.Priorities"
///              hx-get="/api/tasks" hx-trigger="change" hx-target="#task-list" /&gt;
///
/// &lt;rhx-select name="tags" rhx-label="Tags" rhx-multiple="true"&gt;
///     &lt;rhx-option value="bug"&gt;Bug&lt;/rhx-option&gt;
///     &lt;rhx-option value="feature"&gt;Feature&lt;/rhx-option&gt;
/// &lt;/rhx-select&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-select")]
public class SelectTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "select";

    // ──────────────────────────────────────────────
    //  Select-specific properties
    // ──────────────────────────────────────────────

    /// <summary>Placeholder text when no value is selected.</summary>
    [HtmlAttributeName("rhx-placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>Enable multi-select mode.</summary>
    [HtmlAttributeName("rhx-multiple")]
    public bool Multiple { get; set; }

    /// <summary>Maximum number of options visible before scrolling. Default: 8.</summary>
    [HtmlAttributeName("rhx-max-visible")]
    public int MaxOptionsVisible { get; set; } = 8;

    /// <summary>Show a clear button to reset the selection.</summary>
    [HtmlAttributeName("rhx-with-clear")]
    public bool WithClear { get; set; }

    /// <summary>Use the filled appearance variant.</summary>
    [HtmlAttributeName("rhx-filled")]
    public bool Filled { get; set; }

    /// <summary>
    /// Collection of SelectListItem to generate options from.
    /// When set, child rhx-option elements are ignored.
    /// </summary>
    [HtmlAttributeName("rhx-items")]
    public IEnumerable<SelectListItem>? Items { get; set; }

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    public SelectTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    // ──────────────────────────────────────────────
    //  Rendering
    // ──────────────────────────────────────────────

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var resolvedName = ResolveName();
        var resolvedId = ResolveId();
        var resolvedValue = ResolveValue();
        var resolvedRequired = ResolveRequired();
        var hasError = HasError();
        var size = Size.ToLowerInvariant();

        var hintId = $"{resolvedId}-hint";
        var errorId = $"{resolvedId}-error";
        var listboxId = $"{resolvedId}-listbox";
        var labelId = $"{resolvedId}-label";

        // Store context for child <rhx-option> elements
        context.Items["OptionClassPrefix"] = "select";
        context.Items["SelectedValues"] = ParseSelectedValues(resolvedValue);

        // Process children (needed for <rhx-option> elements)
        var childContent = await output.GetChildContentAsync();

        // ── CSS classes on wrapper ──
        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("filled"), Filled)
            .AddIf(GetModifierClass("multiple"), Multiple)
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("readonly"), Readonly)
            .AddIf(GetModifierClass("error"), hasError);

        ApplyWrapperAttributes(output, css);
        output.Attributes.SetAttribute("data-rhx-select", "");
        if (Multiple)
            output.Attributes.SetAttribute("data-rhx-select-multiple", "");

        var sb = new StringBuilder();

        // ── Label ──
        var labelText = ResolveLabelText();
        if (!string.IsNullOrEmpty(labelText))
        {
            sb.Append($"<label class=\"{GetElementClass("label")}\" id=\"{Enc(labelId)}\">{Enc(labelText)}</label>");
        }

        // ── Trigger button ──
        sb.Append($"<button class=\"{GetElementClass("trigger")}\" type=\"button\"");
        sb.Append($" id=\"{Enc(resolvedId)}\"");
        sb.Append(" role=\"combobox\"");
        sb.Append(" aria-expanded=\"false\"");
        sb.Append(" aria-haspopup=\"listbox\"");
        sb.Append($" aria-controls=\"{Enc(listboxId)}\"");
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        if (!string.IsNullOrEmpty(AriaLabel))
            sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");
        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null)
            sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");
        if (hasError)
            sb.Append(" aria-invalid=\"true\"");
        if (resolvedRequired)
            sb.Append(" aria-required=\"true\"");
        if (Disabled)
            sb.Append(" disabled");
        sb.Append(">");

        // Value display
        sb.Append($"<span class=\"{GetElementClass("value")}\">");
        if (string.IsNullOrEmpty(resolvedValue) && !string.IsNullOrEmpty(Placeholder))
            sb.Append($"<span class=\"{GetElementClass("placeholder")}\">{Enc(Placeholder)}</span>");
        else if (!string.IsNullOrEmpty(resolvedValue))
            sb.Append(Enc(GetDisplayTextForValue(resolvedValue)));
        sb.Append("</span>");

        // Arrow icon
        sb.Append($"<span class=\"{GetElementClass("arrow")}\" aria-hidden=\"true\">");
        sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">");
        sb.Append("<polyline points=\"6 9 12 15 18 9\"></polyline>");
        sb.Append("</svg></span>");

        sb.Append("</button>");

        // ── Clear button ──
        if (WithClear)
        {
            sb.Append($"<button class=\"{GetElementClass("clear")}\" type=\"button\" aria-label=\"Clear\"");
            if (string.IsNullOrEmpty(resolvedValue)) sb.Append(" hidden");
            sb.Append(">");
            sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">");
            sb.Append("<line x1=\"18\" y1=\"6\" x2=\"6\" y2=\"18\"></line><line x1=\"6\" y1=\"6\" x2=\"18\" y2=\"18\"></line>");
            sb.Append("</svg></button>");
        }

        // ── Listbox panel ──
        sb.Append($"<div class=\"{GetElementClass("listbox")}\" role=\"listbox\" id=\"{Enc(listboxId)}\"");
        if (!string.IsNullOrEmpty(labelText))
            sb.Append($" aria-labelledby=\"{Enc(labelId)}\"");
        if (Multiple)
            sb.Append(" aria-multiselectable=\"true\"");
        sb.Append($" data-rhx-max-visible=\"{MaxOptionsVisible}\"");
        sb.Append(" hidden>");

        // Generate options from Items, enum, or child content
        var generatedOptions = GenerateOptions(resolvedValue);
        if (!string.IsNullOrEmpty(generatedOptions))
            sb.Append(generatedOptions);
        else
            sb.Append(childContent.GetContent());

        sb.Append("</div>");

        // ── Hidden input(s) for form submission ──
        if (!Multiple)
        {
            // The value-carrying input. A `type="hidden"` input is barred from HTML constraint
            // validation, so native `required` never fires on it (issue #14). When the select is
            // required, render a focusable, validatable mirror instead — visually hidden via CSS
            // but still rendered, so the browser enforces `required` and anchors its validation
            // bubble to the control. Otherwise keep the plain hidden input.
            if (resolvedRequired)
                sb.Append($"<input class=\"{GetElementClass("hidden")}--required\" data-rhx-select-value required tabindex=\"-1\" aria-hidden=\"true\"");
            else
                sb.Append($"<input type=\"hidden\" class=\"{GetElementClass("hidden")}\" data-rhx-select-value");
            if (!string.IsNullOrEmpty(resolvedName))
                sb.Append($" name=\"{Enc(resolvedName)}\"");
            sb.Append($" value=\"{Enc(resolvedValue ?? "")}\"");
            sb.Append(BuildHtmxAttributeString());
            sb.Append(BuildValidationAttributeString());
            sb.Append(" />");
        }
        else
        {
            // Multi-select: container with per-value hidden inputs
            sb.Append($"<div class=\"{GetElementClass("values")}\" data-rhx-select-values data-name=\"{Enc(resolvedName)}\">");
            var values = ParseSelectedValues(resolvedValue);
            foreach (var v in values)
            {
                sb.Append($"<input type=\"hidden\" name=\"{Enc(resolvedName)}\" value=\"{Enc(v)}\" />");
            }
            sb.Append("</div>");
        }

        // ── Hint ──
        sb.Append(BuildHintHtml(hintId));

        // ── Error ──
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    // ──────────────────────────────────────────────
    //  Option generation
    // ──────────────────────────────────────────────

    private HashSet<string> ParseSelectedValues(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (Multiple)
        {
            return new HashSet<string>(
                value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.OrdinalIgnoreCase);
        }

        return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { value };
    }

    private string? GenerateOptions(string? selectedValue)
    {
        if (Items != null)
            return GenerateOptionsFromItems(selectedValue);

        if (For != null)
        {
            var modelType = Nullable.GetUnderlyingType(For.Metadata.ModelType) ?? For.Metadata.ModelType;
            if (modelType.IsEnum)
                return GenerateOptionsFromEnum(modelType, selectedValue);
        }

        return null;
    }

    private string GenerateOptionsFromItems(string? selectedValue)
    {
        var sb = new StringBuilder();
        var selected = ParseSelectedValues(selectedValue);

        foreach (var item in Items!)
        {
            var isSelected = selected.Contains(item.Value ?? "") || item.Selected;
            sb.Append($"<div class=\"{GetElementClass("option")}");
            if (isSelected) sb.Append($" {GetElementClass("option")}--selected");
            if (item.Disabled) sb.Append($" {GetElementClass("option")}--disabled");
            sb.Append("\" role=\"option\"");
            sb.Append($" data-value=\"{Enc(item.Value)}\"");
            sb.Append($" aria-selected=\"{isSelected.ToString().ToLowerInvariant()}\"");
            if (item.Disabled) sb.Append(" aria-disabled=\"true\"");
            sb.Append(" tabindex=\"-1\">");
            sb.Append(Enc(item.Text));
            sb.Append("</div>");
        }

        return sb.ToString();
    }

    private string GenerateOptionsFromEnum(Type enumType, string? selectedValue)
    {
        var sb = new StringBuilder();
        var selected = ParseSelectedValues(selectedValue);

        foreach (var val in Enum.GetValues(enumType))
        {
            var name = val.ToString()!;
            var member = enumType.GetMember(name).FirstOrDefault();
            var displayAttr = member?.GetCustomAttribute<DisplayAttribute>();
            var text = displayAttr?.Name ?? name;

            var isSelected = selected.Contains(name);
            sb.Append($"<div class=\"{GetElementClass("option")}");
            if (isSelected) sb.Append($" {GetElementClass("option")}--selected");
            sb.Append("\" role=\"option\"");
            sb.Append($" data-value=\"{Enc(name)}\"");
            sb.Append($" aria-selected=\"{isSelected.ToString().ToLowerInvariant()}\"");
            sb.Append(" tabindex=\"-1\">");
            sb.Append(Enc(text));
            sb.Append("</div>");
        }

        return sb.ToString();
    }

    private string GetDisplayTextForValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";

        if (Items != null)
        {
            var item = Items.FirstOrDefault(i =>
                string.Equals(i.Value, value, StringComparison.OrdinalIgnoreCase));
            if (item != null) return item.Text;
        }

        if (For != null)
        {
            var modelType = Nullable.GetUnderlyingType(For.Metadata.ModelType) ?? For.Metadata.ModelType;
            if (modelType.IsEnum)
            {
                var member = modelType.GetMember(value).FirstOrDefault();
                var displayAttr = member?.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr?.Name != null) return displayAttr.Name;
            }
        }

        return value;
    }
}
