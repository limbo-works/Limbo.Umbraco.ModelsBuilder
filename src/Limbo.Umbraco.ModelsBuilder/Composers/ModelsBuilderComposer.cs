﻿using Limbo.Umbraco.ModelsBuilder.Components;
using Limbo.Umbraco.ModelsBuilder.Extensions;
using Limbo.Umbraco.ModelsBuilder.Manifests;
using Limbo.Umbraco.ModelsBuilder.Services;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Composers;

public class ModelsBuilderComposer : IComposer {

    public void Compose(IUmbracoBuilder builder) {

        builder.Services
            .AddSingleton<ModelsGenerator>()
            .AddSingleton<ModelsGeneratorDependencies>()
            .AddSingleton<ModelsSourceGenerator>()
            .AddSingleton<ModelsSourceGeneratorDependencies>();

        builder.AddUmbracoOptions<LimboModelsBuilderSettings>();

        builder.Components().Append<ModelsBuilderComponent>();

        builder.ManifestFilters().Append<ModelsBuilderManifest>();

    }

}