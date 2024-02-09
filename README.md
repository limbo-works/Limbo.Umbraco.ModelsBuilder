# Limbo Models Builder

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/limbo-works/Limbo.Umbraco.ModelsBuilder/blob/v13/main/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/vpre/Limbo.Umbraco.ModelsBuilder.svg)](https://www.nuget.org/packages/Limbo.Umbraco.ModelsBuilder)
[![NuGet](https://img.shields.io/nuget/dt/Limbo.Umbraco.ModelsBuilder.svg)](https://www.nuget.org/packages/Limbo.Umbraco.ModelsBuilder)
[![Umbraco Marketplace](https://img.shields.io/badge/umbraco-marketplace-%233544B1)](https://marketplace.umbraco.com/package/limbo.umbraco.modelsbuilder)


`Limbo.Umbraco.ModelsBuilder` is a custom models builder for Umbraco 9+ we use internally at [**@limbo-works**](https://github.com/limbo-works). It's purpose is to add additional features compared to what is available today in the build-in ModelsBuilder in Umbraco 9+ and the older community ModelsBuilder (which doesn't work for Umbraco 9+).



<br /><br />

## Installation

Via [**NuGet**](https://www.nuget.org/packages/Limbo.Umbraco.ModelsBuilder/13.0.0):

```
dotnet add package Limbo.Umbraco.ModelsBuilder --version 13.0.0
```

or:

```
Install-Package Limbo.Umbraco.ModelsBuilder -Version 13.0.0
```



<br /><br />

## Documentation

Coming soon. Meanwhile, have a look at the following sections in this README file:

- [Configuration](#configuration)
- [Events](#events)
- [Notifications](#notifications)
- [Usage Tips](#usage)




<br /><br />

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



<br /><br />

## Building models

The package adds a new dashboard to the *Settings* section. The dashboard then let's you see a quick status about your generated models, as well as generate your models in case they are not up-to-date:

![image](https://user-images.githubusercontent.com/3634580/155564176-8adb8147-cfb2-44d5-b548-42c5b4d1ec88.png)

The dashboard uses an authenticated endpoint, which you may also call directly by accessing the `/umbraco/backoffice/Limbo/ModelsBuilder/GenerateModels`.

A new generation of the models may also be initiated programmatically:

```csharp
// Get the settings
ModelsGeneratorSettings settings = _modelsGenerator.GetDefaultSettings();

// Generate definitions for all the models
TypeModelList models = _modelsGenerator.GetModels();

// Generate the source code and save the models to disk
_sourceGenerator.SaveModels(models, settings);
```



<br /><br />

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



<br /><br />

## Notifications

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



<br /><br />

## Usage

### Strongly-Type All the Things

Using the generated models, not only can you access the property data in a strongly-typed fashion, but you can also access information about the Content Type and Properties - no more magic strings!

Some examples:

`MyContentType.ModelTypeAlias` returns the string representation of the Content Type's alias. This can help in code where you are checking the type of an IPublishedContent:

```csharp
if(currentPage.ContentType.Alias == MyContentType.ModelTypeAlias)
{
	//Do stuff ...
}
else if(currentPage.ContentType.Alias == MyOtherType.ModelTypeAlias)
{
	//Do something else ...
}
else 
{
	//No match, it is some other Content Type
}
```

If you want more than just the alias, use `MyContentType.GetModelContentType()` which returns an `IPublishedContentType`:

```csharp

@using Umbraco.Cms.Core.PublishedCache;
@inject IPublishedSnapshotAccessor  PublishedSnapshotAccessor 

...

var theDocType = MyContentType.GetModelContentType(PublishedSnapshotAccessor);
var publishedItemType = theDocType.ItemType;
```

`MyContentType.GetModelPropertyType()` returns an `IPublishedPropertyType` for a specified property from the Content Type. Once you have the property, you can get its string alias or other information about the property:

```csharp

@using Umbraco.Cms.Core.PublishedCache;
@inject IPublishedSnapshotAccessor  PublishedSnapshotAccessor 

...

var theProperty = MyContentType.GetModelPropertyType(PublishedSnapshotAccessor, n=> n.MySpecialProperty);

var propAlias = theProperty.Alias;
var propEditor = theProperty.EditorAlias;
```
