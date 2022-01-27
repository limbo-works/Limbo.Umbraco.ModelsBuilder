using Limbo.Umbraco.ModelsBuilder.Containers;
using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Limbo.Umbraco.ModelsBuilder.Settings {

    /// <summary>
    /// Class representing the settings for the models generator.
    /// </summary>
    public class ModelsGeneratorSettings {

        #region Properties

        /// <summary>
        /// Gets or sets the default namespace. The namespace used for the individual models may be overridden in the
        /// models generation process.
        /// </summary>
        public string DefaultNamespace { get; set; }
        
        /// <summary>
        /// Gets or sets a value for the models directory.
        /// </summary>
        /// <remarks>Default is <c>~/umbraco/models</c> but that can be changed.</remarks>
        public string DefaultModelsPath { get; set; }

        /// <summary>
        /// Gets or sets whether the model types should be saved in a nested directory structure (eg. so that element
        /// types are saved in a <c>Elements</c> sub directory. Default is <c>true</c>.
        /// </summary>
        public bool UseDirectories { get; set; } = true;

        /// <summary>
        /// Gets or sets a list of containers to be used when generating the models.
        /// </summary>
        public List<IModelsContainer> Containers { get; }

        /// <summary>
        /// Gets a reference to the <c>.editorconfig</c> settings.
        /// </summary>
        public EditorConfigSettings EditorConfig { get; set; }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="defaultNamespace"/> and <paramref name="defaultModelsPath"/>.
        /// </summary>
        /// <param name="defaultNamespace">The default namespace of the generated models.</param>
        /// <param name="defaultModelsPath">The default path to the directory where the generated models will be saved.</param>
        public ModelsGeneratorSettings(string defaultNamespace, string defaultModelsPath) {
            if (string.IsNullOrWhiteSpace(defaultNamespace)) throw new ArgumentNullException(nameof(defaultNamespace));
            if (string.IsNullOrWhiteSpace(defaultModelsPath)) throw new ArgumentNullException(nameof(defaultModelsPath));
            DefaultNamespace = defaultNamespace;
            DefaultModelsPath = defaultModelsPath;
            Containers = new List<IModelsContainer>();
            EditorConfig = new EditorConfigSettings();
        }

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="appSettings"/>.
        /// </summary>
        /// <param name="appSettings">The ModelsBuilder settings from the <c>appsettings.json</c> file.</param>
        /// <param name="hostingEnvironment">The current hosting environment.</param>
        public ModelsGeneratorSettings(LimboModelsBuilderSettings appSettings, IHostingEnvironment hostingEnvironment) {

            // Update the settings instance based on the configuration
            DefaultModelsPath = appSettings.ModelsDirectoryAbsolute(hostingEnvironment);
            DefaultNamespace = appSettings.ModelsNamespace;
            UseDirectories = appSettings.UseDirectories;

            EditorConfig = new EditorConfigSettings();

        }

        #endregion

    }

}