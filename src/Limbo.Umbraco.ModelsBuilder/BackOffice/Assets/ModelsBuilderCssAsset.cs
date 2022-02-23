using Umbraco.Cms.Core.WebAssets;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.BackOffice.Assets {
    
    public class ModelsBuilderCssAsset : IAssetFile {
        
        public string FilePath { get; set; }

        public AssetType DependencyType { get; }

        public ModelsBuilderCssAsset() {
            DependencyType = AssetType.Css;
            FilePath = $"/App_Plugins/Limbo.Umbraco.ModelsBuilder/Styles/Default.css?v={ModelsBuilderPackage.SemVersion}";
        }

    }

}