using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis;

/// <summary>
/// Class representing a summary about a constructor.
/// </summary>
public class ConstructorSummary {

    /// <summary>
    /// Gets a reference to the <see cref="ConstructorDeclarationSyntax"/> this instance was based on.
    /// </summary>
    public ConstructorDeclarationSyntax Syntax { get; }

    /// <summary>
    /// Gets an array of the parameters of the constructor.
    /// </summary>
    public IReadOnlyList<ParameterSummary> Parameters { get; }

    /// <summary>
    /// Initializes a new instance based on the specified constructor declaration <paramref name="syntax"/>.
    /// </summary>
    /// <param name="syntax">The declaration syntax describing the constructor.</param>
    public ConstructorSummary(ConstructorDeclarationSyntax syntax) {
        Syntax = syntax;
        Parameters = syntax.ParameterList.Parameters.Select(x => new ParameterSummary(x)).ToArray();
    }

}