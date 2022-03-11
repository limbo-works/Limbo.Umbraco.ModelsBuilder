using Limbo.Umbraco.ModelsBuilder.Attributes;
using Limbo.Umbraco.ModelsBuilder.CodeAnalasis;
using Limbo.Umbraco.ModelsBuilder.Extensions;
using Limbo.Umbraco.ModelsBuilder.Models;
using Limbo.Umbraco.ModelsBuilder.Models.Generator;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.Extensions.Options;
using Skybrud.Essentials.Reflection;
using Skybrud.Essentials.Time;
using Skybrud.Essentials.Time.Iso8601;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Extensions;

#pragma warning disable 1591

// ReSharper disable PatternAlwaysOfType
// ReSharper disable AssignNullToNotNullAttribute

namespace Limbo.Umbraco.ModelsBuilder.Services {

    /// <summary>
    /// Primary class servering as the models source generator.
    /// </summary>
    public class ModelsSourceGenerator {
        
        private readonly LimboModelsBuilderSettings _modelsBuilderSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly OutOfDateModelsStatus _outOfDateModels;

        /// <summary>
        /// Gets or sets a map of simple types.
        /// </summary>
        protected Dictionary<string, string> SimpleNames => new() {
            { "System.String", "string" },
            { "System.Boolean", "bool" },
            { "System.Int32", "int" },
            { "System.Int64", "long" },
            { "System.Decimal", "decimal" },
            { "System.Double", "double" },
            { "System.Single", "float" },
            { "System.Object", "object" }
        };

        #region Constructors

        public ModelsSourceGenerator(IHostingEnvironment hostingEnvironment,
            IOptions<LimboModelsBuilderSettings> modelsBuilderSettings, OutOfDateModelsStatus outOfDateModels) {
            _modelsBuilderSettings = modelsBuilderSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _outOfDateModels = outOfDateModels;
        }

        #endregion

        #region Member methods

        /// <summary>
        /// Saves the specified collection of <paramref name="models"/> to their respective locations on the disk.
        /// </summary>
        /// <param name="models">A collection with the models to be saved.</param>
        /// <param name="settings">The models generator settings.</param>
        public virtual void SaveModels(IEnumerable<TypeModel> models, ModelsGeneratorSettings settings) {

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

            // Update the file on disk
            SaveLastBuildDate();

            // Clear the file on disk
            _outOfDateModels.Clear();

        }

        /// <summary>
        /// Gets the name of the specified <paramref name="type"/> .
        /// </summary>
        /// <param name="model">The model who's name to return.</param>
        /// <param name="type">The type.</param>
        /// <param name="models">A list of all models.</param>
        /// <returns>A string representing the name of the model.</returns>
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
                if (string.IsNullOrWhiteSpace(attr.PropertyAlias)) continue;
                temp.Add(attr.PropertyAlias);
            }

