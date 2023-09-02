﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis {

    /// <summary>
    /// Class representing a summary about a C# file.
    /// </summary>
    public class FileSummary {

        #region Properties

        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the usings/imports of the file.
        /// </summary>
        public List<string> Usings { get; }

        /// <summary>
        /// Gets a list with the namespaces of the file. Following .NET convention, a file should really only have one
        /// namespace.
        /// </summary>
        public List<NamespaceSummary> Namespaces { get; }

        /// <summary>
        /// Gets a list with the top-level classes of the file. If you're using block-scoped namespaces, this list will be empty, and you need to use <see cref="Namespaces"/> property instead.
        /// </summary>
        public List<ClassSummary> Classes { get; }

        #endregion

        #region Constructors

        private FileSummary(string path, CompilationUnitSyntax root) {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            Usings = root.Usings.Select(x => x.Name.ToString()).ToList();
            Namespaces = new List<NamespaceSummary>();
            Classes = new List<ClassSummary>();
        }

        #endregion

        #region Member methods

        /// <summary>
        /// Gets the class summary with the specified <paramref name="fullname"/>.
        /// </summary>
        /// <param name="fullname">The full name of the class (namespace and class name).</param>
        /// <param name="result">When this method returns, contains the class with the specified full name, if the class is found; otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the class is found; otherwise, <c>false</c>.</returns>
        public bool TryGetClass(string fullname, [NotNullWhen(true)] out ClassSummary? result) {

            foreach (var ns in Namespaces) {
                foreach (var cs in ns.Classes) {
                    if (cs.FullName == fullname) {
                        result = cs;
                        return true;
                    }
                }
            }

            result = null;
            return false;

        }

        #endregion

        #region Static methods
        
        /// <summary>
        /// Loads the C# file at <paramref name="path"/> and returns a summary about the file.
        /// </summary>
        /// <param name="path">The path to the C# file.</param>
        /// <returns>An instance of <see cref="FileSummary"/>.</returns>
        public static FileSummary Load(string path) {

            SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path, Encoding.UTF8));
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            FileSummary file = new(path, root);

            foreach (BaseNamespaceDeclarationSyntax ns in root.Members.OfType<BaseNamespaceDeclarationSyntax>()) {

                NamespaceSummary nss = new(ns);

                foreach (MemberDeclarationSyntax h in ns.Members) {
                    if (h is not ClassDeclarationSyntax cs) continue;
                    nss.Classes.Add(new ClassSummary(cs, ns));
                }

                file.Namespaces.Add(nss);

            }

            return file;

        }

        /// <summary>
        /// Gets a summary about the C# file at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the C# file.</param>
        /// <param name="summary">When this method returns, contains the summary, if the file is found; otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the file is found; otherwise, <c>false</c>.</returns>
        public static bool TryLoad(string path, [NotNullWhen(true)] out FileSummary? summary) {
            summary = File.Exists(path) ? Load(path) : null;
            return summary != null;
        }

        #endregion

    }

}