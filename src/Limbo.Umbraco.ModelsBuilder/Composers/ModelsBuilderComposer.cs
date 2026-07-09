using Limbo.Umbraco.ModelsBuilder.Api;
using Limbo.Umbraco.ModelsBuilder.Extensions;
using Limbo.Umbraco.ModelsBuilder.Manifests;
using Limbo.Umbraco.ModelsBuilder.Services;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Manifest;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Composers;

public class ModelsBuilderComposer : IComposer {

    public void Compose(IUmbracoBuilder builder) {

        builder
            .AddUmbracoOptions<LimboModelsBuilderSettings>()
            .Services
            .ConfigureOptions<ModelsBuilderSwaggerGenOptions>()
            .AddSingleton<IPackageManifestReader, ModelsBuilderPackageManifestReader>()
            .AddSingleton<ModelsBuilderService>()
            .AddSingleton<ModelsBuilderServiceDependencies>()
            .AddSingleton<ModelsGenerator>()
            .AddSingleton<ModelsGeneratorDependencies>()
            .AddSingleton<ModelsSourceGenerator>()
            .AddSingleton<ModelsSourceGeneratorDependencies>();

    }

}