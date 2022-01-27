using Microsoft.Extensions.Options;
using OmgBacon.ModelsBuilder.Composers;

namespace OmgBacon.ModelsBuilder.Settings {
    
    public class ModelsBuilderSettingsLimbo {
        
        private readonly IOptions<Umbraco.Cms.Core.Configuration.Models.ModelsBuilderSettings> _modelsBuilderSettings;
        private readonly LimboModelsBuilderSettings _limboModelsBuilderSettings;
        
        public bool AcceptUnsafeModelsDirectory { get; }
        
        public string ModelsDirectory { get; }
        
        public string ModelsNamespace { get; }

        public ModelsBuilderSettingsLimbo(IOptions<Umbraco.Cms.Core.Configuration.Models.ModelsBuilderSettings> modelsBuilderSettings, LimboModelsBuilderSettings limboModelsBuilderSettings) {
            
            _modelsBuilderSettings = modelsBuilderSettings;
            _limboModelsBuilderSettings = limboModelsBuilderSettings;

            AcceptUnsafeModelsDirectory = modelsBuilderSettings.Value.AcceptUnsafeModelsDirectory;
            ModelsDirectory = modelsBuilderSettings.Value.ModelsDirectory;
            ModelsNamespace = modelsBuilderSettings.Value.ModelsNamespace;

        }

    }

}