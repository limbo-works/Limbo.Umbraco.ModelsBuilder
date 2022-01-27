using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis {

    /// <summary>
    /// Class representing a summary about a constructor.
    /// </summary>
    public class ConstructorSummary {

        /// <summary>
        /// Gets an array of the parameters of the constructor.
        /// </summary>
        public ParameterSummary[] Parameters { get; }

        /// <summary>
        /// Initializes a new instance based on the specified constructor declaration <paramref name="syntax"/>.
        /// </summary>
        /// <param name="syntax">The declaration syntax describing the constructor.</param>
        public ConstructorSummary(ConstructorDeclarationSyntax syntax) {
            Parameters = syntax.ParameterList.Parameters.Select(x => new ParameterSummary(x)).ToArray();
        }

    }

}