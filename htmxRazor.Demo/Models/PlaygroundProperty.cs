namespace htmxRazor.Demo.Models;

/// <summary>
/// Defines a configurable property for the interactive component playground.
/// </summary>
/// <param name="Name">The attribute name (e.g., "rhx-variant").</param>
/// <param name="Type">Control type: "select", "bool", "string", "number".</param>
/// <param name="Default">Default value for the property.</param>
/// <param name="Description">Brief description shown as tooltip/label.</param>
/// <param name="Options">Available options for "select" type properties.</param>
public record PlaygroundProperty(
    string Name,
    string Type,
    string Default,
    string Description,
    string[]? Options = null
)
{
    /// <summary>
    /// Resolves the current value from query parameters, falling back to the default.
    /// </summary>
    public string GetCurrentValue(IQueryCollection query)
    {
        var key = Name.Replace("rhx-", "").Replace("-", "");
        return query.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val)
            ? val.ToString()
            : Default;
    }

    /// <summary>
    /// Returns the query parameter key for this property.
    /// </summary>
    public string QueryKey => Name.Replace("rhx-", "").Replace("-", "");
}
