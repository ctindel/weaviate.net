using System;

namespace Weaviate.Client;

public class VersionMismatchException : Exception
{
    public VersionMismatchException(string message) : base(message)
    {
    }
}
