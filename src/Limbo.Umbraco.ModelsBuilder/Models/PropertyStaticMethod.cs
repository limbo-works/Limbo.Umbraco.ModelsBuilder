namespace Limbo.Umbraco.ModelsBuilder.Models {
    
    /// <summary>
    /// Enum class indicating whether a static method should be generated for a property.
    /// </summary>
    public enum PropertyStaticMethod {

        /// <summary>
        /// Indicates that the models generator should automatically determine whether a static method should be added for the property. 
        /// </summary>
        Auto,

        /// <summary>
        /// Indicates that the models builder should always generate a static method for the property.
        /// </summary>
        Always,
        
        /// <summary>
        /// Indicates that the models builder should never generate a static method for the property.
        /// </summary>
        Never

    }

}