using Limbo.Umbraco.ModelsBuilder.Models;
using System;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Containers {

    public class ModelsBuilderContainer : IModelsContainer {

        public string Directory { get; set; }

        public Func<TypeModel, bool> Callback { get; set; }

        public ModelsBuilderContainer() {
            Callback = x => true;
        }

        public ModelsBuilderContainer(string directory, Func<TypeModel, bool> callback) {
            Directory = directory;
            Callback = callback;
        }

        public virtual bool Include(TypeModel type) {
            return Callback(type);
        }

    }

}