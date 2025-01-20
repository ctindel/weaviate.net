using System;

namespace Weaviate.Client;

public class ServerVersionMissingException : Exception
{
    public ServerVersionMissingException(string message) : base(message)
    {
    }
}
