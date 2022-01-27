using System;

namespace OmgBacon.ModelsBuilder.Attributes {
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IgnorePropertyTypeAttribute : Attribute {

        public string PropertyName { get; }

        public IgnorePropertyTypeAttribute(string propertyName) {
            PropertyName = propertyName;
        }

    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IgnorePropertyTypesAttribute : Attribute {

        public string[] PropertyNames { get; }

        public IgnorePropertyTypesAttribute(string propertyName1) {
            if (string.IsNullOrWhiteSpace(propertyName1)) throw new ArgumentNullException(nameof(propertyName1));
            PropertyNames = new[] { propertyName1 };
        }

        public IgnorePropertyTypesAttribute(string propertyName1, string propertyName2) {
            if (string.IsNullOrWhiteSpace(propertyName1)) throw new ArgumentNullException(nameof(propertyName1));
            if (string.IsNullOrWhiteSpace(propertyName2)) throw new ArgumentNullException(nameof(propertyName2));
            PropertyNames = new[] { propertyName1, propertyName2 };
        }

        public IgnorePropertyTypesAttribute(params string[] propertyNames) {
            PropertyNames = propertyNames;
        }

    }

}