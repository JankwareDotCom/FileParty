# FileParty
![dotnet_test](https://github.com/JankwareDotCom/FileParty/workflows/dotnet_test/badge.svg)

| Package | Description | Version |
| ------- | ----------- | ------- |
|FileParty.Core|Registration, Factory, and Watcher|[![Nuget Package](https://badgen.net/nuget/v/FileParty.Core)](https://www.nuget.org/packages/FileParty.Core/)|
|FileParty.Providers.FileSystem|Storage Providers for FileSystem|[![Nuget Package](https://badgen.net/nuget/v/FileParty.Providers.FileSystem)](https://www.nuget.org/packages/FileParty.Providers.FileSystem/)|
|FileParty.Providers.AWS.S3|Storage Providers for AWS S3 Buckets|[![Nuget Package](https://badgen.net/nuget/v/FileParty.Providers.AWS.S3)](https://www.nuget.org/packages/FileParty.Providers.AWS.S3/)|

Providing a common set of methods for interacting with files across storage providers 

## How to Use
FileParty uses `Microsoft.Extensions.DependencyInjection` to register itself.  If your project does not, 
do not worry, everything can be self-contained.

Register File Party by calling `this.AddFileParty()` either on an existing `IServiceCollection` or any object. 
If `this` is not an `IServiceCollection` a new Service Collection will be made.

Register each desired module calling a configuration action `x => x.AddModule<ModuleName>(defaultConfigObject)`
which will register that module configuration with the Service Collection. Any quantity of modules may be added this way.

```c#
this.AddFileParty(x => 
    x.AddModule<FileSystemModule>(new FileSystemConfiguration("C:\FilePartyBaseDirectory"))
```

When you need access to an `IAsyncStorageProvider` or `IStorageProvider` simply DI either an `IAsyncFilePartyFactory` 
or an `IFilePartyFactory` and call use one of its methods to get a Storage Provider, Storage Reader, or Storage Writer.

## How to Extend

Create a project that implements:

- BaseAsyncStorageProvider
- BaseStorageProvider
- BaseFilePartyModule

## Questions / Issues / Concerns
Just make an issue on the Repo.  Contributions are welcome.

