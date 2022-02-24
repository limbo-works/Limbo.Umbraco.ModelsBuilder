using Limbo.Umbraco.ModelsBuilder.Services;
using Newtonsoft.Json;
using Skybrud.Essentials.Time;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Models.Api {
    
    public class StatusResult {

        #region Properties

        [JsonProperty("success")]
        public bool IsSuccessful { get; }
        
        [JsonProperty("version")]
        public string Version { get; }
        
        [JsonProperty("mode")]
        public string Mode { get; }
        
        [JsonProperty("isOutOfDate")]
        public bool IsOutOfDate { get; }
        
        [JsonProperty("lastBuildDate")]
        public EssentialsTime LastBuildDate { get; }
        
        [JsonProperty("links")]
        public List<StatusLink> Links { get; }

        #endregion

        #region Constructors

        public StatusResult(ModelsBuilderSettings settings, OutOfDateModelsStatus outOfDate, ModelsSourceGenerator sourceGenerator) {
            IsSuccessful = true;
            Version = ModelsBuilderPackage.SemVersion.ToString();
            Mode = settings.ModelsMode.ToString();
            IsOutOfDate = outOfDate.IsOutOfDate;
            LastBuildDate = sourceGenerator.GetLastBuildDate();
            Links = new List<StatusLink> {
                new() {
                    Text = "GitHub",
                    Url = ModelsBuilderPackage.GitHubUrl,
                    Target = "_blank",
                    Rel = "noopener noreferrer",
                    Icon = "fa fa-github"
                },
                new() {
                    Text = "Issues",
                    Url = ModelsBuilderPackage.IssuesUrl,
                    Target = "_blank",
                    Rel = "noopener noreferrer",
                    Icon = "fa fa-bug"
                },
                new() {
                    Text = "Documentation",
                    Url = ModelsBuilderPackage.DocumentationUrl,
                    Target = "_blank",
                    Rel = "noopener noreferrer",
                    Icon = "fa fa-book"
                }
            };
        }

        #endregion

    }
}