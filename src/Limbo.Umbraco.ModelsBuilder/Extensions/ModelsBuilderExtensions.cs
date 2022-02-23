using Limbo.Umbraco.ModelsBuilder.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;

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

        public static bool HasPropertyType(this TypeModel subject, string propertyAlias, out TypeModel type) {
            
            //IPublishedPropertyType pt = subject.PublishedContentType.GetPropertyType(propertyAlias);
            var pt = subject.ContentType.PropertyTypes.FirstOrDefault(x => x.Alias == propertyAlias);

            if (pt != null) {
                type = subject;
                return true;
            }

            foreach (var composition in subject.Compositions) {

                if (HasPropertyType(composition, propertyAlias, out type)) return true;

            }

            type = null;

            return false;

        }

        public static LazyReadOnlyCollection<T> ToLazyReadOnlyCollection<T>(this IEnumerable<T> collection) {
            return new LazyReadOnlyCollection<T>(() => collection);
        }

    }

}