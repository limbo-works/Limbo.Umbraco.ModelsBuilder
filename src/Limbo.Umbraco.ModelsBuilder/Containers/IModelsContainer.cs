using Limbo.Umbraco.ModelsBuilder.Models;

namespace Limbo.Umbraco.ModelsBuilder.Containers {
    
    public interface IModelsContainer {

        string Directory { get; }

        bool Include(TypeModel type);

    }

}