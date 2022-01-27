using System.Collections.Generic;

namespace OmgBacon.ModelsBuilder.CodeAnalasis
{
    public class NamespaceSummary {

        public string Name { get; set; }

        public List<ClassSummary> Classes { get; set; }

    }
}