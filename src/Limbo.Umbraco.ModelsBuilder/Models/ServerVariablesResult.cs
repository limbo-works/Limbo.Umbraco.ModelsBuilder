namespace Limbo.Umbraco.ModelsBuilder.Models;

/// <summary>
/// Model class representing the result of a request to the server variables endpoint.
/// </summary>
public class ServerVariablesResult {

    /// <summary>
    /// Gets the informational version of the package.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets the cache buster value for the package.
    /// </summary>
    public required string CacheBuster { get; init; }

    /// <summary>
    /// Gets or sets the settings to be exposed via the server variables.
    /// </summary>
    public required ServerVariablesSettings Settings { get; init; }

}