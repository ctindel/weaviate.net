[<img align="right" width="250" height="auto" src="https://weaviate.io/img/site/weaviate-logo.svg">](https://weaviate.io/)

Learn more about Weaviate: [https://weaviate.io/](https://weaviate.io/)

# Weaviate .NET client

A Microsoft .NET native client for [Weaviate](https://weaviate.io/)

## Build status

![Tests status](https://github.com/weaviate/weaviate-dotnet-client/actions/workflows/tests.yml/badge.svg)

## Packages

The .NET assembly is published to NuGet under the package name [Weaviate.Client](http://nuget.org/packages/Weaviate.Client)

## Getting started

### Installing

You can install `Weaviate.Client` from the package manager console:

    PM> Install-Package Weaviate.Client

Alternatively, simply search for `Weaviate.Client` in the package manager UI.

### Usage

```csharp
using Weaviate.Client;
using Flurl.Http;

public class Main
{
    public void Main()
    {
        var flurlClient = new FlurlClient();
        var weaviateClient = new WeaviateClient(new Config("http", "localhost:8080"), flurlClient);
        var meta = weaviateClient.Misc.Meta();
        Console.WriteLine(meta.Error != null ? meta.Error.Message : meta.Result.Version);
    }
}
```

For a better understanding of usage, please refer to the [API Integration Tests](https://github.com/weaviate/weaviate-dotnet-client/tree/main/tests-integration/Weaviate.Client.IntegrationTests/Api)

## Versioning

The version of the `Weaviate.Client` should match the published Weaviate server version.

For example, if you are using Weaviate server version `1.18` you should use version `1.18` of this package.

Patch versions are compatible within the minor version, for example;

`Weaviate.Client` version `1.18.4` can communicate with Weaviate server version `1.18`.

A verification check takes place on creation of the client to ensure that the client and server versions match.
