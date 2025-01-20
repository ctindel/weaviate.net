using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Weaviate.Client;

public class MiscApi
{
    private readonly Transport _transport;

    public MiscApi(Transport transport)
    {
        _transport = transport;
    }

    public ApiResponse<MetaResponse> Meta()
    {
        try
        {
            return _transport.GetAsync<MetaResponse>("/v1/meta").GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return new ApiResponse<MetaResponse>
            {
                Error = new ErrorResponse 
                { 
                    Error = new[] { new Error { Message = ex.Message } }
                }
            };
        }
    }

    public async Task<ApiResponse<MetaResponse>> MetaAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _transport.GetAsync<MetaResponse>("/v1/meta", cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new ApiResponse<MetaResponse>
            {
                Error = new ErrorResponse 
                { 
                    Error = new[] { new Error { Message = ex.Message } }
                }
            };
        }
    }
}

public class MetaResponse
{
    [JsonPropertyName("grpcMaxMessageSize")]
    public long GrpcMaxMessageSize { get; set; }
    
    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }
    
    [JsonPropertyName("modules")]
    public Dictionary<string, ModuleInfo>? Modules { get; set; }
    
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("gitHash")]
    public string? GitHash { get; set; }
}

public class ModuleInfo
{
    [JsonPropertyName("documentationHref")]
    public string? DocumentationHref { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
