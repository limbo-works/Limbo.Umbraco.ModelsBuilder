using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis
{
    public class ParameterSummary {

        public string Type { get; }

        public string Name { get; }
        
        public ParameterSummary(ParameterSyntax parameter) {
            Type = parameter.Type.ToString();
            Name = parameter.Identifier.ToString();
        }

    }
}