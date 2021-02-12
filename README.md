# FileParty
![dotnet_test](https://github.com/JankwareDotCom/FileParty/workflows/dotnet_test/badge.svg)

| Package | Description | Version |
| ------- | ----------- | ------- |
|FileParty.Core|Registration, Factory, and Watcher|[![Nuget Package](https://badgen.net/nuget/v/FileParty.Core)](https://www.nuget.org/packages/FileParty.Core/)|
|FileParty.Providers.FileSystem|Storage Providers for FileSystem|[![Nuget Package](https://badgen.net/nuget/v/FileParty.Providers.FileSystem)](https://www.nuget.org/packages/FileParty.Providers.FileSystem/)|
|FileParty.Providers.AWS.S3|Storage Providers for AWS S3 Buckets|[![Nuget Package](https://badgen.net/nuget/v/FileParty.Providers.AWS.S3)](https://www.nuget.org/packages/FileParty.Providers.AWS.S3/)|

Providing a common set of methods for interacting with files across storage providers 

## How to Use

### Registration
FileParty uses `Microsoft.Extensions.DependencyInjection` to register itself.  Dependency Injection is not explicitly required to use FileParty.

Register File Party by calling `this.AddFileParty()` either on an existing `IServiceCollection` or any object. If `this` is not an `IServiceCollection` 
a new Service Collection will be made and returned as a result of the method call; When not using Dependency Injection, make sure to assign this to a
variable.

Register each desired module calling a configuration action `x => x.AddModule<ModuleName>(defaultConfigObject)` where `defaultConfigObject` is 
a `StorageProviderConfiguration<TModule>`

This command which will register a given module and its storage providers with the Service Collection. Any quantity of modules may be added this way.

If desired, a default module may be specified, when provided this module will be the default storage provider type provided when either 
`FilePartyFactory` or `AsyncFilePartyFactory` returns a storage provider.  When not provided, the default module shall be the last module loaded using the 
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
- Use Dependency Injection for either an `IAsyncFilePartyFactory` or `IFilePartyFactory` and call one of the following methods:
  - `GetAsyncStorageProvider()` or `GetStorageProvider()` to get a storage provider from the default module with the default 
    configuration for that module, or
  - `GetAsyncStorageProvider<TModule>()` or `GetStorageProvider<TModule>()` to get a storage provider from a specified module 
    with the default configuration for that module, or
  - `GetAsyncStorageProvider<TModule>(StorageProviderConfiguration<TModule>)` or `GetStorageProvider<TModule>(StorageProviderConfiguration<TModule>)`
    to get a storage provider from a specified module with a specified configuration.
    
The following methods are also available in the factory to return a more limited action set from the storage provider
- Storage Readers, which include methods to check to see if a file exists, to read a file stream, and get information about a file 
  - `GetAsyncStorageReader()` or `GetStorageReader()`
  - `GetAsyncStorageReader<TModule>()` or `GetStorageReader<TModule>()`
  - `GetAsyncStorageReader<TModule>(StorageProviderConfiguration<TModule>)` or `GetStorageReader<TModule>(StorageProviderConfiguration<TModule>)`
- Storage Writers, which include methods to Write and Delete a file stream
  - `GetAsyncStorageWriter()` or `GetStorageWriter()`
  - `GetAsyncStorageWriter<TModule>()` or `GetStorageWriter<TModule>()`
  - `GetAsyncStorageWriter<TModule>(StorageProviderConfiguration<TModule>)` or `GetStorageWriter<TModule>(StorageProviderConfiguration<TModule>)`

### Subscribing to Write Progress Status
Use Dependency Injection to get an `IWriteProgressSubscriptionManager`.  Use a delegate to subscribe to all or some write progress events.  Each module should
have write progress updates implemented.  When creating a `FilePartyWriteRequest` use this Id to subscribe to write progress updates for a specific file stream.

### Changing Default Configs On-The-Fly
Use Dependency Injection to get an `IFilePartyFactoryModifier` and use the sole method to set a new default module and configuration.  This will change which 
Storage Providers are returned by the default factory methods.

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
- Please make PRs into develop and remember to increment the version number for each project.
- Please name branches in the following formats 
 - /bugfix/issue-fixed
 - /feature/feature-added
- Release PRs will go into main
- Please write unit tests
- If adding a new storage provider
 - please make a new repository
 - use the following namespace format, marking External as it is not maintained by Jankware: FileParty.Providers.External.YourProvider
  - let us know about it, we're very interested
  - we may ask to make this part of the core code in the future
 
