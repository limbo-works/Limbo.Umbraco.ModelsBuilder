using System.IO;

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

        public virtual void WriteTo(TextWriter writer) {
            
            string indent1 = "".PadLeft(8);
            string indent2 = "".PadLeft(12);
            
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