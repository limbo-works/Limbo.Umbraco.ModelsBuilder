using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis {
    
    public class FileSummary {

        public string Path { get; set; }

        public string Name { get; set; }

        public List<string> Usings { get; set; }

        public List<NamespaceSummary> Namespaces { get; set; }

        public bool TryGetClass(string fullname, out ClassSummary result)
        {

            foreach (var ns in Namespaces)
            {
                foreach (var cs in ns.Classes)
                {
                    if (cs.FullName == fullname)
                    {
                        result = cs;
                        return true;
                    }
                }
            }

            result = null;
            return false;

        }

        public static bool TryLoad(string path, out FileSummary file) {
            file = File.Exists(path) ? Load(path) : null;
            return file != null;
        }

        public static FileSummary Load(string path) {

            SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path, Encoding.UTF8));
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            
            FileSummary file = new FileSummary {
                Path = path,
                Name = System.IO.Path.GetFileName(path),
                Usings = root.Usings.Select(x => x.Name.ToString()).ToList(),
                Namespaces = new List<NamespaceSummary>()
            };

            foreach (NamespaceDeclarationSyntax ns in root.Members.OfType<NamespaceDeclarationSyntax>()) {

                NamespaceSummary nss = new NamespaceSummary {
                    Name = ns.Name.ToString(),
                    Classes = new List<ClassSummary>()
                };

                foreach (MemberDeclarationSyntax h in ns.Members) {
                
                    if (h is ClassDeclarationSyntax cs) {

                        ClassSummary summary = new ClassSummary(cs);

                        string className = cs.Identifier.ToString();

                        summary.Namespace = ns.Name.ToString();

                        summary.Name = className;

                        summary.BaseTypes = cs.BaseList?.Types.Select(x => x.ToString()).ToList() ?? new List<string>();

                        nss.Classes.Add(summary);

                    }

                }

                file.Namespaces.Add(nss);

            }

            return file;

        }

    }
}