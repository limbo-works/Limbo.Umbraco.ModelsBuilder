using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Limbo.Umbraco.ModelsBuilder.Services;

/// <summary>
/// Class responsible for loading the dependencies for <see cref="ModelsSourceGenerator"/>. This is done to make
/// extending the <see cref="ModelsSourceGenerator"/> class easier as well as avoid breaking changes should we need
/// another dependency in the future.
/// </summary>
public class ModelsSourceGeneratorDependencies {

    private readonly IOptions<LimboModelsBuilderSettings> _modelsBuilderSettings;

    #region Properties

    /// <summary>
    /// Gets a reference to the current <see cref="IWebHostEnvironment"/>.
    /// </summary>
    public IWebHostEnvironment WebHostEnvironment { get; }

    /// <summary>
    /// Gets a reference to the current <see cref="IHostingEnvironment"/>.
    /// </summary>
    public IHostingEnvironment HostingEnvironment { get; }

    /// <summary>
    /// Gets a reference to the current <see cref="LimboModelsBuilderSettings"/>.
    /// </summary>
    public LimboModelsBuilderSettings ModelsBuilderSettings => _modelsBuilderSettings.Value;

    /// <summary>
    /// Gets a reference to the current <see cref="OutOfDateModelsStatus"/>.
    /// </summary>
    public OutOfDateModelsStatus OutOfDateModels { get; }

    /// <summary>
    /// Gets a reference to the current <see cref="ModelsGenerator"/>.
    /// </summary>
    public ModelsGenerator ModelsGenerator { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance based on the specified dependencies.
    /// </summary>
    /// <param name="webHostEnvironment"></param>
    /// <param name="hostingEnvironment"></param>
    /// <param name="modelsBuilderSettings"></param>
    /// <param name="outOfDateModels"></param>
    /// <param name="modelsGenerator"></param>
    public ModelsSourceGeneratorDependencies(IWebHostEnvironment webHostEnvironment,
        IHostingEnvironment hostingEnvironment,
        IOptions<LimboModelsBuilderSettings> modelsBuilderSettings,
        OutOfDateModelsStatus outOfDateModels,
        ModelsGenerator modelsGenerator) {
        _modelsBuilderSettings = modelsBuilderSettings;
        WebHostEnvironment = webHostEnvironment;
        HostingEnvironment = hostingEnvironment;
        OutOfDateModels = outOfDateModels;
        ModelsGenerator = modelsGenerator;
    }

    #endregion

}