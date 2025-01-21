namespace Weaviate.Client;

public class BatchApi
{
    private readonly Transport _transport;
    private readonly DbVersionSupport _dbVersionSupport;

    public BatchApi(Transport transport, DbVersionSupport dbVersionSupport)
    {
        _transport = transport;
        _dbVersionSupport = dbVersionSupport;
    }

    public ApiResponse<BatchResponse<WeaviateObjectResponse>> CreateObjects(CreateObjectsBatchRequest request)
    {
        // Ensure Class is set for backward compatibility
        foreach (var obj in request.Objects)
        {
            #pragma warning disable CS0618 // Type or member is obsolete
            obj.Class = obj.Collection;
            #pragma warning restore CS0618
        }

        var response = _transport.Post<BatchResponse<WeaviateObjectResponse>, CreateObjectsBatchRequest>("/v1/batch/objects", request);
        
        return response;
    }

    public BatchReference? Reference(string fromCollection, string toCollection, string referenceProperty, string fromId, string toId)
    {
        if (string.IsNullOrEmpty(fromCollection)) throw new ArgumentNullException(nameof(fromCollection));
        if (string.IsNullOrEmpty(toCollection)) throw new ArgumentNullException(nameof(toCollection));
        if (string.IsNullOrEmpty(referenceProperty)) throw new ArgumentNullException(nameof(referenceProperty));
        if (string.IsNullOrEmpty(fromId)) throw new ArgumentNullException(nameof(fromId));
        if (string.IsNullOrEmpty(toId)) throw new ArgumentNullException(nameof(toId));

        return new BatchReference(
            $"weaviate://localhost/{fromCollection}/{fromId}/{referenceProperty}",
            $"weaviate://localhost/{toCollection}/{toId}");
    }

    public ApiResponse<BatchResponse<WeaviateObjectResponse>> CreateReferences(CreateReferencesRequest request)
    {
        return _transport.Post<BatchResponse<WeaviateObjectResponse>, CreateReferencesRequest>("/v1/batch/references", request);
    }

    public ApiResponse<BatchResponse<WeaviateObjectResponse>> DeleteObjects(DeleteObjectsBatchRequest request)
    {
        var path = $"/v1/batch/{request.Collection}";
        var queryParams = new Dictionary<string, string>();

        if (request.DryRun.HasValue)
            queryParams["dryRun"] = request.DryRun.Value.ToString().ToLowerInvariant();
        if (request.Output != null)
            queryParams["output"] = request.Output;
        if (request.ConsistencyLevel.HasValue)
            queryParams["consistency_level"] = request.ConsistencyLevel.Value.ToString();

        return _transport.Send<BatchResponse<WeaviateObjectResponse>>(HttpMethod.Delete, path, request.Where, queryParams);
    }
}
