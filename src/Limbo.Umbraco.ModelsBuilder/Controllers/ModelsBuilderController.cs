using Limbo.Umbraco.ModelsBuilder.Models;
using Limbo.Umbraco.ModelsBuilder.Models.Api;
using Limbo.Umbraco.ModelsBuilder.Services;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Controllers {
    
    [PluginController("Limbo")]
    public class ModelsBuilderController : UmbracoAuthorizedApiController {
        
        private readonly ILogger<ModelsBuilderController> _logger;
        private readonly OutOfDateModelsStatus _outOfDateModelsStatus;
        private readonly LimboModelsBuilderSettings _modelsBuilderSettings;
        private readonly ModelsGenerator _modelsGenerator;
        private readonly ModelsSourceGenerator _sourceGenerator;

        public ModelsBuilderController(ILogger<ModelsBuilderController> logger, OutOfDateModelsStatus outOfDateModelsStatus,
            IOptions<LimboModelsBuilderSettings> modelsBuilderSettings, ModelsGenerator modelsGenerator, ModelsSourceGenerator sourceGenerator) {
            _logger = logger;
            _outOfDateModelsStatus = outOfDateModelsStatus;
            _modelsBuilderSettings = modelsBuilderSettings.Value;
            _modelsGenerator = modelsGenerator;
            _sourceGenerator = sourceGenerator;
        }
        
        [HttpGet]
        public object GetStatus() {

            try {
                
                return new StatusResult(_modelsBuilderSettings, _outOfDateModelsStatus, _sourceGenerator);

            } catch (Exception ex) {

                _logger.LogError(ex, "Failed getting status.");

                return new { success = false };

            }

        }

        [HttpGet]
        public object GenerateModels() {

            try {

                // Get the default settings
                ModelsGeneratorSettings settings = _modelsGenerator.GetDefaultSettings();

                // Generate definitions for all the models
                TypeModelList models = _modelsGenerator.GetModels();

                // Generate the source code and save the models to disk
                _sourceGenerator.SaveModels(models, settings);

                // Return a new status result
                return GetStatus();

            } catch (Exception ex) {

                _logger.LogError(ex, "Failed building models.");

                return new { success = false };

            }

        }

    }

}