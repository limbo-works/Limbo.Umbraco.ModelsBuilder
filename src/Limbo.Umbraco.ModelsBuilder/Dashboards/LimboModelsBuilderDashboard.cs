using System;
using Umbraco.Cms.Core.Dashboards;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Dashboards {
    
    public class LimboModelsBuilderDashboard : IDashboard {
        
        public string Alias => "limboModelsBuilder";

        public string[] Sections => new[] { "settings" };

        public string View => $"/App_Plugins/Limbo.Umbraco.ModelsBuilder/Views/Dashboard.html?v={ModelsBuilderPackage.SemVersion}";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();

    }

}