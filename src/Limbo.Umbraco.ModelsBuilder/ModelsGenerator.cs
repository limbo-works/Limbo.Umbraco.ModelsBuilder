using Limbo.Umbraco.ModelsBuilder.Composers;
using Limbo.Umbraco.ModelsBuilder.Events;
using Limbo.Umbraco.ModelsBuilder.Models;
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
using Umbraco.Extensions;

namespace Limbo.Umbraco.ModelsBuilder {
    
    public class ModelsGenerator {
        
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IContentTypeService _contentTypeService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
        private readonly IOptions<LimboModelsBuilderSettings> _modelsBuilderSettings;

        public ModelsGenerator(IHostingEnvironment hostingEnvironment, IContentTypeService contentTypeService, IMemberTypeService memberTypeService,
            IMediaTypeService mediaTypeService, IPublishedContentTypeFactory publishedContentTypeFactory,
            IOptions<LimboModelsBuilderSettings> modelsBuilderSettings)
        {
            _hostingEnvironment = hostingEnvironment;
            _contentTypeService = contentTypeService;
            _memberTypeService = memberTypeService;
            _mediaTypeService = mediaTypeService;
            _publishedContentTypeFactory = publishedContentTypeFactory;
            _modelsBuilderSettings = modelsBuilderSettings;
        }

        public event EventHandler<GetModelsEventArgs> GetModelsReturning;

        void OnGetModelsReturning(GetModelsEventArgs args) {
            GetModelsReturning?.Invoke(this, args);
        }

        public ModelsGeneratorSettings GetDefaultSettings() {
            
            ModelsGeneratorSettings settings = new();

            // Get a reference to the ModelsBuilder config section (Limbo style)
            LimboModelsBuilderSettings config = _modelsBuilderSettings.Value;

            // Update the settings instance based on the configuration
            settings.DefaultModelsPath = config.ModelsDirectoryAbsolute(_hostingEnvironment);
            settings.DefaultNamespace = config.ModelsNamespace;
            settings.UseDirectories = config.UseDirectories;

            settings.EditorConfig = new EditorConfigSettings();

            return settings;

        }

        public TypeModelList GetModels() {
            return GetModels(GetDefaultSettings());
        }

        protected virtual void AppendContentTypes(ModelsGeneratorSettings settings, List<TypeModel> types) {

            foreach (IContentType contentType in _contentTypeService.GetAll()) {
                AppendContentType(settings, contentType, types);
            }

        }

        protected virtual void AppendContentType(ModelsGeneratorSettings settings, IContentType contentType, List<TypeModel> types) {

            IPublishedContentType pct = _publishedContentTypeFactory.CreateContentType(contentType);

            TypeModel type = new(contentType, pct, contentType.IsElement ? ContentTypeKind.Element : ContentTypeKind.Content) {
                Namespace = settings.DefaultNamespace
            };

            foreach (IPropertyType propertyType in contentType.CompositionPropertyTypes) {

                IPublishedPropertyType ppt = pct.GetPropertyType(propertyType.Alias);
                if (ppt == null) throw new Exception("Published property type not found.");
                
                type.Properties.Add(new PropertyModel(propertyType, ppt));

            }

            types.Add(type);

        }

        protected virtual void AppendMemberTypes(ModelsGeneratorSettings settings, List<TypeModel> types) {

            foreach (IMemberType memberType in _memberTypeService.GetAll()) {

                IPublishedContentType pct = _publishedContentTypeFactory.CreateContentType(memberType);

                TypeModel type = new(memberType, pct, ContentTypeKind.Member) {
                    Namespace = settings.DefaultNamespace
                };

                foreach (IPropertyType propertyType in memberType.PropertyTypes) {

                    IPublishedPropertyType ppt = pct.GetPropertyType(propertyType.Alias);
                    if (ppt == null) throw new Exception("Published property type not found.");
                    
                    type.Properties.Add(new PropertyModel(propertyType, ppt));

                }

                types.Add(type);

            }

        }

        protected virtual void AppendMediaTypes(ModelsGeneratorSettings settings, List<TypeModel> types) {

            foreach (IMediaType mediaType in _mediaTypeService.GetAll()) {

                IPublishedContentType pct = _publishedContentTypeFactory.CreateContentType(mediaType);

                TypeModel type = new(mediaType, pct, ContentTypeKind.Media) {
                    Namespace = settings.DefaultNamespace
                };

                foreach (IPropertyType propertyType in mediaType.PropertyTypes) {

                    IPublishedPropertyType ppt = pct.GetPropertyType(propertyType.Alias);
                    if (ppt == null) throw new Exception("Published property type not found.");
                    
                    type.Properties.Add(new PropertyModel(propertyType, ppt));

                }

                types.Add(type);

            }

        }

        protected virtual void BuildModelRelations(ModelsGeneratorSettings settings, List<TypeModel> types) {
            
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

        protected virtual void UpdateModels(ModelsGeneratorSettings settings, List<TypeModel> types) {

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

        public TypeModelList GetModels(ModelsGeneratorSettings settings) {
            
            // Input validation
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.DefaultModelsPath)) throw new PropertyNotSetException(nameof(settings.DefaultModelsPath));
            if (string.IsNullOrWhiteSpace(settings.DefaultNamespace)) throw new PropertyNotSetException(nameof(settings.DefaultNamespace));

            // Create a list of all types
            List<TypeModel> types = new();
            AppendContentTypes(settings, types);
            AppendMediaTypes(settings, types);
            AppendMemberTypes(settings, types);

            BuildModelRelations(settings, types);

            UpdateModels(settings, types);

            GetModelsEventArgs args = new() { Types = types };

            OnGetModelsReturning(args);

            foreach (TypeModel type in args.Types) {
                
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

            return new TypeModelList(args.Types);

        }


    }

}