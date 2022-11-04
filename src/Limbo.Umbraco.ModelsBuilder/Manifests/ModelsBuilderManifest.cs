using Skybrud.Essentials.Strings.Extensions;
using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;

#pragma warning disable CS1591

namespace Limbo.Umbraco.ModelsBuilder.Manifests {

    public class ModelsBuilderManifest : IManifestFilter {

        public void Filter(List<PackageManifest> manifests) {
            manifests.Add(new PackageManifest {
                PackageName = ModelsBuilderPackage.Alias.ToKebabCase(),
                BundleOptions = BundleOptions.Independent,
                Scripts = new[] {
                    $"/App_Plugins/{ModelsBuilderPackage.Alias}/Scripts/Controllers/Dashboard.js"
                },
                Stylesheets = new[] {
                    $"/App_Plugins/{ModelsBuilderPackage.Alias}/Styles/Default.css"
                }
            });
        }

    }

}