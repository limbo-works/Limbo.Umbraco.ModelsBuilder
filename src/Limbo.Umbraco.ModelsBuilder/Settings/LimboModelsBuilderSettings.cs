﻿using J2N.Collections.Generic;
using Limbo.Umbraco.ModelsBuilder.Containers;
using Skybrud.Essentials.Time;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Limbo.Umbraco.ModelsBuilder.Settings {

    [UmbracoOptions(Constants.Configuration.ConfigModelsBuilder, BindNonPublicProperties = true)]
    public class LimboModelsBuilderSettings : ModelsBuilderSettings {

        //private readonly ModelsBuilderSettings _umbracoModelsBuilderSettings = new ModelsBuilderSettings();

        // Ideally we'd want to extend "ModelsBuilderSettings", but inheritance doesn't seem to work properly with the IOptions<> pattern :(

        #region Properties

        //public ModelsMode ModelsMode { get; set; }

        ///// <summary>
        ///// Gets or sets a value for models namespace.
        ///// </summary>
        ///// <remarks>That value could be overriden by other (attribute in user's code...). Return default if no value was supplied.</remarks>
        //public string ModelsNamespace { get; set; }

        ///// <summary>
        ///// Gets or sets a value for the models directory.
        ///// </summary>
        ///// <remarks>Default is ~/umbraco/models but that can be changed.</remarks>
        //public string ModelsDirectory { get; set; }

        ///// <summary>
        ///// Gets or sets a value indicating whether to accept an unsafe value for ModelsDirectory.
        ///// </summary>
        ///// <remarks>
        ///// An unsafe value is an absolute path, or a relative path pointing outside
        ///// of the website root.
        ///// </remarks>
        //public bool AcceptUnsafeModelsDirectory {
        //    get => _umbracoModelsBuilderSettings.AcceptUnsafeModelsDirectory;
        //    set => _umbracoModelsBuilderSettings.AcceptUnsafeModelsDirectory = value;
        //}

        /// <summary>
        /// Gets or sets whether models should be added to nested directories based on their type - eg. so regular
        /// content types are placed in a <c>Content</c> sub directory. Default is <c>true</c>.
        ///
        /// Notice that even if set to <c>false</c>, each type may have individual settings (eg. if set through events).
        /// </summary>
        public bool UseDirectories { get; set; } = true;

        public string Loaded { get; } = EssentialsTime.Now.Iso8601;

        public List<HelloContainer> Containers { get; set; }

        #endregion

    }

}