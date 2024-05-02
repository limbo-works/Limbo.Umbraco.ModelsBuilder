using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;

#pragma warning disable CS1591

namespace Limbo.Umbraco.ModelsBuilder.Manifests;

public class ModelsBuilderManifest : IManifestFilter {

    public void Filter(List<PackageManifest> manifests) {

        // Initialize a new manifest filter for this package
        PackageManifest manifest = new() {
            AllowPackageTelemetry = true,
            PackageId = ModelsBuilderPackage.Alias,
            PackageName = ModelsBuilderPackage.Name,
            Version = ModelsBuilderPackage.InformationalVersion,
            BundleOptions = BundleOptions.Independent,
            Scripts = [$"/App_Plugins/{ModelsBuilderPackage.Alias}/Scripts/Controllers/Dashboard.js"],
            Stylesheets = [$"/App_Plugins/{ModelsBuilderPackage.Alias}/Styles/Default.css"]
        };

        // Append the manifest
        manifests.Add(manifest);

    }

}