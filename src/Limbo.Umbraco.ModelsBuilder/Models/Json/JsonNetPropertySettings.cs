using Newtonsoft.Json;

namespace Limbo.Umbraco.ModelsBuilder.Models.Json;

/// <summary>
/// Class describing the JSON.net settings of a property.
/// </summary>
public class JsonNetPropertySettings {

    /// <summary>
    /// Gets or sets whether the property should be ignored when serialized to JSON.
    /// </summary>
    public bool Ignore { get; set; }

    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="NullValueHandling"/> option of the property.
    /// </summary>
    public NullValueHandling NullValueHandling { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DefaultValueHandling"/> option of the property.
    /// </summary>
    public DefaultValueHandling DefaultValueHandling { get; set; }

    /// <summary>
    /// Gets or sets the order of the property.
    /// </summary>
    public int? Order { get; set; }

}