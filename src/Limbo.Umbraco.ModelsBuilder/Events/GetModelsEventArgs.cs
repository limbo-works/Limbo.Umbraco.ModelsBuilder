using Limbo.Umbraco.ModelsBuilder.Models;
using Limbo.Umbraco.ModelsBuilder.Services;
using Limbo.Umbraco.ModelsBuilder.Settings;
using System.Collections.Generic;

namespace Limbo.Umbraco.ModelsBuilder.Events {

    /// <summary>
    /// Event arguments that is broadcasted by the <see cref="ModelsGenerator.GetModels(ModelsGeneratorSettings)"/> method.
    /// </summary>
    public class GetModelsEventArgs {

        /// <summary>
        /// Gets or sets the list of models.
        /// </summary>
        public List<TypeModel> Models { get; set; }

        /// <summary>
        /// Get a reference to the models generator settings
        /// </summary>
        public ModelsGeneratorSettings Settings { get; }

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="models"/> and <paramref name="settings"/>.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <param name="settings">The models generator settings.</param>
        public GetModelsEventArgs(List<TypeModel> models, ModelsGeneratorSettings settings) {
            Models = models;
            Settings = settings;
        }

    }

}