using Limbo.Umbraco.ModelsBuilder.Services;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Notifications;

namespace Limbo.Umbraco.ModelsBuilder.Notifications {

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
        /// Gets a reference to the current hosting environment.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The models generator settings.</param>
        /// <param name="appSettings">The settings as specified in the <c>appSettings.json</c> file.</param>
        /// <param name="hostingEnvironment">The current hosting environment.</param>
        public GetDefaultSettingsNotification(ModelsGeneratorSettings settings, LimboModelsBuilderSettings appSettings, IHostingEnvironment hostingEnvironment) {
            Settings = settings;
            AppSettings = appSettings;
            HostingEnvironment = hostingEnvironment;
        }

    }

}