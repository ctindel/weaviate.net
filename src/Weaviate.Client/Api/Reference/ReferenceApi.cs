namespace Weaviate.Client;

public class ReferenceApi
{
    private readonly Transport _transport;
    private readonly DbVersionSupport _dbVersionSupport;

    public ReferenceApi(Transport transport, DbVersionSupport dbVersionSupport)
    {
        _transport = transport;
        _dbVersionSupport = dbVersionSupport;
    }

    public BatchReference Reference(string fromCollection, string toCollection, string referenceProperty, string fromId, string toId)
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

    public static SingleRef CreateReference(string collection, string id)
    {
        return new SingleRef(collection, id);
    }

    public ApiResponse<object> Create(CreateReferenceRequest request)
    {
        if (string.IsNullOrEmpty(request.Collection))
            throw new ArgumentNullException(nameof(request.Collection));
        if (string.IsNullOrEmpty(request.Id))
            throw new ArgumentNullException(nameof(request.Id));
        if (string.IsNullOrEmpty(request.ReferenceProperty))
            throw new ArgumentNullException(nameof(request.ReferenceProperty));
        if (request.ReferencePayload == null)
            throw new ArgumentNullException(nameof(request.ReferencePayload));

        var path = $"/objects/{request.Collection}/{request.Id}/references/{request.ReferenceProperty}";
        var queryParams = request.ConsistencyLevel.HasValue
            ? new Dictionary<string, string> { { "consistency_level", request.ConsistencyLevel.ToString()! } }
            : null;

        return _transport.Send<object>(HttpMethod.Post, path, request.ReferencePayload, queryParams);
    }

    public ApiResponse<object> DeleteReference(DeleteReferenceRequest request)
    {
        if (string.IsNullOrEmpty(request.Collection))
            throw new ArgumentNullException(nameof(request.Collection));
        if (string.IsNullOrEmpty(request.Id))
            throw new ArgumentNullException(nameof(request.Id));
        if (string.IsNullOrEmpty(request.ReferenceProperty))
            throw new ArgumentNullException(nameof(request.ReferenceProperty));
        if (request.ReferencePayload == null)
            throw new ArgumentNullException(nameof(request.ReferencePayload));

        var path = $"/objects/{request.Collection}/{request.Id}/references/{request.ReferenceProperty}";
        var queryParams = request.ConsistencyLevel.HasValue
            ? new Dictionary<string, string> { { "consistency_level", request.ConsistencyLevel.ToString()! } }
            : null;

        return _transport.Send<object>(HttpMethod.Delete, path, request.ReferencePayload, queryParams);
    }

    public ApiResponse<object> AddReference(AddReferenceRequest request)
    {
        if (string.IsNullOrEmpty(request.Collection))
            throw new ArgumentNullException(nameof(request.Collection));
        if (string.IsNullOrEmpty(request.Id))
            throw new ArgumentNullException(nameof(request.Id));
        if (string.IsNullOrEmpty(request.ReferenceProperty))
            throw new ArgumentNullException(nameof(request.ReferenceProperty));
        if (request.ReferencePayload == null)
            throw new ArgumentNullException(nameof(request.ReferencePayload));

        var path = $"/objects/{request.Collection}/{request.Id}/references/{request.ReferenceProperty}";
        var queryParams = request.ConsistencyLevel.HasValue
            ? new Dictionary<string, string> { { "consistency_level", request.ConsistencyLevel.ToString()! } }
            : null;

        return _transport.Send<object>(HttpMethod.Post, path, request.ReferencePayload, queryParams);
    }

    public ApiResponse<object> Replace(ReplaceReferenceRequest request)
    {
        if (string.IsNullOrEmpty(request.Collection))
            throw new ArgumentNullException(nameof(request.Collection));
        if (string.IsNullOrEmpty(request.Id))
            throw new ArgumentNullException(nameof(request.Id));
        if (string.IsNullOrEmpty(request.ReferenceProperty))
            throw new ArgumentNullException(nameof(request.ReferenceProperty));
        if (request.ReferencePayload == null)
            throw new ArgumentNullException(nameof(request.ReferencePayload));

        var path = $"/objects/{request.Collection}/{request.Id}/references/{request.ReferenceProperty}";
        var queryParams = request.ConsistencyLevel.HasValue
            ? new Dictionary<string, string> { { "consistency_level", request.ConsistencyLevel.ToString()! } }
            : null;

        return _transport.Send<object>(HttpMethod.Put, path, request.ReferencePayload, queryParams);
    }
}
