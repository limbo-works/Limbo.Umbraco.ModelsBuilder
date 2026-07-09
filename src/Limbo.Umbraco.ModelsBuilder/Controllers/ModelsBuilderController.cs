using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Limbo.Umbraco.ModelsBuilder.Api;
using Limbo.Umbraco.ModelsBuilder.Models;
using Limbo.Umbraco.ModelsBuilder.Services;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skybrud.Essentials.Security.Extensions;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.Authorization;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Controllers;

[ApiController]
[VersionedApiBackOfficeRoute(ModelsBuilderApiConstants.Route)]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
[MapToApi(ModelsBuilderApiConstants.Alias)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = ModelsBuilderApiConstants.GroupName)]
public class ModelsBuilderController : Controller {

    private readonly ILogger<ModelsBuilderController> _logger;
    private readonly IOptions<LimboModelsBuilderSettings> _modelsBuilderSettings;
    private readonly ModelsBuilderService _modelsBuilderService;

    #region Constructors

    public ModelsBuilderController(ILogger<ModelsBuilderController> logger, IOptions<LimboModelsBuilderSettings> modelsBuilderSettings, ModelsBuilderService modelsBuilderService) {
        _logger = logger;
        _modelsBuilderSettings = modelsBuilderSettings;
        _modelsBuilderService = modelsBuilderService;
    }

    #endregion

    #region Public API methods

    [HttpGet]
    [Route("serverVariables")]
    public ActionResult<ServerVariablesResult> GetServerVariables() {
        return new ServerVariablesResult {
            Version = ModelsBuilderPackage.InformationalVersion,
            CacheBuster = ModelsBuilderPackage.InformationalVersion.ToMd5Hash(),
            Settings = new ServerVariablesSettings {
                DisableDefaultDashboard = _modelsBuilderSettings.Value.DisableDefaultDashboard
            }
        };
    }

    [HttpGet("status")]
    public async Task<ActionResult<StatusResult>> GetStatus() {

        try {

            return await _modelsBuilderService.GetStatus();

        } catch (Exception ex) {

            _logger.LogError(ex, "Failed getting status.");

            return InternalServerError("Failed getting status.");

        }

    }

    [HttpGet("build")]
    public async Task<ActionResult<StatusResult>> BuildModels() {

        // TODO: should this be a POST request instead?

        try {

            return await _modelsBuilderService.BuildModels();

        } catch (Exception ex) {

            _logger.LogError(ex, "Failed building models.");

            return InternalServerError("Failed building models.");

        }

    }

    #endregion

    #region Private helper methods

    private ActionResult InternalServerError(string message) {
        return StatusCode(500, new ErrorResult { Message = message });
    }

    #endregion

}