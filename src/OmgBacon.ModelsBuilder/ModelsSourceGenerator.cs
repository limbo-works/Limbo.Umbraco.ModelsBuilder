using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using OmgBacon.ModelsBuilder.Attributes;
using OmgBacon.ModelsBuilder.CodeAnalasis;
using OmgBacon.ModelsBuilder.Models;
using Skybrud.Essentials.Reflection;
using Umbraco.Cms.Core.Models.PublishedContent;

// ReSharper disable PatternAlwaysOfType
// ReSharper disable AssignNullToNotNullAttribute

namespace OmgBacon.ModelsBuilder {
    
    public class ModelsSourceGenerator {
        
        protected Dictionary<string, string> SimpleNames => new() {
            {"System.String", "string" },
            {"System.Boolean", "bool" },
            {"System.Int32", "int" },
            {"System.Int64", "long" },
            {"System.Decimal", "decimal" },
            {"System.Double", "double" },
            {"System.Single", "float" },
            {"System.Object", "object" }
        };

        #region Member methods

        /// <summary>
        /// Saves the specified collection of <paramref name="models"/> to their respective locations on the disk.
        /// </summary>
        /// <param name="models">A collection with the models to be saved.</param>
        /// <param name="settings">The models generator settings.</param>
        public virtual void SaveModels(IEnumerable<TypeModel> models, ModelsGeneratorSettings settings)  {
            
            // Create a new list for the models so we can quickly look them up later
            TypeModelList list = models as TypeModelList ?? new TypeModelList(models);

            foreach (TypeModel model in list) {
                
                // Skip types that have been explicitly ignored
                if (model.IsIgnored) continue;

                // Generate the C# source code for the model
                string source = GetSource(model, list, settings);

                Directory.CreateDirectory(Path.GetDirectoryName(model.Path));
                File.WriteAllText(model.Path, source, Encoding.UTF8);

            }

        }

        /// <summary>
        /// Gets the name of the specified <paramref name="type"/> .
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        public virtual string GetValueTypeName(TypeModel model, Type type, TypeModelList models) {

            if (type is ModelType modelType) {
                if (models.TryGetModel(modelType.ContentTypeAlias, out var actualTypeName)) {
                    return actualTypeName.Namespace == model.Namespace ? string.Empty : actualTypeName.Namespace + "." + actualTypeName.ClrName;
                }
                throw new InvalidOperationException($"Don't know how to map ModelType with content type alias \"{modelType.ContentTypeAlias}\".");
            }

            if (type.FullName != null && SimpleNames.TryGetValue(type.FullName, out string simpleName)) {
                return simpleName;
            }

            string ns;

            switch (type.Namespace) {

                //case "System.Collections.Generic":
                case "Umbraco.Core.Models.PublishedContent":
                    ns = string.Empty;
                    break;

                default:
                    ns = $"{type.Namespace}.";
                    break;

            }

            if (ns.StartsWith("Umbraco.")) {
                ns = "global::" + ns;
            }

            if (type.GenericTypeArguments.Length == 0) {
                return $"{ns}{type.Name}";
            }

            return $"{ns}{type.Name.Split('`')[0]}<{string.Join(",", from t in type.GenericTypeArguments select GetValueTypeName(model, t, models))}>";

        }

        /// <summary>
        /// Returns the CLR type of the specified <paramref name="model"/>, or <c>null</c> if not found.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>An instance of <see cref="Type"/>, or <c>null</c>.</returns>
        protected virtual Type GetClrType(TypeModel model) {

            // Get the full name of the CLR type
            string fullname = $"{model.Namespace}.{model.ClrName}";

            // Find the CLR type
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Reverse()
                .Select(assembly => assembly.GetType(fullname))
                .FirstOrDefault(tt => tt != null);

        }

        /// <summary>
        /// Returns a hash set of the property types that should be ignored for the specified <paramref name="model"/>.
        ///
        /// By default, the ignored property types are determined based on the <c>[IgnorePropertyType]</c> and
        /// <c>[IgnorePropertyTypes]</c> attributes of the CLR type.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>An instance of <see cref="HashSet{String}"/> containing the aliases of the ignored property types.</returns>
        protected virtual HashSet<string> GetIgnoredPropertyTypes(TypeModel model) {

            // TODO: Check parent types and compositions

            Type type = GetClrType(model);
            if (type == null) return new HashSet<string>();

            List<string> temp = new();

            foreach (var attr in type.GetCustomAttributes<IgnorePropertyTypeAttribute>()) {
                if (string.IsNullOrWhiteSpace(attr.PropertyName)) continue;
                temp.Add(attr.PropertyName);
            }

            foreach (var attr in type.GetCustomAttributes<IgnorePropertyTypesAttribute>()) {
                foreach (string propertyName in attr.PropertyNames) {
                    if (string.IsNullOrWhiteSpace(propertyName)) continue;
                    temp.Add(propertyName);
                }
            }

            return new HashSet<string>(temp);

        }

