using Limbo.Umbraco.ModelsBuilder.Extensions;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Limbo.Umbraco.ModelsBuilder.Composers {

    public class ModelsComposer : IComposer {

        public void Compose(IUmbracoBuilder builder) {

            builder.Services.AddTransient<ModelsGenerator>();
            builder.Services.AddTransient<ModelsSourceGenerator>();

            builder.AddUmbracoOptions<LimboModelsBuilderSettings>();

        }

    }

}