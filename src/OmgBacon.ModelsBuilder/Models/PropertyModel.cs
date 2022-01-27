using System;
using OmgBacon.ModelsBuilder.Models.Json;
using Skybrud.Essentials.Strings.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace OmgBacon.ModelsBuilder.Models {

    public class PropertyModel {

        #region Properties

        /// <summary>
        /// Gets the alias of the property.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Gets or sets friendly name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the CLR name of the property.
        /// </summary>
        public string ClrName { get; set; }

        /// <summary>
        /// Gets or sets whether this property should be ignored in the generated model.
        /// </summary>
        public bool IsIgnored { get; set; }

        /// <summary>
        /// Gets or sets the CLR value type of the property.
        /// </summary>
        public Type ValueType { get; set; }

        /// <summary>
        /// Gets or sets the JSON.net settings for this property.
        /// </summary>
        public JsonNetPropertySettings JsonNetSettings { get; set; }

        /// <summary>
        /// Gets the alias of the property editor associated with this property.
        /// </summary>
        public string EditorAlias { get; }
        
        /// <summary>
        /// Gets a reference to the <see cref="PublishedDataType"/> associated with this property.
        /// </summary>
        public PublishedDataType DataType { get; }

        #endregion

        #region Constructors

        public PropertyModel(IPropertyType propertyType, IPublishedPropertyType publishedPropertyType) {

            Alias = propertyType.Alias;
            Name = propertyType.Name;
            ClrName = propertyType.Alias.ToPascalCase();
            ValueType = publishedPropertyType.ModelClrType;

            JsonNetSettings = new JsonNetPropertySettings();

            EditorAlias = publishedPropertyType.EditorAlias;
            DataType = publishedPropertyType.DataType;

        }

        #endregion

    }

}