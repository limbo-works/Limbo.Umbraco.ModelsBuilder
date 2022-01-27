using System.Collections.Generic;
using Newtonsoft.Json;
using Skybrud.Essentials.Strings.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace OmgBacon.ModelsBuilder.Models {
    
    public class TypeModel {

        [JsonIgnore]
        public IContentTypeComposition ContentType { get; }

        [JsonIgnore]
        public IPublishedContentType PublishedContentType { get; }

        public ContentTypeKind Kind { get; }

        public int Id => ContentType.Id;

        public string Alias => ContentType.Alias;

        public string Name => ContentType.Name;

        public string Namespace { get; set; }

        public string ClrName { get; set; }

        public bool IsIgnored { get; set; }

        public TypeModel ParentType { get; set; }

        public List<TypeModel> Compositions { get; set; }

        public List<PropertyModel> Properties { get; set; }

        public bool IsComposition { get; set; }

        public bool IsElementType { get; set; }

        public string Path { get; set; }

        public List<string> Directories { get; set; }

        public TypeModel(IContentTypeComposition contentType, IPublishedContentType publishedContentType, ContentTypeKind kind) {
            ContentType = contentType;
            PublishedContentType = publishedContentType;
            Kind = kind;
            ClrName = ContentType.Alias.ToPascalCase();
            Compositions = new List<TypeModel>();
            Properties = new List<PropertyModel>();
            Directories = new List<string>();
            IsElementType = publishedContentType.IsElement;
        }

    }

}