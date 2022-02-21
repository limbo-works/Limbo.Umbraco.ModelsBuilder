using Limbo.Umbraco.ModelsBuilder.Events;
using Limbo.Umbraco.ModelsBuilder.Models;
using Limbo.Umbraco.ModelsBuilder.Notifications;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.Extensions.Options;
using Skybrud.Essentials.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Limbo.Umbraco.ModelsBuilder.Services {

    /// <summary>
    /// Primary class for the models generator. The class is available via dependency injection as a transitient service.
    /// </summary>
    public class ModelsGenerator {
        
        private readonly ModelsGeneratorDependencies _dependencies;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IContentTypeService _contentTypeService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
        private readonly IOptions<LimboModelsBuilderSettings> _modelsBuilderSettings;

        /// <summary>
        /// Event called before models are returned by the <see cref="GetModels(ModelsGeneratorSettings)"/> method.
        /// </summary>
        public event EventHandler<GetModelsEventArgs> GetModelsReturning;

        #region Constructors

        /// <summary>
        /// Initializes a new instance based on the specified dependencies.
        /// </summary>
        /// <param name="dependencies">The dependencies for this class.</param>
        public ModelsGenerator(ModelsGeneratorDependencies dependencies) {
            _dependencies = dependencies;
            _hostingEnvironment = dependencies.HostingEnvironment;
            _contentTypeService = dependencies.ContentTypeService;
            _memberTypeService = dependencies.MemberTypeService;
            _mediaTypeService = dependencies.MediaTypeService;
            _publishedContentTypeFactory = dependencies.PublishedContentTypeFactory;
            _modelsBuilderSettings = dependencies.ModelsBuilderSettings;
        }

        #endregion

        #region Member methods

        void OnGetModelsReturning(ref List<TypeModel> models, ModelsGeneratorSettings settings) {
            
            // Initialize the event arguments
            GetModelsEventArgs args = new(models, settings);
            
            // Invoke the event handlers (if any)
            GetModelsReturning?.Invoke(this, args);
            
            // Set "models" in case an event handler replaced the list
            models = args.Models;

            // Initialize a new notification object
            GetModelsNotification notification = new(models, settings);

            // Publish/broadcast the notification via the event aggregator
            _dependencies.EventAggregator.Publish(notification);
            
            // Set "models" in case a notification handler replaced the list
            models = args.Models;

        }

        /// <summary>
        /// Returns the default settings, as specified in the <c>appsettings.json</c> file.
        /// </summary>
        /// <returns>An instance of <see cref="ModelsGeneratorSettings"/>.</returns>
        public virtual ModelsGeneratorSettings GetDefaultSettings() {

            // Get a reference to the ModelsBuilder appSettings section (Limbo style)
            LimboModelsBuilderSettings appSettings = _modelsBuilderSettings.Value;

            // Initialize a new settings instance
            ModelsGeneratorSettings settings = new(appSettings, _hostingEnvironment);

            // Initialize a new notification
            GetDefaultSettingsNotification notification = new(settings, appSettings, _hostingEnvironment);

            // Publish/broadcast the notification
            _dependencies.EventAggregator.Publish(notification);

            // Return the settings from the notification (an event handler may have replaced it)
            return notification.Settings;

        }

        /// <summary>
        /// Returns a list of models based on default settings, as specified in the <c>appsettings.json</c> file.
        /// </summary>
        /// <returns>An instance of <see cref="TypeModelList"/>.</returns>
        public TypeModelList GetModels() {
            return GetModels(GetDefaultSettings());
        }

        /// <summary>
        /// Returns a list of models based on the specified <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The settings be used when loading the models.</param>
        /// <returns>An instance of <see cref="TypeModelList"/>.</returns>
        public virtual TypeModelList GetModels(ModelsGeneratorSettings settings) {

            // Input validation
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.DefaultModelsPath)) throw new PropertyNotSetException(nameof(settings.DefaultModelsPath));
            if (string.IsNullOrWhiteSpace(settings.DefaultNamespace)) throw new PropertyNotSetException(nameof(settings.DefaultNamespace));

            // Create a list of all types
            List<TypeModel> types = new();
            AppendContentTypes(types, settings);
            AppendMediaTypes(types, settings);
            AppendMemberTypes(types, settings);

            BuildModelRelations(types, settings);

            UpdateModels(types, settings);
            
            OnGetModelsReturning(ref types, settings);

            foreach (TypeModel type in types) {

                // If "Path" has a value at this point, it means the user explicitly set one from an event handler,
                // and if so, we shouldn't overwrite the value
                if (!string.IsNullOrWhiteSpace(type.Path)) continue;

                // 
                List<string> path = new() { settings.DefaultModelsPath };


                if (type.Directories != null) path.AddRange(type.Directories);
                path.Add($"{type.ClrName}.generated.cs");
                type.Path = Path.Combine(path.ToArray());

                //throw new Exception("\r\n" + settings.DefaultModelsPath + "\r\n" + type.Path);

            }

            return new TypeModelList(types);

        }

        /// <summary>
        /// Internal method used for appending all content types to the specified <paramref name="list"/>
        /// </summary>
        /// <param name="list">The list to which the content types should be added.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void AppendContentTypes(List<TypeModel> list, ModelsGeneratorSettings settings) {

            foreach (IContentType contentType in _contentTypeService.GetAll()) {
                AppendContentType(list, contentType, settings);
            }

        }

        /// <summary>
        /// Internal method used for appending a single content type 
        /// </summary>
        /// <param name="list">The list to which the content type should be added.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void AppendContentType(List<TypeModel> list, IContentType contentType, ModelsGeneratorSettings settings) {

            // The published content type let's us get additional information about the content type, so we need to
            // retrieve this as well
            IPublishedContentType pct = _publishedContentTypeFactory.CreateContentType(contentType);

            // Initialize a new type model
            TypeModel type = new(contentType, pct, settings);

            // Run through all the properties and add them to the type model
            foreach (IPropertyType propertyType in contentType.CompositionPropertyTypes) {

                // Get the published property type as well
                IPublishedPropertyType ppt = pct.GetPropertyType(propertyType.Alias);
                if (ppt == null) throw new Exception("Published property type not found.");

                // Append the property model to the type model
                type.Properties.Add(new PropertyModel(propertyType, ppt));

            }

            // Append the type model to the list of types
            list.Add(type);

        }
        
        /// <summary>
        /// Internal method used for appending all member types to the specified <paramref name="list"/>
        /// </summary>
        /// <param name="list">The list to which the member types should be added.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void AppendMemberTypes(List<TypeModel> list, ModelsGeneratorSettings settings) {

            foreach (IMemberType memberType in _memberTypeService.GetAll()) {
                
                // The published content type let's us get additional information about the content type, so we need to
                // retrieve this as well
                IPublishedContentType pct = _publishedContentTypeFactory.CreateContentType(memberType);
                
                // Initialize a new type model
                TypeModel type = new(memberType, pct, settings);
                
                // Run through all the properties and add them to the type model
                foreach (IPropertyType propertyType in memberType.CompositionPropertyTypes) {
                    
                    // Get the published property type as well
                    IPublishedPropertyType ppt = pct.GetPropertyType(propertyType.Alias);
                    if (ppt == null) throw new Exception("Published property type not found.");
                    
                    // Append the property model to the type model
                    type.Properties.Add(new PropertyModel(propertyType, ppt));

                }
                
                // Append the type model to the list of types
                list.Add(type);

            }

        }

        /// <summary>
        /// Internal method used for appending all media types to the specified <paramref name="list"/>
        /// </summary>
        /// <param name="list">The list to which the media types should be added.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void AppendMediaTypes(List<TypeModel> list, ModelsGeneratorSettings settings) {

            foreach (IMediaType mediaType in _mediaTypeService.GetAll()) {
                
                // The published content type let's us get additional information about the content type, so we need to
                // retrieve this as well
                IPublishedContentType pct = _publishedContentTypeFactory.CreateContentType(mediaType);
                
                // Initialize a new type model
                TypeModel type = new(mediaType, pct, settings);
                
                // Run through all the properties and add them to the type model
                foreach (IPropertyType propertyType in mediaType.CompositionPropertyTypes) {
                    
                    // Get the published property type as well
                    IPublishedPropertyType ppt = pct.GetPropertyType(propertyType.Alias);
                    if (ppt == null) throw new Exception("Published property type not found.");
                    
                    // Append the property model to the type model
                    type.Properties.Add(new PropertyModel(propertyType, ppt));

                }
                
                // Append the type model to the list of types
                list.Add(type);

            }

        }

        /// <summary>
        /// Intenral method for building the relations between model types and their parents as well as model types and their compositions.
        /// </summary>
        /// <param name="types">The list of type models.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void BuildModelRelations(List<TypeModel> types, ModelsGeneratorSettings settings) {

            // Create a new dictionary based on the list (foir faster lookups)
            Dictionary<int, TypeModel> lookup = types.ToDictionary(x => x.Id);

            // Build relations between the different types
            foreach (TypeModel type in types) {

                if (type.ContentType.ParentId > 0) {
                    if (lookup.TryGetValue(type.ContentType.ParentId, out TypeModel parentModel)) {
                        type.ParentType = parentModel;
                    }
                }

                IEnumerable<IContentTypeComposition> compositionTypes = type.ContentType.ContentTypeComposition;

                foreach (IContentTypeComposition composition in compositionTypes) {
                    if (!lookup.TryGetValue(composition.Id, out TypeModel typeModel)) continue;
                    type.Compositions.Add(typeModel);
                    typeModel.IsComposition = true;
                }

            }

        }

        /// <summary>
        /// Internal method used for updating the model definitions a bit before they are returned.
        ///
        /// If the <see cref="ModelsGeneratorSettings.UseDirectories"/> option is set to <c>true</c>, this method will
        /// created a nested directory structure to match the type and purpose of the models.
        ///
        /// The method will also ensure that propery types with certain Skybrud property editors are ignored, as they
        /// don't really hold a value or serve a purpose beyond the backoffice.
        /// </summary>
        /// <param name="types">The list of type models.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void UpdateModels(List<TypeModel> types, ModelsGeneratorSettings settings) {

            foreach (TypeModel type in types) {

                if (settings.UseDirectories) {

                    switch (type.Kind) {

                        case ContentTypeKind.Media:
                            type.Directories.Add("Media");
                            type.Namespace += ".Media";
                            break;

                        case ContentTypeKind.Member:
                            type.Directories.Add("Members");
                            type.Namespace += ".Members";
                            break;

                        case ContentTypeKind.Element:
                            if (type.IsComposition) {
                                type.Namespace += ".Compositions";
                                type.Directories.Add("Compositions");
                            } else {
                                type.Directories.Add("Elements");
                                type.Namespace += ".Elements";
                            }
                            break;

                        case ContentTypeKind.Content:
                            type.Directories.Add("Content");
                            type.Namespace += ".Content";
                            if (type.IsComposition) {
                                type.Namespace += ".Compositions";
                                type.Directories.Add("Compositions");
                            }
                            break;

                    }

                }

                foreach (PropertyModel property in type.Properties) {

                    switch (property.EditorAlias) {

                        case "Skybrud.Separator":
                        case "Skybrud.Umbraco.Redirects":
                        case "Skybrud.Umbraco.Redirects.OutboundRedirect":
                            property.IsIgnored = true;
                            break;

                    }

                }

            }

        }

        #endregion

    }

}