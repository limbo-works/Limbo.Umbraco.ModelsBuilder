using Limbo.Umbraco.ModelsBuilder.Models;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Containers {

    public interface IModelsContainer {

        string Directory { get; }

        bool Include(TypeModel type);

    }

}