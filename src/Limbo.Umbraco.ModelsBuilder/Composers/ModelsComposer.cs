using Limbo.Umbraco.ModelsBuilder.BackOffice.Assets;
using Limbo.Umbraco.ModelsBuilder.Components;
using Limbo.Umbraco.ModelsBuilder.Extensions;
using Limbo.Umbraco.ModelsBuilder.Services;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Composers {

    public class ModelsComposer : IComposer {

        public void Compose(IUmbracoBuilder builder) {

            builder.Services
                .AddSingleton<ModelsGenerator>()
                .AddSingleton<ModelsGeneratorDependencies>()
                .AddSingleton<ModelsSourceGenerator>();

            builder.AddUmbracoOptions<LimboModelsBuilderSettings>();

            builder.Components().Append<ModelsBuilderComponent>();
            
            builder.BackOfficeAssets()
                .Append<ModelsBuilderCssAsset>()
                .Append<ModelsBuilderDashboardAsset>();

        }

    }

}