using System.Collections.Generic;

namespace Limbo.Umbraco.ModelsBuilder.CodeAnalasis {

    public class NamespaceSummary {

        public string Name { get; set; }

        public List<ClassSummary> Classes { get; set; }

    }

}