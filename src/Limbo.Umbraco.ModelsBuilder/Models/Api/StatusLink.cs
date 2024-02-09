﻿using Newtonsoft.Json;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Models.Api;

public class StatusLink {

    [JsonProperty("text")]
    public string Text { get; set; } = null!;

    [JsonProperty("url")]
    public string Url { get; set; } = null!;

    [JsonProperty("target")]
    public string? Target { get; set; }

    [JsonProperty("rel")]
    public string? Rel { get; set; }

    [JsonProperty("icon")]
    public string? Icon { get; set; }

}