        /// <summary>
        /// Returns a list with the default import to be added to the top of each generated class file.
        /// </summary>
        /// <returns>An instance of <see cref="List{String}"/>.</returns>
        protected virtual List<string> GetDefaultImports() {
            return new() {
                "Umbraco.Cms.Core.Models.PublishedContent",
                "Umbraco.Cms.Infrastructure.ModelsBuilder",
                "Umbraco.Extensions"
            };
        }

        public virtual string GetSource(TypeModel model, TypeModelList models, ModelsGeneratorSettings settings) {
            
            string customPartialPath = model.Path.Replace(".generated.cs", ".cs");

            ClassSummary partialClass = null;
            if (FileSummary.TryLoad(customPartialPath, out FileSummary summary)) {
                summary.TryGetClass($"{model.Namespace}.{model.ClrName}", out partialClass);
            }

            List<string> imports = GetDefaultImports();
            List<string> inherits = new();
            
            // If the partial class already has a base type, we shouldn't try adding one
            string partialBaseType = partialClass?.BaseTypes.FirstOrDefault(x => !Regex.IsMatch(x, "^I[A-Z]"));

            // Make sure a proper base class is appended to the inherits - unless the custom partial already does this
            if (string.IsNullOrWhiteSpace(partialBaseType)) {
                if (model.ParentType != null) {
                    inherits.Add(model.ParentType.ClrName);
                    if (model.ParentType.Namespace != model.Namespace) {
                        imports.Add(model.ParentType.Namespace);
                    }
                } else {
                    inherits.Add(model.IsElementType ? "PublishedElementModel" : "PublishedContentModel");
                }
            }

            // Ensure compositions are added to the inherits (when we need to)
            foreach (TypeModel composition in model.Compositions) {

                // Get the CLR name of the interface
                string clrName = $"I{composition.ClrName}";

                // If the model has a custom partial which already implements the composition interface, we don't add
                // it again here
                if (partialClass != null && partialClass.BaseTypes.Contains(clrName)) continue;

                // Append the inteface to the list of inherits
                inherits.Add(clrName);

                // If the composition is in a different namespace, we add that namespace to the imports
                if (composition.Namespace != model.Namespace) imports.Add(composition.Namespace);

            }
            
            // If the type is a composition, the generated class should implement the composition's interface
            if (model.IsComposition) inherits.Add($"I{model.ClrName}");

            // Ignored property types is a bit special, at we can check the attributes of the CLR type. The attributes
            // are not likely to be specified on the generated class, but whether they have been added on the generated
            // class or the custom partial doesn't really matter.
            HashSet<string> ignoredPropertyTypes = GetIgnoredPropertyTypes(model);

            // The [ImplementPropertyType] attribute works a bit differennt in the way that if we get the attributes
            // properties from the CLR type, we can't know whether the property/attribute is defined in the gnerated
            // class or the custom partial (at least I don't think we can), so we need another approach for these





            StringBuilder sb = new();
            using TextWriter writer = new StringWriter(sb);

            WriteFileStart(writer, model, settings);

            WriteImports(writer, model, imports, settings);
            WriteNamespaceStart(writer, model, settings);

            WriteCompositionInterface(writer, model, ignoredPropertyTypes, models, settings);

            WriteClassStart(writer, model, inherits, partialClass, settings);

            WriteConstructor(writer, model, partialClass, settings);

            WriteProperties(writer, model, ignoredPropertyTypes, models, settings);

            WriteClassEnd(writer, model, settings);

            WriteNamespaceEnd(writer, model, settings);

            return sb.ToString().Trim();

        }
        
        protected virtual void WriteFileStart(TextWriter writer, TypeModel model, ModelsGeneratorSettings settings) {

            Assembly assembly = typeof(ModelsSourceGenerator).Assembly;

            string name = assembly.GetName().Name;

            string version = ReflectionUtils.GetInformationalVersion(assembly);
            
            writer.WriteLine("//------------------------------------------------------------------------------");
            writer.WriteLine("// <auto-generated>");
            writer.WriteLine("//");
            writer.WriteLine("//    " + name + " v" + version);
            writer.WriteLine("//");
            writer.WriteLine("//   Changes to this file will be lost if the code is regenerated.");
            writer.WriteLine("// </auto-generated>");
            writer.WriteLine("//------------------------------------------------------------------------------");
            writer.WriteLine();

        }

        protected virtual void WriteImports(TextWriter writer, TypeModel model, List<string> imports, ModelsGeneratorSettings settings) {

            if (imports.Count == 0) return;

            foreach (var import in imports.Distinct().OrderBy(x => x)) {
                writer.WriteLine($"using {import};");
            }

            writer.WriteLine();

        }

        protected virtual void WriteNamespaceStart(TextWriter writer, TypeModel model, ModelsGeneratorSettings settings) {
            writer.WriteLine("namespace " + model.Namespace + " {");
            writer.WriteLine();
        }

