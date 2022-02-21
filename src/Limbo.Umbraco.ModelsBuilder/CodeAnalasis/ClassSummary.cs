using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis {

    /// <summary>
    /// Class representing a summary about a C# class.
    /// </summary>
    public class ClassSummary {

        #region Properties

        /// <summary>
        /// Gets a reference to the underlying <see cref="ClassDeclarationSyntax"/>.
        /// </summary>
        [JsonIgnore]
        public ClassDeclarationSyntax Source { get; }

        /// <summary>
        /// Gets the namespace of the class.
        /// </summary>
        public string Namespace { get; set; }
        
        /// <summary>
        /// Gets the name of the class.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets the full name of the class.
        /// </summary>
        public string FullName => $"{Namespace}.{Name}".Trim('.');

        /// <summary>
        /// Gets a list of the base types of the class.
        /// </summary>
        public List<string> BaseTypes { get; set; }
        
        /// <summary>
        /// Gets a list of the constructors of the class.
        /// </summary>
        public ConstructorSummary[] Constructors { get; }

        /// <summary>
        /// Gets whether the class has a suitable constructor for ModelsBuilder.
        /// </summary>
        public bool HasPublishedContentConstructor {
            get {
                return Constructors.Any(x => x.Parameters.Length == 2 && x.Parameters[0].Type == "IPublishedContent" && x.Parameters[1].Type == "IPublishedValueFallback");
            }
        }
        
        /// <summary>
        /// Gets a list of the properties of the class.
        /// </summary>
        public PropertySummary[] Properties { get; }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="syntax"/>.
        /// </summary>
        /// <param name="syntax">The class declaration syntax describing the class.</param>
        public ClassSummary(ClassDeclarationSyntax syntax) {
            Source = syntax;
            Constructors = syntax.Members
                .OfType<ConstructorDeclarationSyntax>()
                .Select(x => new ConstructorSummary(x))
                .ToArray();
            Properties = syntax.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(x => new PropertySummary(x))
                .ToArray();
        }

        #endregion

        #region Properties

        public bool HasProperty(string name) {
            return Properties.Any(x => x.Name == name);
        }

        #endregion

    }

}