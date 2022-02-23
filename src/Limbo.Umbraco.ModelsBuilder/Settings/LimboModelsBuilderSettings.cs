using Limbo.Umbraco.ModelsBuilder.Containers;
using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Limbo.Umbraco.ModelsBuilder.Settings {

    /// <summary>
    /// Class extending Umbraco's <see cref="ModelsBuilderSettings"/> with options relevant to this package.
    /// </summary>
    [UmbracoOptions(Constants.Configuration.ConfigModelsBuilder, BindNonPublicProperties = true)]
    public class LimboModelsBuilderSettings : ModelsBuilderSettings {
        
        #region Properties
        
        /// <summary>
        /// Gets or sets whether models should be added to nested directories based on their type - eg. so regular
        /// content types are placed in a <c>Content</c> sub directory. Default is <c>true</c>.
        ///
        /// Notice that even if set to <c>false</c>, each type may have individual settings (eg. if set through events).
        /// </summary>
        public bool UseDirectories { get; set; } = true;

        /// <summary>
        /// If set to <c>true</c> (default), the dashboard of the build-in Models Builder will be removed.
        /// </summary>
        public bool DisableDefaultDashboard { get; set; } = true;

        /// <summary>
        /// Gets or sets a list of containers to be used when generating the models.
        /// </summary>
        public List<HelloContainer> Containers { get; set; } = new();

        #endregion

    }

}