using Limbo.Umbraco.ModelsBuilder.Attributes;
using Limbo.Umbraco.ModelsBuilder.CodeAnalasis;
using Limbo.Umbraco.ModelsBuilder.Extensions;
using Limbo.Umbraco.ModelsBuilder.Logging;
using Limbo.Umbraco.ModelsBuilder.Models;
using Limbo.Umbraco.ModelsBuilder.Models.Generator;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using Limbo.Umbraco.ModelsBuilder.Models.Json;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

#pragma warning disable 1591

// ReSharper disable PatternAlwaysOfType
// ReSharper disable AssignNullToNotNullAttribute

namespace Limbo.Umbraco.ModelsBuilder.Services {

    /// <summary>
    /// Primary class servering as the models source generator.
    /// </summary>
    public class ModelsSourceGenerator {

        private readonly LimboModelsBuilderSettings _modelsBuilderSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly OutOfDateModelsStatus _outOfDateModels;
        private readonly ModelsGenerator _modelsGenerator;

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

        public ModelsSourceGenerator(IWebHostEnvironment webHostEnvironment,
            IHostingEnvironment hostingEnvironment,
            IOptions<LimboModelsBuilderSettings> modelsBuilderSettings, OutOfDateModelsStatus outOfDateModels,
            ModelsGenerator modelsGenerator) {
            _modelsBuilderSettings = modelsBuilderSettings.Value;
            _webHostEnvironment = webHostEnvironment;
            _hostingEnvironment = hostingEnvironment;
            _outOfDateModels = outOfDateModels;
            _modelsGenerator = modelsGenerator;
        }

        #endregion

        #region Member methods

        public virtual void BuildModels() {
            BuildModels(_modelsGenerator.GetDefaultSettings());
        }

        public virtual void BuildModels(ModelsGeneratorSettings settings) {

            // Initialize a new log (if logging is enabled)
            ModelsBuilderLog? log = settings is { EnableLogging: true } ? new ModelsBuilderLog() : null;

            log?.AppendLine(JObject.FromObject(settings).ToString(Formatting.Indented));
            log?.AppendLine();
            log?.AppendLine();

            // Generate definitions for all the models
            log?.AppendLine($"Getting models from {_modelsGenerator}");
            TypeModelList models = _modelsGenerator.GetModels();
            log?.AppendLine($"> Found {models.Count} models");
            log?.AppendLine();
            log?.AppendLine();

            // Delete existing ".generated.cs" files (if enabled)
            if (settings.DeleteGeneratedFiles) DeleteGenerateFiles(log);

            // Generate the source code and save the models to disk
            SaveModels(models, settings, log);

            SaveToDisk(log);

        }

        /// <summary>
        /// Saves the specified collection of <paramref name="models"/> to their respective locations on the disk.
        /// </summary>
        /// <param name="models">A collection with the models to be saved.</param>
        /// <param name="settings">The models generator settings.</param>
        public virtual void SaveModels(IEnumerable<TypeModel> models, ModelsGeneratorSettings settings) {

            ModelsBuilderLog? log = settings is { EnableLogging: true } ? new ModelsBuilderLog() : null;

            SaveModels(models, settings, log);

            SaveToDisk(log);

        }

        /// <summary>
        /// Saves the specified collection of <paramref name="models"/> to their respective locations on the disk.
        /// </summary>
        /// <param name="models">A collection with the models to be saved.</param>
        /// <param name="settings">The models generator settings.</param>
        /// <param name="log">The current <see cref="ModelsBuilderLog"/> instance.</param>
        protected virtual void SaveModels(IEnumerable<TypeModel> models, ModelsGeneratorSettings settings, ModelsBuilderLog? log) {

            // Create a new list for the models so we can quickly look them up later
            TypeModelList list = models as TypeModelList ?? new TypeModelList(models);

            foreach (TypeModel model in list) {

                // Skip types that have been explicitly ignored
                if (model.IsIgnored) continue;

                log?.AppendLine($"> Generating model for type {model.Name}");

                // Generate the C# source code for the model
                string source = GetSource(model, list, settings);

                // Get the parent of the file's directory
                string directory = Path.GetDirectoryName(model.Path)!;

                if (!Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                    log?.AppendLine($"  > Created new directory {directory}");
                }

                File.WriteAllText(model.Path!, source, Encoding.UTF8);
                log?.AppendLine($"  > Saved new file {model.Path}");

            }

            // Update the file on disk
            SaveLastBuildDate();

            // Clear the file on disk
            _outOfDateModels.Clear();

            // And we're done :D
            log?.AppendLine("> Done");
            log?.AppendLine();
            log?.AppendLine();

        }

