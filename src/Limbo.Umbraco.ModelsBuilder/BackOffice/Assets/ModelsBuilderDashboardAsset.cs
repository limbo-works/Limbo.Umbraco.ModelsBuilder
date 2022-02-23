using Umbraco.Cms.Core.WebAssets;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.BackOffice.Assets {

    public class ModelsBuilderDashboardAsset : IAssetFile {

        public string FilePath { get; set; }

        public AssetType DependencyType { get; }

        public ModelsBuilderDashboardAsset() {
            DependencyType = AssetType.Javascript;
            FilePath = $"/App_Plugins/Limbo.Umbraco.ModelsBuilder/Scripts/Controllers/Dashboard.js?v={ModelsBuilderPackage.SemVersion}";
        }

    }

}