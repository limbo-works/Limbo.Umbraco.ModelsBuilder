using System.Collections.Generic;
using System.Reflection;
using Umbraco.Cms.Core.Manifest;

#pragma warning disable CS1591

namespace Limbo.Umbraco.ModelsBuilder.Manifests {

    public class ModelsBuilderManifest : IManifestFilter {

        public void Filter(List<PackageManifest> manifests) {

            // Initialize a new manifest filter for this package
            PackageManifest manifest = new() {
                AllowPackageTelemetry = true,
                PackageName = ModelsBuilderPackage.Name,
                Version = ModelsBuilderPackage.InformationalVersion,
                BundleOptions = BundleOptions.Independent,
                Scripts = new[] { $"/App_Plugins/{ModelsBuilderPackage.Alias}/Scripts/Controllers/Dashboard.js" },
                Stylesheets = new[] { $"/App_Plugins/{ModelsBuilderPackage.Alias}/Styles/Default.css" }
            };

            // The "PackageId" property isn't available prior to Umbraco 12, and since the package is build against
            // Umbraco 10, we need to use reflection for setting the property value for Umbraco 12+. Ideally this
            // shouldn't fail, but we might at least add a try/catch to be sure
            try {
                PropertyInfo? property = manifest.GetType().GetProperty("PackageId");
                property?.SetValue(manifest, ModelsBuilderPackage.Alias);
            } catch {
                // We don't really care about the exception
            }

            // Append the manifest
            manifests.Add(manifest);

        }

    }

}