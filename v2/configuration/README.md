# Configuration




## App Settings

The package re-uses Umbraco's existing `Umbraco.CMS.ModelsBuilder` section in the `appsettings.json` file, and as such supports the same options as Umbraco's build-in ModelsBuilder.

The configuration may also be changed programmatically - either by extending the `ModelsGenerator` class (and replacing it in the DI container), and overriding the `GetDefaultSettings` - or by creating a notification handler for the `GetDefaultSettingsNotification` notification.

### Models Mode

The `ModelsMode` option specifies how Umbraco's build-in Models Builder should work. To avoid conflicts with this package, the value should always be set to `Nothing`, meaning that Umbraco or the build-in Models Builder will not generate any models on it's own.

### Models Directory

If not specified, Umbraco defaults to the `~/umbraco/models` directory. If the `AcceptUnsafeModelsDirectory` options is set to `true`, the directory may be outside of the website root.

### Models Namespace

If not specified, Umbraco defaults to the `Umbraco.Cms.Web.Common.PublishedModels` namespace.

### Use Directories

Defaults to <c>true</c>. This option is specific to this package, and if enabled, the models will be placed in sub directories based on their type - eg. **Members** for member types, **Media** for media types and then **Content** and **Elements** for content types and element types respectively.

Regardless of this option being enabled or not, the directory to which the individual models will be saved, may still be overriden through events/notifications.

### Disable Default Dashboard

Defaults to <c>true</c>. This options lets you disable the dashboard of the embedded Models Builder, as the dashboard has little purpose when using this package instead.





## Other

### Nested Files

When having a lot of generated class files and custom partials, the file tree in *Solution Explorer* can feel a bit cluttered. To make sure generated class files are shown nested under their custom partial, you can add the following to your `.csproj` file:

```xml
<ItemGroup>
  <Compile Update="**\*.generated.cs">
    <DependentUpon>$([System.String]::Copy(%(Filename)).Replace('.generated', '.cs'))</DependentUpon>
  </Compile>
</ItemGroup>
```

This means that any files matching the pattern will automatically be nested under their custom partial, so you or this package doesn't have to add a `<Compile>` element for each generated file. Technically this also applies to generated files that doesn't have a custom partial, but Visual Studio will just ignore if the file doesn't exist.