        /// <summary>
        /// Delete all <c>*.generated.cs</c> files in the models
        /// </summary>
        /// <param name="settings"></param>
        public void DeleteGenerateFiles(ModelsGeneratorSettings settings) {

            ModelsBuilderLog? log = settings is { EnableLogging: true } ? new ModelsBuilderLog() : null;

            DeleteGenerateFiles(log);

            SaveToDisk(log);

        }

        protected void DeleteGenerateFiles(ModelsBuilderLog? log) {

            // Determine the full path to the models directory
            string path = _modelsBuilderSettings.ModelsDirectoryAbsolute(_hostingEnvironment);

            // Initialize a new DirectoryInfo instance
            DirectoryInfo directory = new(path);

            // Exit if the directory doesn't exist (no files to delete)
            if (!directory.Exists) {
                log?.AppendLine($"Exiting as models directory does not exist: {directory.FullName}");
                SaveToDisk(log);
                return;
            }

            log?.AppendLine("Deleting generated files in models directory");
            log?.AppendLine("> Directory: " + directory.FullName);

            // Find all "*.generated.cs" files
            FileInfo[] files = directory.GetFiles("*.generated.cs", SearchOption.AllDirectories);
            log?.AppendLine($"> Found {files.Length} files");

            // Iterate through all "*.generated.cs" files
            foreach (FileInfo file in files) {

                log?.AppendLine($"> Deleting file {file.FullName}");

                // Skip if the file doesn't look like it was generated by ModelsBuilder
                if (!IsModelsBuilderGeneratedFile(file.FullName)) {
                    log?.AppendLine(" > Doesn't look like the file was generated by ModelsBuilder :o");
                    continue;
                }

                // Attempt to delete the file
                try {
                    file.Delete();
                    log?.AppendLine(" > Done");
                } catch (Exception ex) {
                    log?.AppendLine(" > Failed");
                    log?.AppendLine(ex.ToString());
                    log?.AppendLine();
                }

            }

            log?.AppendLine();
            log?.AppendLine();

        }

        public bool IsModelsBuilderGeneratedFile(string path) {

            // Definitely not if the file doesn't exist
            if (!File.Exists(path)) return false;

            byte[] buffer = new byte[512];

            // Better watch for any exceptions
            try {

                // Open a new file stream
                using FileStream fs = new(path, FileMode.Open, FileAccess.Read);

                // Read the first 512 bytes
                int _ = fs.Read(buffer, 0, buffer.Length);

                // Close the stream
                fs.Close();

                // Convert the bytes to an UTF-8 encoded string
                string contents = Encoding.UTF8.GetString(buffer);

                // Does the first 512 bytes contains the ModelsBuilder header?
                return contents.IndexOf("//    Limbo.Umbraco.ModelsBuilder v", StringComparison.CurrentCultureIgnoreCase) > 0;

            } catch (Exception) {

                // Can't tell for sure, but probably not
                return false;

            }

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
                    return actualTypeName.Namespace == model.Namespace ? actualTypeName.ClrName : "global::" + actualTypeName.Namespace + "." + actualTypeName.ClrName;
                }
                throw new InvalidOperationException($"Don't know how to map ModelType with content type alias \"{modelType.ContentTypeAlias}\".");
            }

            if (type.FullName != null && SimpleNames.TryGetValue(type.FullName, out string? simpleName)) {
                return simpleName;
            }

            string prefix;

            if (type.DeclaringType != null) {
                prefix = GetValueTypeName(model, type.DeclaringType, models) + ".";
            } else {
                prefix = type.Namespace switch {
                    "Umbraco.Core.Models.PublishedContent" => string.Empty,
                    _ => $"global::{type.Namespace}."
                };
            }

            if (type.GenericTypeArguments.Length == 0) {
                return $"{prefix}{type.Name}";
            }

