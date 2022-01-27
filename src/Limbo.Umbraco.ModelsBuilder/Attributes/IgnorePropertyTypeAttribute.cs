using System;

namespace Limbo.Umbraco.ModelsBuilder.Attributes {

    /// <summary>
    /// Attribute which can be used to ignore a single property type. The attribute may be added more than once to
    /// ignore more than one property type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IgnorePropertyTypeAttribute : Attribute {

        /// <summary>
        /// Gets the alias of the ignored property type.
        /// </summary>
        public string PropertyAlias { get; }

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="propertyAlias"/>.
        /// </summary>
        /// <param name="propertyAlias">The alias of the property type.</param>
        public IgnorePropertyTypeAttribute(string propertyAlias) {
            PropertyAlias = propertyAlias;
        }

    }

}