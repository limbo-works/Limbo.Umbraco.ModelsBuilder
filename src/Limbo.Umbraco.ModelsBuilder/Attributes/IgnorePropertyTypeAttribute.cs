using System;

namespace Limbo.Umbraco.ModelsBuilder.Attributes {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IgnorePropertyTypeAttribute : Attribute {

        public string PropertyName { get; }

        public IgnorePropertyTypeAttribute(string propertyName) {
            PropertyName = propertyName;
        }

    }

}