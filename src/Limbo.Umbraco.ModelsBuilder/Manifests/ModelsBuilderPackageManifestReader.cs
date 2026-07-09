#pragma warning disable CS1591

using System.Collections.Generic;
using System.Threading.Tasks;
using Skybrud.Essentials.Security.Extensions;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace Limbo.Umbraco.ModelsBuilder.Manifests;

public class ModelsBuilderPackageManifestReader : IPackageManifestReader {

    public async Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync() {

        const string alias = ModelsBuilderPackage.Alias;
        string cacheBuster = ModelsBuilderPackage.InformationalVersion.ToMd5Hash();

        List<PackageManifest> temp = [
            new() {
                Id = alias,
                Name = ModelsBuilderPackage.Name,
                AllowTelemetry = true,
                Version = ModelsBuilderPackage.InformationalVersion,
                Extensions = [
                    new {
                        alias = $"{alias}.EntryPoint",
                        name = "Limbo Models Builder Entry Point",
                        type = "backofficeEntryPoint",
                        js = $"/App_Plugins/{alias}/EntryPoint.js?v={cacheBuster}"
                    }
                ],
                Importmap = new PackageManifestImportmap {
                    Imports = new Dictionary<string, string> {
                        {"@limbo/models-builder/auth", $"/App_Plugins/{alias}/ModelsBuilderAuth.js?v={cacheBuster}"},
                        {"@limbo/models-builder/package", $"/App_Plugins/{alias}/ModelsBuilderPackage.js?{cacheBuster}"},
                        {"@limbo/models-builder/service", $"/App_Plugins/{alias}/ModelsBuilderService.js?v={cacheBuster}"}
                    }
                }
            }

        ];

        return await Task.FromResult(temp);

    }

}