            foreach (var attr in type.GetCustomAttributes<IgnorePropertyTypesAttribute>()) {
                foreach (string propertyName in attr.PropertyAliases) {
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

        /// <summary>
        /// Returns the C# source code for the specified <paramref name="model"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="models">A list of all the models.</param>
        /// <param name="settings">The models generator settings.</param>
        /// <returns>A string with the generated C# source code.</returns>
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

            // Ignored property types is a bit special, as we can check the attributes of the CLR type. The attributes
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
            
            WritePragma(writer, model, settings);

            WriteNamespaceStart(writer, model, settings);

            WriteCompositionInterface(writer, model, ignoredPropertyTypes, models, settings);

            WriteClassStart(writer, model, inherits, partialClass, settings);

            WriteConstants(writer, model, settings);

            WriteConstructor(writer, model, partialClass, settings);

            WriteProperties(writer, model, ignoredPropertyTypes, partialClass, models, settings);

            WriteStaticMethods(writer, model, ignoredPropertyTypes, partialClass, models, settings);

            WriteClassEnd(writer, model, settings);

            WriteExtensionMethodsClass(writer, model, ignoredPropertyTypes, partialClass, models, settings);

            WriteNamespaceEnd(writer, model, settings);

            return sb.ToString().Trim();

        }

        private void WriteExtensionMethodsClass(TextWriter writer, TypeModel model, HashSet<string> ignoredPropertyTypes, ClassSummary partialClass, TypeModelList models, ModelsGeneratorSettings settings) {

            List<GeneratorExtensionMethod> extensionMethods = new();
                
            // Determine the class name
            string className = $"{(model.IsComposition ? "I" : string.Empty)}{model.ClrName}";

            // Iterate through the properties
            foreach (PropertyModel property in model.Properties) {
                
                // Skip the property if it already has been flagged as ignored
                if (property.IsIgnored || ignoredPropertyTypes.Contains(property.Alias)) continue;
            
                // If the model has a custom partial class, and the property has been manually added there, we shouldn't add it here
                if (partialClass != null && partialClass.HasProperty(property.ClrName)) continue;

                // Get the name of the property's value type
                string valueTypeName = GetValueTypeName(model, property.ValueType, models);

                // Append the property model to the list
                extensionMethods.Add(new GeneratorExtensionMethod(property, className, valueTypeName));

            }

            // Dont add the clas if there are no extension methods to write
            if (extensionMethods.Count == 0) return;

            // Write the start of the class
            writer.WriteLine($"    public static class {model.ClrName}Extensions {{");
            writer.WriteLine();

            // Write the extension methods
            foreach (GeneratorExtensionMethod extensionMethod in extensionMethods) {
                extensionMethod.WriteTo(writer);
            }
            
            // Write the end of the class
            writer.WriteLine("    }");
            writer.WriteLine();

        }

        /// <summary>
        /// Internal method used for writing something to the start of the file - eg. a file header comment.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="settings">The models generator settings.</param>
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

        /// <summary>
        /// Internal method used for writing the usings/imports of the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="imports">The usings/imports to be added.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void WriteImports(TextWriter writer, TypeModel model, List<string> imports, ModelsGeneratorSettings settings) {

            if (imports.Count == 0) return;

            foreach (var import in imports.Distinct().OrderBy(x => x)) {
                writer.WriteLine($"using {import};");
            }

            writer.WriteLine();

        }

