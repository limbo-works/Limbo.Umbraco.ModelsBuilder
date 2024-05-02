using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable PossibleNullReferenceException

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis;

/// <summary>
/// Class representing a summary about a constructor or method parameter.
/// </summary>
public class ParameterSummary {

    /// <summary>
    /// Gets a reference to the <see cref="ParameterSyntax"/> this instance was based on.
    /// </summary>
    public ParameterSyntax Syntax { get; }

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance based on the specified parameter <paramref name="syntax"/>.
    /// </summary>
    /// <param name="syntax">The syntax describing the parameter.</param>
    public ParameterSummary(ParameterSyntax syntax) {
        Syntax = syntax;
        Type = syntax.Type!.ToString();
        Name = syntax.Identifier.ToString();
    }

}