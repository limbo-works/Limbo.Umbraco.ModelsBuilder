using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace Limbo.Umbraco.ModelsBuilder.Services;

#pragma warning disable 1591

public class ModelsBuilderServiceDependencies {

    private readonly IOptions<LimboModelsBuilderSettings> _modelsBuilderSettings;

    public IWebHostEnvironment Environment { get; }

    public LimboModelsBuilderSettings Settings => _modelsBuilderSettings.Value;

    public OutOfDateModelsStatus OutOfDateModelsStatus { get; }

    public ModelsSourceGenerator SourceGenerator { get; }

    public ModelsBuilderServiceDependencies(IWebHostEnvironment environment, IOptions<LimboModelsBuilderSettings> modelsBuilderSettings, OutOfDateModelsStatus outOfDateModelsStatus, ModelsSourceGenerator sourceGenerator) {
        _modelsBuilderSettings = modelsBuilderSettings;
        Environment = environment;
        OutOfDateModelsStatus = outOfDateModelsStatus;
        SourceGenerator = sourceGenerator;
    }

}