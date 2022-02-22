using Limbo.Umbraco.ModelsBuilder.Models.Json;
using Skybrud.Essentials.Strings.Extensions;
using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Limbo.Umbraco.ModelsBuilder.Models {

    /// <summary>
    /// Class representing a property.
    /// </summary>
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

        /// <summary>
        /// Gets or sets whether a static getter method should be added for this property. Default is <see cref="PropertyStaticMethod.Auto"/>.
        /// </summary>
        public PropertyStaticMethod StaticMethod { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="propertyType"/> and <paramref name="publishedPropertyType"/>.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <param name="publishedPropertyType">The published property type.</param>
        /// <remarks><see cref="IPropertyType"/> and <see cref="IPublishedPropertyType"/> holds different kinds of information which is why we need them both.</remarks>
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