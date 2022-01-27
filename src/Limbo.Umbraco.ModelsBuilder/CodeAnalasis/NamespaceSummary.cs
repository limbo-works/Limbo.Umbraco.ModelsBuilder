using System.Collections.Generic;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis {

    /// <summary>
    /// Class representing a summary about a namespace.
    /// </summary>
    public class NamespaceSummary {

        /// <summary>
        /// Gets the name of the namespace.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a list of the classes in the namespace.
        /// </summary>
        public List<ClassSummary> Classes { get; set; }

    }

}