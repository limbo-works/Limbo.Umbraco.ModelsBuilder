using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis;

/// <summary>
/// Class representing a summary about a namespace.
/// </summary>
public class NamespaceSummary {

    #region Properties

    /// <summary>
    /// Gets a reference to the <see cref="BaseNamespaceDeclarationSyntax"/> this instance was based on.
    /// </summary>
    public BaseNamespaceDeclarationSyntax Syntax { get; }

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
    /// Initializes a new instance based on the specified <paramref name="syntax"/>.
    /// </summary>
    /// <param name="syntax">The syntax describing the namespace.</param>
    public NamespaceSummary(BaseNamespaceDeclarationSyntax syntax) {
        Syntax = syntax;
        Name = syntax.Name.ToString();
        Classes = new List<ClassSummary>();
    }

    #endregion

}