        protected virtual void WriteNamespaceEnd(TextWriter writer, TypeModel model, ModelsGeneratorSettings settings) {
            writer.Write('}');
        }

        protected virtual void WriteCompositionInterface(TextWriter writer, TypeModel model, HashSet<string> ignoredPropertyTypes, TypeModelList models, ModelsGeneratorSettings settings) {
            
            // Interface is only needed for composition types
            if (!model.IsComposition) return;

            string name = model.ClrName;
            string baseType = model.IsElementType ? "IPublishedElement" : "IPublishedContent";

            writer.WriteLine($"    public partial interface I{name} : {baseType} {{");
            writer.WriteLine();

            foreach (PropertyModel property in model.Properties) {

                // Skip the property as it has been ignored by the user
                if (property.IsIgnored) continue;

                // Skip the property as it has been ignored via the custom partial
                if (ignoredPropertyTypes.Contains(property.Alias)) continue;

                // Get the name of the property's value type
                string valueTypeName = GetValueTypeName(model, property.ValueType, models);
                    
                writer.WriteLine($"        {valueTypeName} {property.ClrName} {{ get; }}");
                writer.WriteLine();

            }

            writer.WriteLine("    }");
            writer.WriteLine();

        }

        protected virtual void WriteClassStart(TextWriter writer, TypeModel model, List<string> inherits, ClassSummary partialClass, ModelsGeneratorSettings settings) {
            
            
            //writer.WriteLine($"    [PublishedModel(\"{model.Alias}\")]");
            writer.WriteLine($"    public partial class {model.ClrName}{(inherits.Any() ? " : " + string.Join(", ", inherits) : "")} {{");
            writer.WriteLine();

        }

        protected virtual void WriteClassEnd(TextWriter writer, TypeModel model, ModelsGeneratorSettings settings) {
            string indent = "".PadLeft(1 * settings.EditorConfig.IndentSize, ' ');
            writer.Write(indent);
            writer.WriteLine('}');
            writer.WriteLine();
        }

        protected virtual void WriteConstructor(TextWriter writer, TypeModel model, ClassSummary partialClass, ModelsGeneratorSettings settings) {
            
            // If there is a custom partial for the model, and it already has a constructor with the require signature,
            // we shouldn't add one to the generated partial
            if (partialClass is { HasPublishedContentConstructor: true }) return;

            // Get the type of the first parameter
            string t = model.IsElementType ? "IPublishedElement" : "IPublishedContent";
            
            writer.WriteLine($"        public {model.ClrName}({t} content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback) {{ }}");
            writer.WriteLine();

        }

        protected virtual void WriteProperties(TextWriter writer, TypeModel model, HashSet<string> ignoredPropertyTypes, TypeModelList models, ModelsGeneratorSettings settings) {

            foreach (var property in model.Properties) {

                WriteProperty(writer, model, property, ignoredPropertyTypes, models, settings);

            }

        }

        protected virtual void WriteProperty(TextWriter writer, TypeModel model, PropertyModel property, HashSet<string> ignoredPropertyTypes, TypeModelList models, ModelsGeneratorSettings settings) {

            if (property.IsIgnored) return;

            if (ignoredPropertyTypes.Contains(property.Alias)) return;
                
            string valueTypeName = GetValueTypeName(model, property.ValueType, models);

            WriteJsonNetPropertySettings(writer, model, property, settings);

            writer.WriteLine("        [ImplementPropertyType(\"" + property.Alias + "\")]");
            writer.WriteLine("        public " + valueTypeName + " " + property.ClrName + " => this.Value<" + valueTypeName + ">(\"" + property.Alias + "\");");
            writer.WriteLine();

        }

        protected virtual void WriteJsonNetPropertySettings(TextWriter writer, TypeModel model, PropertyModel property, ModelsGeneratorSettings settings) {

            var json = property.JsonNetSettings;
            var editor = settings.EditorConfig;
            if (json == null) return;

            string indent = "".PadLeft(2 * editor.IndentSize, ' ');

            if (json.Ignore) {
                writer.Write(indent);
                writer.WriteLine("[Newtonsoft.Json.JsonIgnore]");
                return;
            }

            List<string> hej = new();
            
            if (!string.IsNullOrWhiteSpace(json.PropertyName)) {
                hej.Add($"\"{json.PropertyName}\"");
            }

            if (json.Order != null) { hej.Add($"Order = {json.Order}"); }
            if (json.NullValueHandling != default) { hej.Add($"NullValueHandling = Newtonsoft.Json.NullValueHandling.{json.NullValueHandling}"); }
            if (json.DefaultValueHandling != default) { hej.Add($"DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.{json.DefaultValueHandling}"); }

            if (!hej.Any()) return;

            writer.Write(indent);
            writer.WriteLine("[Newtonsoft.Json.JsonProperty(" + string.Join(", ", hej) + ")]");

        }

        #endregion

    }

}