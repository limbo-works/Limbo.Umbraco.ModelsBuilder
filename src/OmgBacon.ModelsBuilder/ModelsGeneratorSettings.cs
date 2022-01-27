using System;
using System.Collections.Generic;
using OmgBacon.ModelsBuilder.Containers;
using OmgBacon.ModelsBuilder.Settings;

namespace OmgBacon.ModelsBuilder {
    
    public class ModelsGeneratorSettings {

        /// <summary>
        /// Gets or sets the default namespace. The namespace used for the individual models may be overridden in the models generation process.
        /// </summary>
        public string DefaultNamespace { get; set; }

        public string DefaultModelsPath { get; set; }

        public bool UseDirectories { get; set; } = true;

        public List<IModelsContainer> Containers { get; }
        
        public EditorConfigSettings EditorConfig { get; set; }

        public ModelsGeneratorSettings() {
            Containers = new List<IModelsContainer>();
        }
        
        public ModelsGeneratorSettings(string defaultNamespace, string defaultModelsPath) {
            if (string.IsNullOrWhiteSpace(defaultNamespace)) throw new ArgumentNullException(nameof(defaultNamespace));
            if (string.IsNullOrWhiteSpace(defaultModelsPath)) throw new ArgumentNullException(nameof(defaultModelsPath));
            DefaultNamespace = defaultNamespace;
            DefaultModelsPath = defaultModelsPath;
            Containers = new List<IModelsContainer>();
            EditorConfig = new EditorConfigSettings();
        }

    }

}