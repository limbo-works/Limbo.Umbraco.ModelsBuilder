using System.Text.Json.Serialization;
using Newtonsoft.Json;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Models;

public class BuildResult {

    [JsonProperty("success")]
    [JsonPropertyName("success")]
    public required bool IsSuccessful { get; init; }

}