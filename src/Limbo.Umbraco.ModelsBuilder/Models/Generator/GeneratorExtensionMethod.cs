using System.IO;
using Limbo.Umbraco.ModelsBuilder.Services;
using Limbo.Umbraco.ModelsBuilder.Settings;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Models.Generator {

    public class GeneratorExtensionMethod {

        public PropertyModel Property { get; }

        public string ClassName { get; }

        public string ValueTypeName { get; }

        public GeneratorExtensionMethod(PropertyModel property, string className, string valueTypeName) {
            Property = property;
            ClassName = className;
            ValueTypeName = valueTypeName;
        }

        public virtual void WriteTo(TextWriter writer, ModelsSourceGenerator generator, ModelsGeneratorSettings settings) {

            string indent1 = generator.GetIndent(settings, 2);
            string indent2 = generator.GetIndent(settings, 3);

            // The [MaybeNull] attribute should be added to the method if either the "MaybeNull" property is either
            // explicitly set to "true", or set to "null" and "ValueType" doesn't represent a value type
            if (Property.MaybeNull is true || Property.MaybeNull is null && !Property.ValueType.IsValueType) {
                writer.Write(indent1);
                writer.Write("[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]");
                writer.WriteLine();
            }

            writer.Write(indent1);
            writer.Write("public static " + ValueTypeName + " " + Property.ClrName + "(");
            writer.Write("this " + ClassName + " content, string culture = null, string segment = null");
            writer.Write(") {");
            writer.WriteLine();

            writer.Write(indent2);
            writer.Write("return content.Value<" + ValueTypeName + ">(\"" + Property.Alias + "\", culture, segment);");
            writer.WriteLine();

            writer.Write(indent1);
            writer.Write('}');
            writer.WriteLine();

            writer.WriteLine();

        }

    }

}