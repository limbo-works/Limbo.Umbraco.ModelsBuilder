using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Limbo.Umbraco.ModelsBuilder.Models;


/// <summary>
/// Model class representing an error response from the API.
/// </summary>
public class ErrorResult {

    /// <summary>
    /// Gets a value indicating whether the operation completed successfully. Always <see langword="false"/>.
    /// </summary>
    [JsonProperty("success")]
    [JsonPropertyName("success")]
    public bool IsSuccessful => false;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    [JsonProperty("message")]
    [JsonPropertyName("message")]
    public required string Message { get; init; }

}