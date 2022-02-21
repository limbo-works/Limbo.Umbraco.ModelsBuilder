using Limbo.Umbraco.ModelsBuilder.Models;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Containers {

    public class HelloContainer {

        public ContentTypeKind Type { get; set; }

        public int[] RootIds { get; set; }

        public string[] Directories { get; set; }

    }

}