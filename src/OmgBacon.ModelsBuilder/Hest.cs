using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OmgBacon.ModelsBuilder {
    
    public static class Hest {

        public static CompilationUnitSyntax Load(string path) {

            SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path));
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            return root;

        }

    }

}