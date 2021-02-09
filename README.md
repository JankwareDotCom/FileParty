# FileParty
![dotnet_test](https://github.com/JankwareDotCom/FileParty/workflows/dotnet_test/badge.svg)

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

