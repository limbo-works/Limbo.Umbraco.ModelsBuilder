using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis {
    
    /// <summary>
    /// Class representing a summary about a method.
    /// </summary>
    public class MethodSummary {
        
        /// <summary>
        /// Gets a reference to the <see cref="MethodDeclarationSyntax"/> this summary is about.
        /// </summary>
        public MethodDeclarationSyntax Syntax { get; }
        
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Initializes a new instance based on the specified method declaration <paramref name="syntax"/>.
        /// </summary>
        /// <param name="syntax">The declaration syntax describing the method.</param>
        public MethodSummary(MethodDeclarationSyntax syntax) {
            Syntax = syntax;
            Name = syntax.Identifier.Text;
        }

    }

}