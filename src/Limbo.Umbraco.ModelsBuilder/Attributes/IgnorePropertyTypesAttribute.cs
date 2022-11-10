using System;
using System.Collections.Generic;

namespace Limbo.Umbraco.ModelsBuilder.Attributes {

    /// <summary>
    /// Attribute which can be used to ignore multiple property types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IgnorePropertyTypesAttribute : Attribute {

        /// <summary>
        /// Gets the aliases of the ignored property types.
        /// </summary>
        public IReadOnlyList<string> PropertyAliases { get; }

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="propertyAlias"/>.
        /// </summary>
        /// <param name="propertyAlias">The alias of the property type.</param>
        public IgnorePropertyTypesAttribute(string propertyAlias) {
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentNullException(nameof(propertyAlias));
            PropertyAliases = new[] { propertyAlias };
        }

        /// <summary>
        /// Initializes a new instance based on <paramref name="propertyAlias1"/> and <paramref name="propertyAlias2"/>.
        /// </summary>
        /// <param name="propertyAlias1">The alias of the first property type.</param>
        /// <param name="propertyAlias2">The alias of the second property type.</param>
        public IgnorePropertyTypesAttribute(string propertyAlias1, string propertyAlias2) {
            if (string.IsNullOrWhiteSpace(propertyAlias1)) throw new ArgumentNullException(nameof(propertyAlias1));
            if (string.IsNullOrWhiteSpace(propertyAlias2)) throw new ArgumentNullException(nameof(propertyAlias2));
            PropertyAliases = new[] { propertyAlias1, propertyAlias2 };
        }

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="propertyAliases"/>.
        /// </summary>
        /// <param name="propertyAliases">The aliases of the property types to be ignored.</param>
        public IgnorePropertyTypesAttribute(params string[] propertyAliases) {
            PropertyAliases = propertyAliases;
        }

    }

}