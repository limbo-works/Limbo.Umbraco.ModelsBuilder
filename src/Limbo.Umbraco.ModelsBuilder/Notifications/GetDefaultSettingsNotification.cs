using Limbo.Umbraco.ModelsBuilder.Services;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.AspNetCore.Hosting;
using Umbraco.Cms.Core.Notifications;

namespace Limbo.Umbraco.ModelsBuilder.Notifications;

/// <summary>
/// Notification that is broadcasted by the <see cref="ModelsGenerator.GetDefaultSettings"/> method.
/// </summary>
public class GetDefaultSettingsNotification : INotification {

    /// <summary>
    /// Get a reference to the models generator settings
    /// </summary>
    public ModelsGeneratorSettings Settings { get; set; }

    /// <summary>
    /// Gets a reference to the settings as specified in the <c>appSettings.json</c> file.
    /// </summary>
    public LimboModelsBuilderSettings AppSettings { get; }

    /// <summary>
    /// Gets a reference to the current web host environment.
    /// </summary>
    public IWebHostEnvironment WebHostEnvironment { get; }

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="settings"/>.
    /// </summary>
    /// <param name="settings">The models generator settings.</param>
    /// <param name="appSettings">The settings as specified in the <c>appSettings.json</c> file.</param>
    /// <param name="webHostEnvironment">The current hosting environment.</param>
    public GetDefaultSettingsNotification(ModelsGeneratorSettings settings, LimboModelsBuilderSettings appSettings, IWebHostEnvironment webHostEnvironment) {
        Settings = settings;
        AppSettings = appSettings;
        WebHostEnvironment = webHostEnvironment;
    }

}