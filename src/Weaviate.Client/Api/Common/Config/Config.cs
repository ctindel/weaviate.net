namespace Weaviate.Client;

public class Config
{
    public string Host { get; set; } = string.Empty;
    public string Scheme { get; set; } = string.Empty;
    public int? Port { get; set; }
    public bool? GrpcSecure { get; set; }
    public string? ApiKey { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? AccessToken { get; set; }
    public string? AdditionalHeaders { get; set; }

    public Config(string scheme, string host)
    {
        Scheme = scheme;
        Host = host;
    }
}
