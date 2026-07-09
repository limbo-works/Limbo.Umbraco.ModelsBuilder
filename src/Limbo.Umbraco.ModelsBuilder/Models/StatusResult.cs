using System.Collections.Generic;
using System.Text.Json.Serialization;
using Limbo.Umbraco.ModelsBuilder.Text.Json;
using Newtonsoft.Json;
using Skybrud.Essentials.Time;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Models;

public class StatusResult {

    [JsonProperty("success")]
    [JsonPropertyName("success")]
    public required bool IsSuccessful { get; init; }

    [JsonProperty("environment")]
    [JsonPropertyName("environment")]
    public required string Environment { get; init; }

    [JsonProperty("version")]
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonProperty("mode")]
    [JsonPropertyName("mode")]
    public required string Mode { get; init; }

    [JsonProperty("isOutOfDate")]
    [JsonPropertyName("isOutOfDate")]
    public bool IsOutOfDate { get; init; }

    [JsonProperty("lastBuildDate")]
    [JsonPropertyName("lastBuildDate")]
    [System.Text.Json.Serialization.JsonConverter(typeof(Iso8601TimeConverter))]
    public EssentialsTime? LastBuildDate { get; init; }

    [JsonProperty("links")]
    [JsonPropertyName("links")]
    public IReadOnlyList<StatusLink> Links { get; init; } = [];

}