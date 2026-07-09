import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

import { ModelsBuilderAuth } from "@limbo/models-builder/auth";
import { ModelsBuilderPackage } from "@limbo/models-builder/package";
import { ModelsBuilderService } from "@limbo/models-builder/service";

export const onInit = (_host, extensionRegistry) => {

    _host.consumeContext(UMB_AUTH_CONTEXT, async (authContext) => {

        const config = authContext.getOpenApiConfiguration();
        ModelsBuilderAuth.TOKEN = config.token;

        ModelsBuilderPackage.serverVariables = await ModelsBuilderService.getServerVariables();

        extensionRegistry.register({
            type: "dashboard",
            alias: "Limbo.Umbraco.ModelsBuilder.Dashboard",
            name: "Models Builder",
            elementName: "limbo-models-builder-dashboard",
            js: () => import("./Elements/Dashboard.js?v=" + ModelsBuilderPackage.cacheBuster),
            weight: -10,
            meta: {
                label: "Models Builder",
                pathname: "modelsBuilder"
            },
            conditions: [
                {
                    alias: "Umb.Condition.SectionAlias",
                    match: "Umb.Section.Settings"
                }
            ]
        });

        if (ModelsBuilderPackage.settings?.disableDefaultDashboard) {
            extensionRegistry.exclude("Umb.Dashboard.ModelsBuilder");
        }

    });

};