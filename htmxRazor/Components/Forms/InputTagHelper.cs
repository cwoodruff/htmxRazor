using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>
/// Renders a styled text input with label, hint, error, clear button, and password toggle.
/// Deeply integrates with ASP.NET Core model binding when <c>rhx-for</c> is set,
/// auto-detecting type, constraints, and validation from model metadata.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-input rhx-for="Email" rhx-with-clear="true" /&gt;
///
/// &lt;rhx-input rhx-label="Search" rhx-placeholder="Type to search..."
///            hx-get="/search" hx-trigger="input changed delay:300ms" hx-target="#results" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-input")]
public class InputTagHelper : FormControlTagHelperBase
{
    /// <inheritdoc/>
    protected override string BlockName => "input";

    // ──────────────────────────────────────────────
    //  Input-specific properties
    // ──────────────────────────────────────────────

    /// <summary>Placeholder text for the input.</summary>
    [HtmlAttributeName("rhx-placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// The input type. Auto-detected from model metadata when <c>rhx-for</c> is set.
    /// Options: text, email, password, number, tel, url, search, date, datetime-local, time.
    /// Default: text.
    /// </summary>
    [HtmlAttributeName("rhx-type")]
    public string? Type { get; set; }

    /// <summary>
    /// Show a clear button when the input has a value. Implemented with the native
    /// <c>type="search"</c> clear affordance (no JavaScript); applies to plain text inputs.
    /// </summary>
    [HtmlAttributeName("rhx-with-clear")]
    public bool WithClear { get; set; }

    /// <summary>
    /// Show a password visibility toggle button. Only effective when type is password.
    /// Backed by a tiny sanctioned JS holdout (<c>rhx-password-toggle.js</c>) since toggling
    /// an input's type has no HTML/CSS/htmx equivalent.
    /// </summary>
    [HtmlAttributeName("rhx-password-toggle")]
    public bool PasswordToggle { get; set; }

    /// <summary>Use the filled appearance variant (background instead of border).</summary>
    [HtmlAttributeName("rhx-filled")]
    public bool Filled { get; set; }

    /// <summary>Regex pattern for validation. Auto-set from [RegularExpression].</summary>
    [HtmlAttributeName("rhx-pattern")]
    public string? Pattern { get; set; }

    /// <summary>Minimum text length. Auto-set from [StringLength] or [MinLength].</summary>
    [HtmlAttributeName("rhx-minlength")]
    public int? Minlength { get; set; }

    /// <summary>Maximum text length. Auto-set from [StringLength] or [MaxLength].</summary>
    [HtmlAttributeName("rhx-maxlength")]
    public int? Maxlength { get; set; }

    /// <summary>Minimum value for number/date types.</summary>
    [HtmlAttributeName("rhx-min")]
    public string? Min { get; set; }

    /// <summary>Maximum value for number/date types.</summary>
    [HtmlAttributeName("rhx-max")]
    public string? Max { get; set; }

    /// <summary>Step increment for number types.</summary>
    [HtmlAttributeName("rhx-step")]
    public string? Step { get; set; }

    /// <summary>The autocomplete hint (e.g., "email", "off").</summary>
    [HtmlAttributeName("rhx-autocomplete")]
    public string? Autocomplete { get; set; }

    /// <summary>Whether the input should receive focus on page load.</summary>
    [HtmlAttributeName("rhx-autofocus")]
    public bool Autofocus { get; set; }

    // ──────────────────────────────────────────────
    //  Constructor
    // ──────────────────────────────────────────────

    public InputTagHelper(IUrlHelperFactory urlHelperFactory) : base(urlHelperFactory) { }

    // ──────────────────────────────────────────────
    //  Rendering
    // ──────────────────────────────────────────────

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var resolvedType = ResolveType();
        // A clear button is the browser's native job: type="search" renders a built-in
        // clear "✕" with no JavaScript. Only applies to plain text inputs.
        if (WithClear && resolvedType == "text")
            resolvedType = "search";
        var resolvedName = ResolveName();
        var resolvedId = ResolveId();
        var resolvedValue = ResolveValue();
        var resolvedRequired = ResolveRequired();
        var hasError = HasError();
        var size = Size.ToLowerInvariant();

        var hintId = $"{resolvedId}-hint";
        var errorId = $"{resolvedId}-error";

        // ── CSS classes on wrapper ──
        var css = CreateCssBuilder()
            .AddIf(GetModifierClass(size), size != "medium")
            .AddIf(GetModifierClass("filled"), Filled)
            .AddIf(GetModifierClass("disabled"), Disabled)
            .AddIf(GetModifierClass("readonly"), Readonly)
            .AddIf(GetModifierClass("error"), hasError);

        ApplyWrapperAttributes(output, css);

        // ── Build inner HTML ──
        var sb = new StringBuilder();

        // Label
        sb.Append(BuildLabelHtml(resolvedId));

        // Control wrapper
        sb.Append($"<div class=\"{GetElementClass("control")}\">");

        // Native input
        sb.Append($"<input class=\"{GetElementClass("native")}\"");
        sb.Append($" type=\"{Enc(resolvedType)}\"");
        sb.Append($" id=\"{Enc(resolvedId)}\"");

        if (!string.IsNullOrEmpty(resolvedName))
            sb.Append($" name=\"{Enc(resolvedName)}\"");

        if (resolvedValue != null)
            sb.Append($" value=\"{Enc(resolvedValue)}\"");

        if (!string.IsNullOrEmpty(Placeholder))
            sb.Append($" placeholder=\"{Enc(Placeholder)}\"");

        if (resolvedRequired)
            sb.Append(" required");

        if (Disabled)
            sb.Append(" disabled");

        if (Readonly)
            sb.Append(" readonly");

        if (Autofocus)
            sb.Append(" autofocus");

        // Constraints
        var minlength = Minlength ?? ExtractMinlength();
        var maxlength = Maxlength ?? ExtractMaxlength();
        var pattern = Pattern ?? ExtractPattern();

        if (minlength.HasValue)
            sb.Append($" minlength=\"{minlength.Value}\"");
        if (maxlength.HasValue)
            sb.Append($" maxlength=\"{maxlength.Value}\"");
        if (!string.IsNullOrEmpty(pattern))
            sb.Append($" pattern=\"{Enc(pattern)}\"");
        if (!string.IsNullOrEmpty(Min))
            sb.Append($" min=\"{Enc(Min)}\"");
        if (!string.IsNullOrEmpty(Max))
            sb.Append($" max=\"{Enc(Max)}\"");
        if (!string.IsNullOrEmpty(Step))
            sb.Append($" step=\"{Enc(Step)}\"");
        if (!string.IsNullOrEmpty(Autocomplete))
            sb.Append($" autocomplete=\"{Enc(Autocomplete)}\"");

        // ARIA
        if (!string.IsNullOrEmpty(AriaLabel))
            sb.Append($" aria-label=\"{Enc(AriaLabel)}\"");

        var describedBy = BuildAriaDescribedBy(hintId, errorId);
        if (describedBy != null)
            sb.Append($" aria-describedby=\"{Enc(describedBy)}\"");

        if (hasError)
            sb.Append(" aria-invalid=\"true\"");

        if (resolvedRequired)
            sb.Append(" aria-required=\"true\"");

        // htmx attributes on native input
        sb.Append(BuildHtmxAttributeString());

        // data-val attributes for unobtrusive validation
        sb.Append(BuildValidationAttributeString());

        sb.Append(" />");

        // Password toggle — backed by the tiny sanctioned holdout (rhx-password-toggle.js).
        if (PasswordToggle && resolvedType == "password")
        {
            sb.Append($"<button class=\"{GetElementClass("toggle")}\" type=\"button\" aria-label=\"Show password\" data-rhx-input-toggle>");
            sb.Append($"<svg class=\"{GetElementClass("toggle-show")}\" xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">");
            sb.Append("<path d=\"M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z\"></path><circle cx=\"12\" cy=\"12\" r=\"3\"></circle>");
            sb.Append("</svg>");
            sb.Append($"<svg class=\"{GetElementClass("toggle-hide")}\" xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" hidden>");
            sb.Append("<path d=\"M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24\"></path>");
            sb.Append("<line x1=\"1\" y1=\"1\" x2=\"23\" y2=\"23\"></line>");
            sb.Append("</svg></button>");
        }

        sb.Append("</div>"); // close control

        // Hint
        sb.Append(BuildHintHtml(hintId));

        // Error
        sb.Append(BuildErrorHtml(errorId));

        output.Content.SetHtmlContent(sb.ToString());
    }

    // ──────────────────────────────────────────────
    //  Type inference
    // ──────────────────────────────────────────────

    private string ResolveType()
    {
        if (!string.IsNullOrEmpty(Type)) return Type;

        if (For != null)
        {
            // Check DataTypeName first (from [DataType] or [EmailAddress] etc.)
            var dataType = For.Metadata.DataTypeName;
            if (!string.IsNullOrEmpty(dataType))
            {
                return dataType switch
                {
                    "EmailAddress" => "email",
                    "Url" => "url",
                    "PhoneNumber" => "tel",
                    "Password" => "password",
                    "Date" => "date",
                    "DateTime" => "datetime-local",
                    "Time" => "time",
                    _ => "text"
                };
            }

            // Check model type
            var modelType = Nullable.GetUnderlyingType(For.Metadata.ModelType) ?? For.Metadata.ModelType;

            if (modelType == typeof(int) || modelType == typeof(long) ||
                modelType == typeof(decimal) || modelType == typeof(double) ||
                modelType == typeof(float) || modelType == typeof(short) ||
                modelType == typeof(byte))
                return "number";

            if (modelType == typeof(DateTime) || modelType == typeof(DateTimeOffset))
                return "datetime-local";

            if (modelType == typeof(DateOnly))
                return "date";

            if (modelType == typeof(TimeOnly))
                return "time";
        }

        return "text";
    }
}
