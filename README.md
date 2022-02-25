# Limbo Models Builder

`Limbo.Umbraco.ModelsBuilder` is a custom models builder for Umbraco 9 we use internally at [**@limbo-works**](https://github.com/limbo-works). It's purpose is to add additional features compared to what is available today in the build-in ModelsBuilder in Umbraco 9 and the older community ModelsBuilder (which doesn't work for Umbraco 9).

## Installation

Via <a href="https://www.nuget.org/packages/Limbo.Umbraco.ModelsBuilder/1.0.0-beta004" target="_blank">NuGet</a>:

```
dotnet add package Limbo.Umbraco.ModelsBuilder --version 1.0.0-beta004
```

or:

```
Install-Package Limbo.Umbraco.ModelsBuilder -Version 1.0.0-beta004
```

## Documentation

Coming soon. Meanwhile, have a look at the following sections in this README file:

- [Configuration](#configuration)
- [Events](#events)
- [Notifcations](#notifcations)

## Configuration

### App Settings

The package re-uses the existing `Umbraco.CMS.ModelsBuilder` section in the `appsettings.json` file, and as such supports the same options as Umbraco's build-in ModelsBuilder.

The configuration may also be changed programmatically - either by extending the `ModelsGenerator` class (and replacing it in the DI container), and overriding the `GetDefaultSettings` - or by creating a notification handler for the `GetDefaultSettingsNotification` notification.

#### Models Mode

The `ModelsMode` option specifies how Umbraco's build-in Models Builder should work. To avoid conflicts with this package, the value should always be set to `Nothing`, meaning that Umbraco or the build-in Models Builder will not generate any models on it's own.

#### Models Directory

If not specified, Umbraco defaults to the `~/umbraco/models` directory. If the `AcceptUnsafeModelsDirectory` options is set to `true`, the directory may be outside of the website root.

#### Models Namespace

If not specified, Umbraco defaults to the `Umbraco.Cms.Web.Common.PublishedModels` namespace.

#### Use Directories

Defaults to <c>true</c>. This option is specific to this package, and if enabled, the models will be placed in sub directories based on their type - eg. **Members** for member types, **Media** for media types and then **Content** and **Elements** for content types and element types respectively.

Regardless of this option being enabled or not, the directory to which the individual models will be saved, may still be overriden through events/notifications.

#### Disable Default Dashboard

Defaults to <c>true</c>. This options lets you disable the dashboard of the embedded Models Builder, as the dashboard has little purpose when using this package instead.

#### Containers

*Not ready yet*

### Other

#### Nested Files

When having a lot of generated class files and custom partials, the file tree in *Solution Explorer* can feel a bit cluttered. To make sure generated class files are shown nested under their custom partial, you can add the following to your `.csproj` file:

```xml
<ItemGroup>
  <Compile Update="**\*.generated.cs">
    <DependentUpon>$([System.String]::Copy(%(Filename)).Replace('.generated', '.cs'))</DependentUpon>
  </Compile>
</ItemGroup>
```

This means that any files matching the pattern will automatically be nested under their custom partial, so you or this package doesn't have to add a `<Compile>` element for each generated file. Technically this also applies to generated files that doesn't have a custom partial, but Visual Studio will just ignore if the file doesn't exist.

## Building models

The plan is that models should be build from the backoffice (like Umbraco 9 supports via the models mode). The dashboard for this hasn't been implemented yet, so the models can be build by accessing the `/umbraco/backoffice/Limbo/ModelsBuilder/GenerateModels` endpoint.

A new generation of the models may also be initiated programmatically:

```csharp
// Get the settings
ModelsGeneratorSettings settings = _modelsGenerator.GetDefaultSettings();

// Generate definitions for all the models
TypeModelList models = _modelsGenerator.GetModels();

// Generate the source code and save the models to disk
_sourceGenerator.SaveModels(models, settings);
```

## Events

<table>
  <thead>
    <tr>
      <td align="left">
        :warning:
      </td>
      <td align="left" width="100%">
          <strong>NOTICE</strong>
      </td>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td colspan="2">
          As Umbraco 9 has introduced a notifcations pattern as an alternative to events, it's recommended to use <a href="#notifications"><strong>Notifcations</strong></a> instead. The events in this package may very likely be removed at a later time.
      </td>
    </tr>
  </tbody>
</table>

To control how models are generated, the `ModelsGenerator` class features a `GetModelsReturning` that you can hook into. The example below shows how to modify a few properties - eg. setting `property.IsIgnored` to `true` will make the models generator skip the property in the generated C# file.

It's also possible to control the JSON.net settings of the property - eg. ignore the property in the JSON outout, but without removing the property from the generated C# model.

```csharp
using Limbo.Umbraco.ModelsBuilder.Events;
using Limbo.Umbraco.ModelsBuilder.Models;
using Limbo.Umbraco.ModelsBuilder.Services;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Composing;

namespace UmbracoNineTests.ModelsBuilder {
    
    public class MyModelsBuilderComponent : IComponent {
        
        private readonly ModelsGenerator _modelsGenerator;

        public MyModelsBuilderComponent(ModelsGenerator modelsGenerator) {
            _modelsGenerator = modelsGenerator;
        }
        
        public void Initialize() {
            _modelsGenerator.GetModelsReturning += ModelsGeneratorOnGetModelsReturning;
        }

        public void Terminate() {
            _modelsGenerator.GetModelsReturning -= ModelsGeneratorOnGetModelsReturning;
        }

        private void ModelsGeneratorOnGetModelsReturning(object sender, GetModelsEventArgs e) {

            foreach (TypeModel model in e.Models) {

                foreach (PropertyModel property in model.Properties) {

                    if (property.Alias == "umbracoNavihide") {
                        property.ClrName = "IsHidden";
                        property.JsonNetSettings.PropertyName = "hidden";
                        property.JsonNetSettings.Order = 123;
                        property.JsonNetSettings.NullValueHandling = NullValueHandling.Ignore;
                        property.JsonNetSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                    }

                    if (property.Alias == "keywords") {
                        property.IsIgnored = true;
                    }

                    if (property.Alias == "inboundRedirects") {
                        property.JsonNetSettings.Ignore = true;
                    }

                    if (property.Alias == "seoPreview") {
                        property.IsIgnored = true;
                    }

                }

            }

        }

    }

}
```

## Notifcations

### GetDefaultSettingsNotification

While the same settings can be configured in the `appSettings.json` file, they can also be set programmatically via the `GetDefaultSettingsNotification` notification:

```csharp
using Limbo.Umbraco.ModelsBuilder.Notifications;
using Limbo.Umbraco.ModelsBuilder.Settings;
using Umbraco.Cms.Core.Events;

namespace UmbracoNineTests.ModelsBuilder {
    
    public class GetDefaultSettingsNotificationHandler : INotificationHandler<GetDefaultSettingsNotification> {
        
        public void Handle(GetDefaultSettingsNotification notification) {

            ModelsGeneratorSettings settings = notification.Settings;

            // Should be a full path as this point
            settings.DefaultModelsPath = notification.HostingEnvironment.MapPathContentRoot("~/../code/Models/Umbraco");
            
            // Overwrite the namespace
            settings.DefaultNamespace = "UmbracoNineTests.Models.Umbraco";

            // Enable (or disable) sub directories
            settings.UseDirectories = true;

        }

    }

}
```

### GetModelsNotification

The `GetModelsNotification` notification can be used to control the models returned by the `ModelsGenerator.GetModelsReturning` method. The example below shows how to modify a few properties - eg. setting `property.IsIgnored` to `true` will make the models generator skip the property in the generated C# file.

It's also possible to control the JSON.net settings of the property - eg. ignore the property in the JSON outout, but without removing the property from the generated C# model.

```csharp
using Limbo.Umbraco.ModelsBuilder.Models;
using Limbo.Umbraco.ModelsBuilder.Notifications;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Events;

namespace UmbracoNineTests.ModelsBuilder {
    
    public class GetModelsNotificationHandler : INotificationHandler<GetModelsNotification> {

        public void Handle(GetModelsNotification notification) {

            foreach (TypeModel model in notification.Models) {

                foreach (PropertyModel property in model.Properties) {

                    if (property.Alias == "umbracoNaviHide") {
                        property.ClrName = "IsHidden";
                        property.JsonNetSettings.PropertyName = "hidden";
                        property.JsonNetSettings.Order = 123;
                        property.JsonNetSettings.NullValueHandling = NullValueHandling.Ignore;
                        property.JsonNetSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                    }

                    if (property.Alias == "keywords") {
                        property.IsIgnored = true;
                    }

                    if (property.Alias == "inboundRedirects") {
                        property.JsonNetSettings.Ignore = true;
                    }

                    if (property.Alias == "seoPreview") {
                        property.IsIgnored = true;
                    }

                }


            }

        }

    }

}
```
