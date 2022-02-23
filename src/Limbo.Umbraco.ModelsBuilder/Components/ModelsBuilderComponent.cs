using Limbo.Umbraco.ModelsBuilder.Extensions;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Dashboards;

#pragma warning disable 1591

namespace Limbo.Umbraco.ModelsBuilder.Components {

    public class ModelsBuilderComponent : IComponent {

        private readonly LimboModelsBuilderSettings _modelsBuilderSettings;
        private readonly DashboardCollection _dashboardCollection;

        public ModelsBuilderComponent(IOptions<LimboModelsBuilderSettings> modelsBuilderSettings, DashboardCollection dashboardCollection) {
            _modelsBuilderSettings = modelsBuilderSettings.Value;
            _dashboardCollection = dashboardCollection;
        }

        public void Initialize() {
            DisableDefaultDashboard();
        }

        public void Terminate() { }

        private void DisableDefaultDashboard() {
            
            // Return right away if the option is set to false
            if (!_modelsBuilderSettings.DisableDefaultDashboard) return;

            // Get the type of the collection
            Type type = typeof(BuilderCollectionBase<IDashboard>);

            // Get a reference to the internal "_items" 
            FieldInfo field = type.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) return;

            // Get the field value (return if not the expected type)
            if (field.GetValue(_dashboardCollection) is not IReadOnlyCollection<IDashboard> dashboards) return;

            // Remove the default Models Builder dashboard from the collection
            dashboards = dashboards
                .Where(x => x.GetType() != typeof(ModelsBuilderDashboard))
                .ToLazyReadOnlyCollection();

            // Update the field value
            field.SetValue(_dashboardCollection, dashboards);

        }

    }

}