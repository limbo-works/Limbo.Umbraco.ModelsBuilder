using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace OmgBacon.ModelsBuilder.CodeAnalasis {

    public class ClassSummary {

        [JsonIgnore]
        public ClassDeclarationSyntax Source { get; }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public string FullName => $"{Namespace}.{Name}".Trim('.');

        public List<string> BaseTypes { get; set; }

        public ConstructorSummary[] Constructors { get; }

        public bool HasPublishedContentConstructor {
            get {
                return Constructors.Any(x => x.Parameters.Length == 2 && x.Parameters[0].Type == "IPublishedContent" && x.Parameters[1].Type == "IPublishedValueFallback");
            }
        }

        public ClassSummary(ClassDeclarationSyntax source) {

            Source = source;

            Constructors = source.Members.OfType<ConstructorDeclarationSyntax>().Select(x => new ConstructorSummary(x)).ToArray();

        }

    }

}