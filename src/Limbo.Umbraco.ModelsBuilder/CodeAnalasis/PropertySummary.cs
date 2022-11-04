using Microsoft.CodeAnalysis.CSharp.Syntax;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis {

    /// <summary>
    /// Class representing a summary about a property.
    /// </summary>
    public class PropertySummary {

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name of the underlying property on the content type if this property has a <see cref="ImplementPropertyTypeAttribute"/>.
        /// </summary>
        public string? ImplementsPropertyType { get; }

        /// <summary>
        /// Initializes a new instance based on the specified property declaration <paramref name="syntax"/>.
        /// </summary>
        /// <param name="syntax">The declaration syntax describing the property.</param>
        public PropertySummary(PropertyDeclarationSyntax syntax) {

            Type = syntax.Type.ToString();
            Name = syntax.Identifier.ToString();

            foreach (AttributeListSyntax hai in syntax.AttributeLists) {
                foreach (var attr in hai.Attributes) {
                    if (attr.ArgumentList == null) continue;
                    if (!string.IsNullOrWhiteSpace(ImplementsPropertyType)) continue;
                    foreach(AttributeArgumentSyntax arg in attr.ArgumentList.Arguments) {
                        if (arg.Expression is LiteralExpressionSyntax lit) {
                            ImplementsPropertyType = lit.Token.Value?.ToString();
                        }
                    }

                }

            }

        }

    }

}