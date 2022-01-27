using OmgBacon.ModelsBuilder.Models;

namespace OmgBacon.ModelsBuilder.Containers {
    
    public interface IModelsContainer {

        string Directory { get; }

        bool Include(TypeModel type);

    }

}