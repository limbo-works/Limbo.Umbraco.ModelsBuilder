using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        /// Initializes a new instance based on the specified property declaration <paramref name="syntax"/>.
        /// </summary>
        /// <param name="syntax">The declaration syntax describing the property.</param>
        public PropertySummary(PropertyDeclarationSyntax syntax) {
            Type = syntax.Type.ToString();
            Name = syntax.Identifier.ToString();
        }

    }

}