        /// <summary>
        /// Internal method used for writing any file level <c>#pragma</c> flags to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void WritePragma(TextWriter writer, TypeModel model, ModelsGeneratorSettings settings) {
            writer.WriteLine("#pragma warning disable 0109");
            writer.WriteLine("#pragma warning disable 1591");
            writer.WriteLine();
        }
        
        /// <summary>
        /// Internal method used for writing the namespace open declaration to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void WriteNamespaceStart(TextWriter writer, TypeModel model, ModelsGeneratorSettings settings) {
            writer.WriteLine("namespace " + model.Namespace + " {");
            writer.WriteLine();
        }
        
        /// <summary>
        /// Internal method used for writing the namespace close declaration to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void WriteNamespaceEnd(TextWriter writer, TypeModel model, ModelsGeneratorSettings settings) {
            writer.Write('}');
        }

        /// <summary>
        /// Internal method used for writing the composition to the file. If <paramref name="model"/> is not a
        /// composition type, this method should not write anything to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="models">A list with all the models.</param>
        /// <param name="settings">The models generator settings.</param>
        /// <param name="ignoredPropertyTypes">A hash set with the ignored property types.</param>
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

        /// <summary>
        /// Internal method used for writing the class open declaration to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="partialClass">A reference to the custom partial, if any.</param>
        /// <param name="settings">The models generator settings.</param>
        /// <param name="inherits">A list of inherits (base type and interfaces).</param>
        protected virtual void WriteClassStart(TextWriter writer, TypeModel model, List<string> inherits, ClassSummary partialClass, ModelsGeneratorSettings settings) {

            writer.WriteLine($"    [PublishedModel(\"{model.Alias}\")]");
            writer.WriteLine($"    public partial class {model.ClrName}{(inherits.Any() ? " : " + string.Join(", ", inherits) : "")} {{");
            writer.WriteLine();

        }

        /// <summary>
        /// Internal method used for writing the class close declaration to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void WriteClassEnd(TextWriter writer, TypeModel model, ModelsGeneratorSettings settings) {
            string indent = "".PadLeft(1 * settings.EditorConfig.IndentSize, ' ');
            writer.Write(indent);
            writer.WriteLine('}');
            writer.WriteLine();
        }

        protected virtual void WriteConstants(TextWriter writer, TypeModel model, ModelsGeneratorSettings settings) {
            
            string indent = "".PadLeft(2 * settings.EditorConfig.IndentSize, ' ');

            writer.WriteLine($"{indent}#region Constants");
            writer.WriteLine();

            writer.WriteLine($"{indent}public new const string ModelTypeAlias = \"{model.Alias}\";");
            writer.WriteLine();

            writer.WriteLine($"{indent}#endregion");
            writer.WriteLine();

        }

        /// <summary>
        /// Internal method used for writing the constructor to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="partialClass">A reference to the custom partial, if any.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void WriteConstructor(TextWriter writer, TypeModel model, ClassSummary partialClass, ModelsGeneratorSettings settings) {

            // If there is a custom partial for the model, and it already has a constructor with the require signature,
            // we shouldn't add one to the generated partial
            if (partialClass is { HasPublishedContentConstructor: true }) return;

            // Get the type of the first parameter
            string t = model.IsElementType ? "IPublishedElement" : "IPublishedContent";
            
            string indent = "".PadLeft(2 * settings.EditorConfig.IndentSize, ' ');

            writer.WriteLine($"{indent}#region Constructors");
            writer.WriteLine();

            writer.WriteLine($"        public {model.ClrName}({t} content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback) {{ }}");
            writer.WriteLine();

            writer.WriteLine($"{indent}#endregion");
            writer.WriteLine();

        }

        /// <summary>
        /// Internal method used for writing the properties to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="partialClass">A reference to the custom partial, if any.</param>
        /// <param name="models">A list with all the models.</param>
        /// <param name="settings">The models generator settings.</param>
        /// <param name="ignoredPropertyTypes">A hash set with the ignored property types.</param>
        protected virtual void WriteProperties(TextWriter writer, TypeModel model, HashSet<string> ignoredPropertyTypes, ClassSummary partialClass, TypeModelList models, ModelsGeneratorSettings settings) {

            string indent = "".PadLeft(2 * settings.EditorConfig.IndentSize, ' ');
            
            // This is an extra, seemingly unnecessary step, but in order to prevent the region from being generated if
            // there are no properties to write, we need create a list of the properties to write, and then check
            // whether that list has any items
            List<PropertyModel> properties = new();
            foreach (PropertyModel property in model.Properties) {
                
                // Skip the property if it already has been flagged as ignored
                if (property.IsIgnored || ignoredPropertyTypes.Contains(property.Alias)) continue;
            
                // If the model has a custom partial class, and the property has been manually added there, we shouldn't add it here
                if (partialClass != null && partialClass.HasProperty(property.ClrName)) continue;

                // Append the property model to the list
                properties.Add(property);

            }

            // Return if there are no properties to write
            if (properties.Count == 0) return;
            
            writer.WriteLine($"{indent}#region Properties");
            writer.WriteLine();

            foreach (PropertyModel property in properties) {

                WriteProperty(writer, model, property, ignoredPropertyTypes, partialClass, models, settings);

            }

            writer.WriteLine($"{indent}#endregion");
            writer.WriteLine();

        }

        /// <summary>
        /// Internal method used for writing a single property to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="partialClass">A reference to a partial class, if present.</param>
        /// <param name="models">A list with all the models.</param>
        /// <param name="settings">The models generator settings.</param>
        /// <param name="property">The property.</param>
        /// <param name="ignoredPropertyTypes">A hash set with the ignored property types.</param>
        protected virtual void WriteProperty(TextWriter writer, TypeModel model, PropertyModel property, HashSet<string> ignoredPropertyTypes, ClassSummary partialClass, TypeModelList models, ModelsGeneratorSettings settings) {

            // Gets the name of the value type
            string valueTypeName = GetValueTypeName(model, property.ValueType, models);
            
            // Get the declaring type of the property. This mey be different than "model" when using compositions
            if (!model.HasPropertyType(property.Alias, out TypeModel declaringType)) {
                throw new Exception("Property type not found. This shouldn't happen.");
            }

            bool useStaticMethod;
            switch (property.StaticMethod) {
                
                case PropertyStaticMethod.Always:
                    useStaticMethod = true;
                    break;
                
                case PropertyStaticMethod.Auto:
                    useStaticMethod = declaringType != model || model.IsComposition;
                    break;

                case PropertyStaticMethod.Never:
                default:
                    useStaticMethod = false;
                    break;

            }
            
            WriteJsonNetPropertySettings(writer, model, property, settings);
            
            writer.WriteLine("        [ImplementPropertyType(\"" + property.Alias + "\")]");
            writer.Write("        public new " + valueTypeName + " " + property.ClrName + " => ");

            if (useStaticMethod) {

                // If "model" and "declaringType" differ, we need to specify the class name
                string className = declaringType == model ? null : $"{declaringType.ClrName}.";

                // TODO: Should "className" also include the namespace?

                writer.Write($"{className}Get" + property.ClrName + "(this);");

            } else {

                writer.Write($"this.Value<{valueTypeName}>(\"{property.Alias}\");");

            }
            
            writer.WriteLine();
            writer.WriteLine();

        }

        protected virtual void WriteStaticMethods(TextWriter writer, TypeModel model, HashSet<string> ignoredPropertyTypes, ClassSummary partialClass, TypeModelList models, ModelsGeneratorSettings settings) {

            string indent = "".PadLeft(2 * settings.EditorConfig.IndentSize, ' ');

            // Find all properties that need a static getter method
            List<PropertyModel> properties = new();
            foreach (PropertyModel property in model.Properties) {
                
                if (property.StaticMethod != PropertyStaticMethod.Always && (property.StaticMethod != PropertyStaticMethod.Auto || !model.IsComposition)) continue;
                if (!model.HasPropertyType(property.Alias, out TypeModel declaringType)) throw new Exception("Property type not found. This shouldn't happen.");
                if (declaringType != model) continue;

                // If the type has a custom partial class, and that class has a method with the same name, we skip adding it to the generated file
                if (partialClass != null && partialClass.HasMethod("Get" + property.ClrName)) continue;

                properties.Add(property);

            }
            
            // Return if we didn't find any properties
            if (properties.Count == 0) return;
            
            writer.WriteLine($"{indent}#region Static methods");
            writer.WriteLine();

            foreach (PropertyModel property in properties) {
                
                string valueTypeName = GetValueTypeName(model, property.ValueType, models);
                
                writer.WriteLine($"{indent}public static {valueTypeName} Get{property.ClrName}(I{model.ClrName} that) => that.Value<{valueTypeName}>(\"{property.Alias}\");");
                writer.WriteLine();

            }

            writer.WriteLine($"{indent}#endregion");
            writer.WriteLine();

        }

        /// <summary>
        /// Internal method used for writing the JSON.net settings of a property to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="property">The property.</param>
        /// <param name="settings">The models generator settings.</param>
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

        public virtual void SaveLastBuildDate() {
            string modelsDirectory = _modelsBuilderSettings.ModelsDirectoryAbsolute(_hostingEnvironment);
            if (!Directory.Exists(modelsDirectory)) Directory.CreateDirectory(modelsDirectory);
            File.WriteAllText(Path.Combine(modelsDirectory, "lastBuild.flag"), EssentialsTime.UtcNow.ToString(Iso8601Constants.DateTimeMilliseconds) + Environment.NewLine);
        }

        public virtual EssentialsTime GetLastBuildDate() {
            
            string modelsDirectory = _modelsBuilderSettings.ModelsDirectoryAbsolute(_hostingEnvironment);

            string path = Path.Combine(modelsDirectory, "lastBuild.flag");
            if (!File.Exists(path)) return null;

            string first = File.ReadAllLines(path).FirstOrDefault();

            return EssentialsTime.TryParseIso8601(first, out EssentialsTime time) ? time : null;

        }

        #endregion

    }

}