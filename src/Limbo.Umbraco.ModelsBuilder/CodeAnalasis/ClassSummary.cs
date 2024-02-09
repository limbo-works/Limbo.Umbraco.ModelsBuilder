using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis;

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
    public IReadOnlyList<ConstructorSummary> Constructors { get; }

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
    public IReadOnlyList<PropertySummary> Properties { get; }

    /// <summary>
    /// Gets a list of the methods of the class.
    /// </summary>
    public IReadOnlyList<MethodSummary> Methods { get; }

    /// <summary>
    /// Gets a list of aliases for the property types that has been explicitly ignored.
    /// </summary>
    public IReadOnlySet<string> IgnoredPropertyTypes { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="syntax"/>.
    /// </summary>
    /// <param name="syntax">The class declaration syntax describing the class.</param>
    /// <param name="namespaceSyntax">The syntax for the parent namespace.</param>
    public ClassSummary(ClassDeclarationSyntax syntax, BaseNamespaceDeclarationSyntax namespaceSyntax) {

        Name = syntax.Identifier.ToString();
        Namespace = namespaceSyntax.Name.ToString();
        BaseTypes = syntax.BaseList?.Types.Select(x => x.ToString()).ToList() ?? new List<string>();

        HashSet<string> ignoredPropertyTypes = new();
        List<ConstructorSummary> constructors = new();
        List<PropertySummary> properties = new();
        List<MethodSummary> methods = new();

        foreach (AttributeListSyntax atrrList in syntax.AttributeLists) {
            foreach (AttributeSyntax attr in atrrList.Attributes) {
                if (attr.Name.ToString() != "IgnorePropertyType") continue;
                if (attr.ArgumentList == null) continue;
                foreach (AttributeArgumentSyntax arg in attr.ArgumentList.Arguments) {
                    if (arg.Expression is LiteralExpressionSyntax { Token: { Value: { } } } lit) {
                        string alias = lit.Token.Value.ToString()!;
                        if (ignoredPropertyTypes.Contains(alias)) continue;
                        ignoredPropertyTypes.Add(alias);
                    }
                }
            }
        }

        foreach (MemberDeclarationSyntax member in syntax.Members) {

            switch (member) {

                case ConstructorDeclarationSyntax xtor:
                    constructors.Add(new ConstructorSummary(xtor));
                    break;

                case PropertyDeclarationSyntax property:
                    properties.Add(new PropertySummary(property));
                    break;

                case MethodDeclarationSyntax method:
                    methods.Add(new MethodSummary(method));
                    break;

            }

        }

        Source = syntax;
        IgnoredPropertyTypes = ignoredPropertyTypes;
        Constructors = constructors;
        Properties = properties;
        Methods = methods;

    }

    #endregion

    #region Properties

    /// <summary>
    /// Returns whether the class has a property with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The CLR name of the property.</param>
    /// <returns><c>true</c> if the property exist; otherwise, <c>false</c>.</returns>
    public bool HasProperty(string name) {
        return Properties.Any(x => x.Name == name);
    }

    /// <summary>
    /// Returns whether the class has a method with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The CLR name of the method.</param>
    /// <returns><c>true</c> if the method exist; otherwise, <c>false</c>.</returns>
    public bool HasMethod(string name) {
        return Methods.Any(x => x.Name == name);
    }

    #endregion

}