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
                .AddTransient<ModelsGenerator>()
                .AddTransient<ModelsGeneratorDependencies>()
                .AddTransient<ModelsSourceGenerator>();

            builder.AddUmbracoOptions<LimboModelsBuilderSettings>();

        }

    }

}