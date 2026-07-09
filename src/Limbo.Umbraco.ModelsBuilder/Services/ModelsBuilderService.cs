using System.Threading.Tasks;
using Limbo.Umbraco.ModelsBuilder.Models;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Skybrud.Essentials.Time;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Services;

public class ModelsBuilderService {

    private readonly ModelsBuilderServiceDependencies _dependencies;

    #region Constructors

    public ModelsBuilderService(ModelsBuilderServiceDependencies dependencies) {
        _dependencies = dependencies;
    }

    #endregion

    #region Member methods

    public async Task<StatusResult> GetStatus() {

        EssentialsTime? lastBuildDate = await _dependencies.SourceGenerator.GetLastBuildDate();

        return new StatusResult {
            IsSuccessful = true,
            Environment = _dependencies.Environment.EnvironmentName,
            Version = ModelsBuilderPackage.SemVersion.ToString(),
            Mode = _dependencies.Settings.ModelsMode,
            IsOutOfDate = _dependencies.OutOfDateModelsStatus.IsOutOfDate,
            LastBuildDate = lastBuildDate,
            Links = [
                new StatusLink {
                    Text = "GitHub",
                    Url = ModelsBuilderPackage.GitHubUrl,
                    Target = "_blank",
                    Rel = "noopener noreferrer",
                    Icon = "icon-github"
                },

                new StatusLink {
                    Text = "Issues",
                    Url = ModelsBuilderPackage.IssuesUrl,
                    Target = "_blank",
                    Rel = "noopener noreferrer",
                    Icon = "icon-bug"
                },

                new StatusLink {
                    Text = "Documentation",
                    Url = ModelsBuilderPackage.DocumentationUrl,
                    Target = "_blank",
                    Rel = "noopener noreferrer",
                    Icon = "icon-book"
                }
            ]
        };

    }

    /// <summary>
    /// Builds the Models Builder models and retrieves the current status.
    /// </summary>
    /// <returns>A task that resolves to the status result after model generation completes.</returns>
    public async Task<StatusResult> BuildModels() {
        await _dependencies.SourceGenerator.BuildModels();
        return await GetStatus();
    }

    /// <summary>
    /// Builds the Models Builder models and retrieves the current status.
    /// </summary>
    /// <param name="settings">The settings to use for model generation.</param>
    /// <returns>A task that resolves to the status result after model generation completes.</returns>
    public async Task<StatusResult> BuildModels(ModelsGeneratorSettings settings) {
        await _dependencies.SourceGenerator.BuildModels(settings);
        return await GetStatus();
    }

    #endregion

}