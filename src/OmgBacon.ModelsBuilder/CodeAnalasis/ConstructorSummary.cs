using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OmgBacon.ModelsBuilder.CodeAnalasis
{
    public class ConstructorSummary
    {

        public ParameterSummary[] Parameters { get; }
        
        public ConstructorSummary(ConstructorDeclarationSyntax constructor) {
            Parameters = constructor.ParameterList.Parameters.Select(x => new ParameterSummary(x)).ToArray();
        }

    }
}