# FileParty

![dotnet_test](https://github.com/JankwareDotCom/FileParty/workflows/dotnet_test/badge.svg)
![CodeQL](https://github.com/JankwareDotCom/FileParty/actions/workflows/codeql-analysis.yml/badge.svg)

| Package | Description | Version |
| ------- | ----------- | ------- |
|FileParty.Core|Registration, Factory, and Watcher|[![Nuget Package](https://badgen.net/nuget/v/FileParty.Core)](https://www.nuget.org/packages/FileParty.Core/)|
|FileParty.Providers.FileSystem|Storage Providers for FileSystem|[![Nuget Package](https://badgen.net/nuget/v/FileParty.Providers.FileSystem)](https://www.nuget.org/packages/FileParty.Providers.FileSystem/)|
|FileParty.Providers.AWS.S3|Storage Providers for AWS S3 Buckets|[![Nuget Package](https://badgen.net/nuget/v/FileParty.Providers.AWS.S3)](https://www.nuget.org/packages/FileParty.Providers.AWS.S3/)|
|FileParty.Providers.Azure.Storage|Storage Providers for Azure Blob Storage ([separate repo](https://github.com/JankwareDotCom/FileParty.Providers.Azure.Storage))|[![Nuget Package](https://badgen.net/nuget/v/FileParty.Providers.Azure.Storage)](https://www.nuget.org/packages/FileParty.Providers.Azure.Storage/)|

Providing a common set of methods for interacting with files across storage providers

## Supported frameworks

Every package ships these assets:

| Asset | Used by |
| ----- | ------- |
| `netstandard2.0` | .NET Framework 4.6.2+ (including 4.8), and anything else netstandard2.0-compatible |
| `netstandard2.1` | .NET Core 3.x, .NET 5–7 |
| `net8.0` | .NET 8 and 9 |
| `net10.0` | .NET 10+ |

Behavior is the same on every target.

## Dependency ranges

FileParty keeps its dependency floors low so you can upgrade FileParty without being forced to upgrade
AWS, Azure, or Microsoft.Extensions packages. NuGet picks the lowest version that satisfies every
package in your graph, so your own pins win as long as they fall inside these ranges.

| Package | Dependency | netstandard2.0 / 2.1 | net8.0 | net10.0 |
| ------- | ---------- | -------------------- | ------ | ------- |
| FileParty.Core | Microsoft.Extensions.DependencyInjection | >= 5.0.0 | >= 8.0.0 | >= 9.0.0 |
| FileParty.Providers.AWS.S3 | AWSSDK.S3 | [3.7.400, 5.0.0) | [3.7.400, 5.0.0) | [3.7.400, 5.0.0) |
| FileParty.Providers.AWS.S3 | AWSSDK.SecurityToken | [3.7.1, 5.0.0) | [3.7.1, 5.0.0) | [3.7.1, 5.0.0) |
| FileParty.Providers.AWS.S3 | System.Threading.Tasks.Extensions | >= 4.5.4 (netstandard2.0 only) | built in | built in |
| FileParty.Providers.Azure.Storage | Azure.Storage.Blobs | see that package | | |

Notes:

- **AWSSDK v3.7 and v4** both work with the same FileParty.Providers.AWS.S3 package. Each AWS v3.7 package
  caps `AWSSDK.Core` below 4.0, so to use v4, reference `AWSSDK.S3` and `AWSSDK.SecurityToken` 4.x
  directly in your project.
- FileParty.Core depends on the full `Microsoft.Extensions.DependencyInjection` package, not just
  `.Abstractions`, because it builds its own internal `ServiceCollection`/`ServiceProvider`.
- `samples/compat` is a .NET Framework 4.8 + .NET 6 consumer that pins older dependency versions and
  restores against freshly packed FileParty packages. CI runs it on every push. Any forced upgrade or
  downgrade (NU1605/NU1608) fails the build.
- Every pack runs [package validation](https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/overview)
  against the latest release on nuget.org, and CI also validates against 1.3.0, so public API breaks fail the build.

## How to Use

### Registration

FileParty uses `Microsoft.Extensions.DependencyInjection` to register itself. Dependency Injection is not explicitly
required to use FileParty.

Register File Party by calling `this.AddFileParty()` either on an existing `IServiceCollection` or any object. If `this`
is not an `IServiceCollection`
a new Service Collection will be made and returned as a result of the method call; When not using Dependency Injection,
make sure to assign this to a variable.

Register each desired module calling a configuration action `x => x.AddModule<ModuleName>(defaultConfigObject)`
where `defaultConfigObject` is a `StorageProviderConfiguration<TModule>`

This command which will register a given module and its storage providers with the Service Collection. Any quantity of
modules may be added this way.

If desired, a default module may be specified, when provided this module will be the default storage provider type
provided when either
`FilePartyFactory` or `AsyncFilePartyFactory` returns a storage provider. When not provided, the default module shall be
the last module loaded using the
`AddModule<TModule>(config)` command.

```c#
this.AddFileParty(config => 
    config.AddModule<FileSystemModule>(new FileSystemConfiguration("C:\FilePartyBaseDirectory"))
          .SetDefaultModule<FilePartyModule>()
    
    )
```

### Getting a Storage Provider

When you need access to an `IAsyncStorageProvider` or `IStorageProvider` there are a few options:

- Use Dependency Injection for either an `IAsyncStorageProvider` or `IStorageProvider` and the appropriate factory will
  return a instance from the default module, or
- Use Dependency Injection for either an `IAsyncFilePartyFactory` or `IFilePartyFactory` and call one of the following
  methods:
    - `GetAsyncStorageProvider()` or `GetStorageProvider()` to get a storage provider from the default module with the
      default configuration for that module, or
    - `GetAsyncStorageProvider<TModule>()` or `GetStorageProvider<TModule>()` to get a storage provider from a specified
      module with the default configuration for that module, or
    - `GetAsyncStorageProvider<TModule>(StorageProviderConfiguration<TModule>)`
      or `GetStorageProvider<TModule>(StorageProviderConfiguration<TModule>)`
      to get a storage provider from a specified module with a specified configuration.

The following methods are also available in the factory to return a more limited action set from the storage provider

- Storage Readers, which include methods to check to see if a file exists, to read a file stream, and get information
  about a file
    - `GetAsyncStorageReader()` or `GetStorageReader()`
    - `GetAsyncStorageReader<TModule>()` or `GetStorageReader<TModule>()`
    - `GetAsyncStorageReader<TModule>(StorageProviderConfiguration<TModule>)`
      or `GetStorageReader<TModule>(StorageProviderConfiguration<TModule>)`
- Storage Writers, which include methods to Write and Delete a file stream
    - `GetAsyncStorageWriter()` or `GetStorageWriter()`
    - `GetAsyncStorageWriter<TModule>()` or `GetStorageWriter<TModule>()`
    - `GetAsyncStorageWriter<TModule>(StorageProviderConfiguration<TModule>)`
      or `GetStorageWriter<TModule>(StorageProviderConfiguration<TModule>)`

### Subscribing to Write Progress Status

Use Dependency Injection to get an `IWriteProgressSubscriptionManager`. Use a delegate to subscribe to all or some write
progress events. Each module should have write progress updates implemented. When creating a `FilePartyWriteRequest` use
this Id to subscribe to write progress updates for a specific file stream.

### Changing Default Configs On-The-Fly

Use Dependency Injection to get an `IFilePartyFactoryModifier` and use the sole method to set a new default module and
configuration. This will change which Storage Providers are returned by the default factory methods.

## Building

The SDK is pinned in `global.json` (.NET 10). Tests target `net10.0`; on Windows they also target `net48`,
which runs them against the `netstandard2.0` build.

```bash
dotnet test test/FileParty.Core.Tests -c Release
dotnet pack src/FileParty.Core -c Release -p:GeneratePackageOnBuild=false -o artifacts/packages
dotnet build samples/compat -p:CompatProfile=low
```

## How to Extend

Create a project that implements:

- BaseAsyncStorageProvider
- BaseStorageProvider
- BaseFilePartyModule

Please be mindful to include updates for write progress.

Additional services may be registered in the module's independent service collection.

## Questions / Issues / Concerns

Just make an issue on the Repo.

- Contributions are welcome.
- Please make PRs into develop and remember to increment the version number for each project. All packages share one version.
- Dependency floors in `src/` are a compatibility promise (see "Dependency ranges"). Dependabot only opens
  security PRs for them (see `.github/dependabot.yml`). Raise a floor only on purpose, and update the README and CHANGELOG when you do.
- Please name branches in the following formats
    - /bugfix/issue-fixed
    - /feature/feature-added
- Release PRs will go into main
- Please write unit tests
- If adding a new storage provider
    - please make a new repository
    - use the following namespace format, marking External as it is not maintained by Jankware:
      FileParty.Providers.External.YourProvider
        - let us know about it, we're very interested
        - we may ask to make this part of the core code in the future