            return $"{prefix}{type.Name.Split('`')[0]}<{string.Join(",", from t in type.GenericTypeArguments select GetValueTypeName(model, t, models))}>";

        }

        /// <summary>
        /// Returns the CLR type of the specified <paramref name="model"/>, or <c>null</c> if not found.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>An instance of <see cref="Type"/>, or <c>null</c>.</returns>
        protected virtual Type? GetClrType(TypeModel model) {

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

            Type? type = GetClrType(model);
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
                "System",
                "System.Linq.Expressions",
                "Umbraco.Cms.Core.Models.PublishedContent",
                "Umbraco.Cms.Core.PublishedCache",
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

            string customPartialPath = model.Path!.Replace(".generated.cs", ".cs");

            ClassSummary? partialClass = null;
            if (FileSummary.TryLoad(customPartialPath, out FileSummary? summary)) {
                summary.TryGetClass($"{model.Namespace}.{model.ClrName}", out partialClass);
            }

            List<string> imports = GetDefaultImports();
            List<string> inherits = new();

            // If the partial class already has a base type, we shouldn't try adding one
            string? partialBaseType = partialClass?.BaseTypes.FirstOrDefault(x => !Regex.IsMatch(x, "^I[A-Z]"));

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

            WriteHelpers(writer, model, settings);

            WriteConstructor(writer, model, partialClass, settings);

            WriteProperties(writer, model, ignoredPropertyTypes, partialClass, models, settings);

            WriteStaticMethods(writer, model, ignoredPropertyTypes, partialClass, models, settings);

            WriteClassEnd(writer, model, settings);

            WriteExtensionMethodsClass(writer, model, ignoredPropertyTypes, partialClass, models, settings);

            WriteNamespaceEnd(writer, model, settings);

            return sb.ToString().Trim();

        }

        private void WriteExtensionMethodsClass(TextWriter writer, TypeModel model, HashSet<string> ignoredPropertyTypes, ClassSummary? partialClass, TypeModelList models, ModelsGeneratorSettings settings) {

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

            string name = assembly.GetName().Name!;

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
            writer.WriteLine("#pragma warning disable 0108");
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
        protected virtual void WriteClassStart(TextWriter writer, TypeModel model, List<string> inherits, ClassSummary? partialClass, ModelsGeneratorSettings settings) {

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

        /// <summary>
        /// Method responsible for generating various constants and static helper methods for working with the models.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void WriteHelpers(TextWriter writer, TypeModel model, ModelsGeneratorSettings settings) {

            string indent1 = "".PadLeft(2 * settings.EditorConfig.IndentSize, ' ');
            string indent2 = "".PadLeft(3 * settings.EditorConfig.IndentSize, ' ');

            writer.WriteLine($"{indent1}#region Helpers");
            writer.WriteLine();

            writer.WriteLine($"{indent1}public new const string ModelTypeAlias = \"{model.Alias}\";");
            writer.WriteLine();

            writer.WriteLine($"{indent1}public new const PublishedItemType ModelItemType = PublishedItemType.{model.PublishedContentType.ItemType};");
            writer.WriteLine();

            writer.WriteLine($"{indent1}[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]");
            writer.WriteLine($"{indent1}public new static IPublishedContentType GetModelContentType(IPublishedSnapshotAccessor publishedSnapshotAccessor)");
            writer.WriteLine($"{indent1}{indent1}=> PublishedModelUtility.GetModelContentType(publishedSnapshotAccessor, ModelItemType, ModelTypeAlias);");
            writer.WriteLine();

            writer.WriteLine($"{indent1}[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]");
            writer.WriteLine($"{indent1}public static IPublishedPropertyType GetModelPropertyType<TValue>(IPublishedSnapshotAccessor publishedSnapshotAccessor, Expression<Func<{model.ClrName}, TValue>> selector)");
            writer.WriteLine($"{indent2}=> PublishedModelUtility.GetModelPropertyType(GetModelContentType(publishedSnapshotAccessor), selector);");
            writer.WriteLine();

            writer.WriteLine($"{indent1}#endregion");
            writer.WriteLine();

        }

        /// <summary>
        /// Internal method used for writing the constructor to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="model">The current model.</param>
        /// <param name="partialClass">A reference to the custom partial, if any.</param>
        /// <param name="settings">The models generator settings.</param>
        protected virtual void WriteConstructor(TextWriter writer, TypeModel model, ClassSummary? partialClass, ModelsGeneratorSettings settings) {

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
        protected virtual void WriteProperties(TextWriter writer, TypeModel model, HashSet<string> ignoredPropertyTypes, ClassSummary? partialClass, TypeModelList models, ModelsGeneratorSettings settings) {

            string indent = "".PadLeft(2 * settings.EditorConfig.IndentSize, ' ');

            // This is an extra, seemingly unnecessary step, but in order to prevent the region from being generated if
            // there are no properties to write, we need create a list of the properties to write, and then check
            // whether that list has any items
            List<PropertyModel> properties = new();
            foreach (PropertyModel property in model.Properties) {

                // Skip the property if it already has been flagged as ignored
                if (property.IsIgnored || ignoredPropertyTypes.Contains(property.Alias)) continue;

                // If the model has a custom partial class, and the property type is ignored through a [IgnorePropertyType] attribute
                if (partialClass != null && partialClass.IgnoredPropertyTypes.Contains(property.Alias)) continue;

                // If the model has a custom partial class, and it has a property with the same CLR name, we shouldn't add it here
                if (partialClass != null && partialClass.HasProperty(property.ClrName)) continue;

                // If the model has a custom partial class, and a property indicates that it implements this property, we shouldn't add it here
                if (partialClass != null && partialClass.Properties.Any(x => x.ImplementsPropertyType == property.Alias)) continue;

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
        protected virtual void WriteProperty(TextWriter writer, TypeModel model, PropertyModel property, HashSet<string> ignoredPropertyTypes, ClassSummary? partialClass, TypeModelList models, ModelsGeneratorSettings settings) {

            string indent1 = "".PadLeft(2 * settings.EditorConfig.IndentSize, ' ');
            string indent2 = "".PadLeft(3 * settings.EditorConfig.IndentSize, ' ');

            // Gets the name of the value type
            string valueTypeName = GetValueTypeName(model, property.ValueType, models);

            // Get the declaring type of the property. This mey be different than "model" when using compositions
            if (!model.HasPropertyType(property.Alias, out TypeModel? declaringType)) {
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

            writer.WriteLine($"{indent1}[ImplementPropertyType(\"{property.Alias}\")]");
            writer.WriteLine($"{indent1}public new {valueTypeName} {property.ClrName}");
            writer.Write($"{indent2}=> ");

            if (useStaticMethod) {

                // If "model" and "declaringType" differ, we need to specify the class name
                string? className = declaringType == model ? null : $"{declaringType.ClrName}.";

                // TODO: Should "className" also include the namespace?

                writer.Write($"{className}Get" + property.ClrName + "(this);");

            } else {

                writer.Write($"this.Value<{valueTypeName}>(\"{property.Alias}\");");

            }

            writer.WriteLine();
            writer.WriteLine();

        }

        protected virtual void WriteStaticMethods(TextWriter writer, TypeModel model, HashSet<string> ignoredPropertyTypes, ClassSummary? partialClass, TypeModelList models, ModelsGeneratorSettings settings) {

            string indent1 = "".PadLeft(2 * settings.EditorConfig.IndentSize, ' ');
            string indent2 = "".PadLeft(3 * settings.EditorConfig.IndentSize, ' ');

            // Find all properties that need a static getter method
            List<PropertyModel> properties = new();
            foreach (PropertyModel property in model.Properties) {

                if (property.StaticMethod != PropertyStaticMethod.Always && (property.StaticMethod != PropertyStaticMethod.Auto || !model.IsComposition)) continue;
                if (!model.HasPropertyType(property.Alias, out TypeModel? declaringType)) throw new Exception("Property type not found. This shouldn't happen.");
                if (declaringType != model) continue;

                // If the type has a custom partial class, and that class has a method with the same name, we skip adding it to the generated file
                if (partialClass != null && partialClass.HasMethod("Get" + property.ClrName)) continue;

                properties.Add(property);

            }

            // Return if we didn't find any properties
            if (properties.Count == 0) return;

            writer.WriteLine($"{indent1}#region Static methods");
            writer.WriteLine();

            foreach (PropertyModel property in properties) {

                string valueTypeName = GetValueTypeName(model, property.ValueType, models);

                writer.WriteLine($"{indent1}public static {valueTypeName} Get{property.ClrName}(I{model.ClrName} that)");
                writer.WriteLine($"{indent2}=> that.Value<{valueTypeName}>(\"{property.Alias}\");");
                writer.WriteLine();

            }

            writer.WriteLine($"{indent1}#endregion");
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

            JsonNetPropertySettings? json = property.JsonNetSettings;
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

        public virtual EssentialsTime? GetLastBuildDate() {

            string modelsDirectory = _modelsBuilderSettings.ModelsDirectoryAbsolute(_hostingEnvironment);

            string path = Path.Combine(modelsDirectory, "lastBuild.flag");
            if (!File.Exists(path)) return null;

            string? first = File.ReadAllLines(path).FirstOrDefault();

            return EssentialsTime.TryParseIso8601(first, out EssentialsTime? time) ? time : null;

        }

        /// <summary>
        /// Saves the specified <paramref name="log"/> to a new file on the disk.
        /// </summary>
        /// <param name="log">A <see cref="ModelsBuilderLog"/> instance representing the log.</param>
        protected virtual void SaveToDisk(ModelsBuilderLog? log) {

            // Nothing to save if "log" is null
            if (log == null) return;

            // Get the path to the logs directory (and create it if doesn't exist)
            string dir = _webHostEnvironment.MapPathContentRoot($"~/Limbo/{ModelsBuilderPackage.Alias}/Logs");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Generate a new filename based on the current time
            string filename = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}.txt";

            // Write the contents to the disk
            File.WriteAllText(Path.Combine(dir, filename), log.ToString());

        }

        #endregion

    }

}