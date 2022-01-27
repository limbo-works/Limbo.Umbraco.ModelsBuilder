using System.Collections.Generic;
using System.Linq;
using OmgBacon.ModelsBuilder.Models;

namespace OmgBacon.ModelsBuilder.Containers {
    
    public class ContainerModel {

        public string Directory { get; }

        public TypeModel[] Types { get; }

        public ContainerModel(string directory, IEnumerable<TypeModel> types) {
            Directory = directory;
            Types = types.ToArray();
        }

    }

}