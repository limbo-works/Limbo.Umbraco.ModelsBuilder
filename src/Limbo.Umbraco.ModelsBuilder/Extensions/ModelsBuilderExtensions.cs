using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;

namespace Limbo.Umbraco.ModelsBuilder.Extensions {

    internal static class ModelsBuilderExtensions {

        public static bool HasValue<T>(this T input) {
            return HasValue<T>(input, out _);
        }

        public static bool HasValue<T>(this T input, out T result) {

            result = input;

            if (input == null) return false;

            // TODO: Validate strings, numbers and enums

            return true;

        }

        internal static IUmbracoBuilder AddUmbracoOptions<TOptions>(this IUmbracoBuilder builder, Action<OptionsBuilder<TOptions>> configure = null) where TOptions : class {

            var umbracoOptionsAttribute = typeof(TOptions).GetCustomAttribute<UmbracoOptionsAttribute>();
            if (umbracoOptionsAttribute is null) {
                throw new ArgumentException($"{typeof(TOptions)} do not have the UmbracoOptionsAttribute.");
            }

            var optionsBuilder = builder.Services.AddOptions<TOptions>()
                .Bind(
                    builder.Config.GetSection(umbracoOptionsAttribute.ConfigurationKey),
                    o => o.BindNonPublicProperties = umbracoOptionsAttribute.BindNonPublicProperties
                )
                .ValidateDataAnnotations();

            configure?.Invoke(optionsBuilder);

            return builder;

        }

    }

}