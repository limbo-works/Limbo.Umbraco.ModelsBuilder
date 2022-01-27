using Limbo.Umbraco.ModelsBuilder.Settings;
using Newtonsoft.Json;
using Skybrud.Essentials.Strings.Extensions;
using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Limbo.Umbraco.ModelsBuilder.Models {
    
    /// <summary>
    /// Class representing a type.
    /// </summary>
    public class TypeModel {

        #region Properties

        /// <summary>
        /// Gets a reference to the underlying <see cref="IContentTypeComposition"/> the type was based on.
        /// </summary>
        [JsonIgnore]
        public IContentTypeComposition ContentType { get; }
        
        /// <summary>
        /// Gets a reference to the underlying <see cref="IPublishedContentType"/> the type was based on.
        /// </summary>
        [JsonIgnore]
        public IPublishedContentType PublishedContentType { get; }

        /// <summary>
        /// Gets the kind of the model - eg. <see cref="ContentTypeKind.Element"/>.
        /// </summary>
        public ContentTypeKind Kind { get; }

        /// <summary>
        /// Gets the numeric ID of the underlying content type.
        /// </summary>
        public int Id => ContentType.Id;

        /// <summary>
        /// Gets the GUID key of the underlying content type.
        /// </summary>
        public Guid Key => ContentType.Key;
        
        /// <summary>
        /// Gets the alias of the content type.
        /// </summary>
        public string Alias => ContentType.Alias;
        
        /// <summary>
        /// Gets the friendly name of the content type.
        /// </summary>
        public string Name => ContentType.Name;

        /// <summary>
        /// Gets or sets the desired namespace of the generated model.
        /// </summary>
        public string Namespace { get; set; }
        
        /// <summary>
        /// Gets or sets the CLR name of the generated model.
        /// </summary>
        public string ClrName { get; set; }

        /// <summary>
        /// Gets or sets whether the model should be ignored.
        /// </summary>
        public bool IsIgnored { get; set; }

        /// <summary>
        /// Gets or sets the parent model.
        /// </summary>
        public TypeModel ParentType { get; set; }

        /// <summary>
        /// Gets or sets the compositions of the model.
        /// </summary>
        public List<TypeModel> Compositions { get; set; }

        /// <summary>
        /// Gets or sets the properties of the model.
        /// </summary>
        public List<PropertyModel> Properties { get; set; }

        /// <summary>
        /// Gets or sets whether the type is a composition.
        /// </summary>
        public bool IsComposition { get; set; }
        
        /// <summary>
        /// Gets or sets whether the type is an element type.
        /// </summary>
        public bool IsElementType { get; set; }

        /// <summary>
        /// Gets or sets the desired path to the generated model. If left empty, the path will automatically be
        /// determined when generating the model.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a list of directories for the model. The directories can be used to nest the model in sub
        /// directories relatively to <see cref="ModelsGeneratorSettings.DefaultModelsPath"/>.
        /// </summary>
        public List<string> Directories { get; set; }

        #endregion

        #region Member methods

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="contentType"/> and <paramref name="publishedContentType"/>.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="publishedContentType">The published content type.</param>
        /// <param name="settings">The models generator settings.</param>
        public TypeModel(IContentTypeComposition contentType, IPublishedContentType publishedContentType, ModelsGeneratorSettings settings) {
            ContentType = contentType;
            PublishedContentType = publishedContentType;
            ClrName = ContentType.Alias.ToPascalCase();
            Compositions = new List<TypeModel>();
            Properties = new List<PropertyModel>();
            Directories = new List<string>();
            IsElementType = publishedContentType.IsElement;
            Namespace = settings.DefaultNamespace;
            Kind = contentType switch {
                IMemberType => ContentTypeKind.Member,
                IMediaType => ContentTypeKind.Media,
                IContentType => contentType.IsElement ? ContentTypeKind.Element : ContentTypeKind.Content,
                _ => throw new Exception($"Unsupported type: {contentType.GetType()}")
            };
        }

        #endregion

    }

}