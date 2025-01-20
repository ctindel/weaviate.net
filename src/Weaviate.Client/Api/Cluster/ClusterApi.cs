namespace Weaviate.Client;

public class ClusterApi
{
    private readonly Transport _transport;

    public ClusterApi(Transport transport)
    {
        _transport = transport;
    }

    public ApiResponse<NodeStatusResponse> NodeStatus()
    {
        var metaResponse = _transport.GetAsync<MetaResponse>("/v1/meta").GetAwaiter().GetResult();
        return new ApiResponse<NodeStatusResponse>
        {
            HttpStatusCode = metaResponse.HttpStatusCode,
            Error = metaResponse.Error,
            Result = new NodeStatusResponse
            {
                Status = "HEALTHY",  // Meta endpoint returning success means node is healthy
                Version = metaResponse.Result?.Version,
                GitHash = metaResponse.Result?.GitHash,
                Shards = null  // Meta endpoint doesn't return shards info
            }
        };
    }

    public async Task<ApiResponse<NodeStatusResponse>> NodeStatusAsync(CancellationToken cancellationToken = default)
    {
        var metaResponse = await _transport.GetAsync<MetaResponse>("/v1/meta", cancellationToken).ConfigureAwait(false);
        return new ApiResponse<NodeStatusResponse>
        {
            HttpStatusCode = metaResponse.HttpStatusCode,
            Error = metaResponse.Error,
            Result = new NodeStatusResponse
            {
                Status = "HEALTHY",  // Meta endpoint returning success means node is healthy
                Version = metaResponse.Result?.Version,
                GitHash = metaResponse.Result?.GitHash,
                Shards = null  // Meta endpoint doesn't return shards info
            }
        };
    }
}
