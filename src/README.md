# Weaviate .NET client

Learn more at: [https://weaviate.io/](https://weaviate.io/)

A Microsoft .NET native client for [Weaviate](https://weaviate.io/)

## Getting started

### Installing

You can install Weaviate.Client from the package manager console:

    PM> Install-Package Weaviate.Client

Alternatively, simply search for `Weaviate.Client` in the package manager UI.

### Simple Usage

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

## Versioning

The version of the `Weaviate.Client` should match the published Weaviate server version.

For example, if you are using Weaviate server version `1.18` you should use version `1.18` of this package.

Patch versions are compatible within the minor version, for example;

`Weaviate.Client` version `1.18.4` can communicate with Weaviate server version `1.18`.

A verification check takes place on creation of the client to ensure that the client and server versions match.
