namespace Limbo.Umbraco.ModelsBuilder.Models;

/// <summary>
/// Model class representing the settings to be exposed via the server variables endpoint.
/// </summary>
public class ServerVariablesSettings {

    /// <summary>
    /// If set to <c>true</c> (default), the dashboard of the build-in Models Builder will be removed.
    /// </summary>
    public required bool DisableDefaultDashboard { get; set; }

}