#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Skybrud.Essentials.Time;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.OpenApi;

namespace Limbo.Umbraco.ModelsBuilder.Api;

public static class ModelsBuilderApiConstants {

    public const string Route = "limbo/modelsbuilder";

    public const string Alias = "limbo-modelsbuilder-v1";

    public const string Name = "Limbo Models Builder API v1";

    public const string GroupName = "Limbo Models Builder";

}

public class ModelsBuilderSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase {

    protected override string ApiName => ModelsBuilderApiConstants.Name;

}

public class ModelsBuilderSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions> {

    public void Configure(SwaggerGenOptions options) {

        options.SwaggerDoc(ModelsBuilderApiConstants.Alias, new OpenApiInfo {
            Title = ModelsBuilderApiConstants.Name,
            Version = "1.0"
        });

        options.OperationFilter<ModelsBuilderSecurityFilter>();

        options.MapType<EssentialsTime>(() => new OpenApiSchema {
            Type = JsonSchemaType.String,
            Format = "date-time"
        });

    }

}