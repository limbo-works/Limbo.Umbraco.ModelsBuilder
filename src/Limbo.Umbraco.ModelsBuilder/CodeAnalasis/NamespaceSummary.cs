using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis {

    /// <summary>
    /// Class representing a summary about a namespace.
    /// </summary>
    public class NamespaceSummary {

        #region Properties

        /// <summary>
        /// Gets the name of the namespace.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a list of the classes in the namespace.
        /// </summary>
        public List<ClassSummary> Classes { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="namespaceDeclarationSyntax"/>.
        /// </summary>
        /// <param name="namespaceDeclarationSyntax">The syntax describing the namespace.</param>
        public NamespaceSummary(BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax) {
            Name = namespaceDeclarationSyntax.Name.ToString();
            Classes = new List<ClassSummary>();
        }

        #endregion

    }

}