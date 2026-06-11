using Microsoft.AspNetCore.Razor.TagHelpers;

namespace htmxRazor.Components.Forms;

/// <summary>Data captured from one <c>&lt;rhx-radial-option&gt;</c> wedge.</summary>
public sealed class RadialOptionData
{
    /// <summary>Category identifier, submitted via the wrapper's category input.</summary>
    public string Value { get; init; } = "";

    /// <summary>Accessible name + visible label for the wedge.</summary>
    public string Label { get; init; } = "";

    /// <summary>Icon name resolved through <see cref="Imagery.IconRegistry"/>, or null.</summary>
    public string? Icon { get; init; }

    /// <summary>Requested wedge color variant, or null to use the deterministic cycle.</summary>
    public string? Color { get; init; }

    /// <summary>Endpoint returning this category's option fragment (fired via <c>htmx.ajax</c> by the JS).</summary>
    public string? HxGet { get; init; }

    /// <summary>Whether the wedge is dimmed and non-selectable.</summary>
    public bool Disabled { get; init; }
}

/// <summary>
/// One pie wedge / category inside <c>&lt;rhx-radial-select&gt;</c>. Contributes its data
/// to the parent via <c>context.Items["RhxRadialOptions"]</c> and renders nothing itself.
/// </summary>
/// <example>
/// <code>
/// &lt;rhx-radial-select name="FoodItem" rhx-category-name="Category"&gt;
///     &lt;rhx-radial-option rhx-value="fruit" rhx-label="Fruit" rhx-icon="apple"
///                        rhx-color="danger" hx-get="/Menu?handler=Items&amp;cat=fruit" /&gt;
/// &lt;/rhx-radial-select&gt;
/// </code>
/// </example>
[HtmlTargetElement("rhx-radial-option", ParentTag = "rhx-radial-select")]
public sealed class RadialOptionTagHelper : TagHelper
{
    /// <summary>Key under which the parent stores the collected option list in <c>context.Items</c>.</summary>
    public const string ItemsKey = "RhxRadialOptions";

    /// <summary>Category identifier, submitted via the wrapper's category input.</summary>
    [HtmlAttributeName("rhx-value")] public string? Value { get; set; }

    /// <summary>Accessible name + visible label for the wedge.</summary>
    [HtmlAttributeName("rhx-label")] public string? Label { get; set; }

    /// <summary>Icon name resolved through <see cref="Imagery.IconRegistry"/>.</summary>
    [HtmlAttributeName("rhx-icon")] public string? Icon { get; set; }

    /// <summary>Wedge color variant: brand, success, warning, danger, neutral. Omitted = deterministic cycle.</summary>
    [HtmlAttributeName("rhx-color")] public string? Color { get; set; }

    /// <summary>Endpoint returning this category's option fragment.</summary>
    [HtmlAttributeName("hx-get")] public string? HxGet { get; set; }

    /// <summary>Whether the wedge is dimmed and non-selectable.</summary>
    [HtmlAttributeName("rhx-disabled")] public bool Disabled { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (context.Items.TryGetValue(ItemsKey, out var obj)
            && obj is List<RadialOptionData> options)
        {
            options.Add(new RadialOptionData
            {
                Value = Value ?? "",
                Label = Label ?? "",
                Icon = Icon,
                Color = Color,
                HxGet = HxGet,
                Disabled = Disabled,
            });
        }

        output.SuppressOutput();
    }
}
