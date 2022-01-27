using OmgBacon.ModelsBuilder.Models;

namespace OmgBacon.ModelsBuilder.Containers {
    
    public class HelloContainer {

        public ContentTypeKind Type { get; set; }

        public int[] RootIds { get; set; }

        public string[] Directories { get; set; }

    }

}