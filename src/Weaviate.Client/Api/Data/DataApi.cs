using System.Threading;
using System.Threading.Tasks;

namespace Weaviate.Client;

public class DataApi
{
    private readonly Transport _transport;
    private readonly DbVersionSupport _dbVersionSupport;

    public DataApi(Transport transport, DbVersionSupport dbVersionSupport)
    {
        _transport = transport;
        _dbVersionSupport = dbVersionSupport;
    }

    public ApiResponse<WeaviateObject> Create(CreateObjectRequest request)
        => _transport.PostAsync<WeaviateObject, CreateObjectRequest>("/objects", request).GetAwaiter().GetResult();

    public async Task<ApiResponse<WeaviateObject>> CreateAsync(CreateObjectRequest request, CancellationToken cancellationToken = default)
        => await _transport.PostAsync<WeaviateObject, CreateObjectRequest>("/objects", request, cancellationToken).ConfigureAwait(false);

    public ApiResponse<WeaviateObject> Update(UpdateObjectRequest request)
    {
        var endpoint = $"/objects/{request.Collection}/{request.Id}";
        if (request.WithMerge.GetValueOrDefault())
        {
            endpoint += "?consistency_level=ALL";
        }
        return _transport.PutAsync<WeaviateObject, UpdateObjectRequest>(endpoint, request).GetAwaiter().GetResult();
    }

    public async Task<ApiResponse<WeaviateObject>> UpdateAsync(UpdateObjectRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/objects/{request.Collection}/{request.Id}";
        if (request.WithMerge.GetValueOrDefault())
        {
            endpoint += "?consistency_level=ALL";
        }
        return await _transport.PutAsync<WeaviateObject, UpdateObjectRequest>(endpoint, request, cancellationToken).ConfigureAwait(false);
    }

    public ApiResponse<WeaviateObject[]> Get(GetObjectRequest request)
    {
        var endpoint = "/objects";
        if (!string.IsNullOrEmpty(request.Collection))
        {
            endpoint += $"/{request.Collection}";
            if (!string.IsNullOrEmpty(request.Id))
            {
                endpoint += $"/{request.Id}";
            }
        }

        var queryParams = new List<string>();
        if (request.Additional?.Any() == true)
        {
            queryParams.Add($"include={string.Join(",", request.Additional)}");
        }
        if (request.Limit.HasValue)
        {
            queryParams.Add($"limit={request.Limit}");
        }
        if (request.Offset.HasValue)
        {
            queryParams.Add($"offset={request.Offset}");
        }
        if (!string.IsNullOrEmpty(request.After))
        {
            queryParams.Add($"after={request.After}");
        }
        if (request.ConsistencyLevel.HasValue)
        {
            queryParams.Add($"consistency_level={request.ConsistencyLevel.Value}");
        }
        if (!string.IsNullOrEmpty(request.NodeName))
        {
            queryParams.Add($"node_name={request.NodeName}");
        }

        if (queryParams.Any())
        {
            endpoint += $"?{string.Join("&", queryParams)}";
        }

        return _transport.GetAsync<WeaviateObject[]>(endpoint).GetAwaiter().GetResult();
    }

    public async Task<ApiResponse<WeaviateObject[]>> GetAsync(GetObjectRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = "/objects";
        if (!string.IsNullOrEmpty(request.Collection))
        {
            endpoint += $"/{request.Collection}";
            if (!string.IsNullOrEmpty(request.Id))
            {
                endpoint += $"/{request.Id}";
            }
        }

        var queryParams = new List<string>();
        if (request.Additional?.Any() == true)
        {
            queryParams.Add($"include={string.Join(",", request.Additional)}");
        }
        if (request.Limit.HasValue)
        {
            queryParams.Add($"limit={request.Limit}");
        }
        if (request.Offset.HasValue)
        {
            queryParams.Add($"offset={request.Offset}");
        }
        if (!string.IsNullOrEmpty(request.After))
        {
            queryParams.Add($"after={request.After}");
        }
        if (request.ConsistencyLevel.HasValue)
        {
            queryParams.Add($"consistency_level={request.ConsistencyLevel.Value}");
        }
        if (!string.IsNullOrEmpty(request.NodeName))
        {
            queryParams.Add($"node_name={request.NodeName}");
        }

        if (queryParams.Any())
        {
            endpoint += $"?{string.Join("&", queryParams)}";
        }

        return await _transport.GetAsync<WeaviateObject[]>(endpoint, cancellationToken).ConfigureAwait(false);
    }

    public ApiResponse<WeaviateObject[]> GetAll()
        => Get(new GetObjectRequest());

    public Task<ApiResponse<WeaviateObject[]>> GetAllAsync(CancellationToken cancellationToken = default)
        => GetAsync(new GetObjectRequest(), cancellationToken);

    public ApiResponse<EmptyResponse> DeleteObject(DeleteObjectRequest request)
    {
        var endpoint = $"/objects/{request.Collection}/{request.Id}";
        var queryParams = new List<string>();

        if (request.ConsistencyLevel.HasValue)
            queryParams.Add($"consistency_level={request.ConsistencyLevel.Value}");

        if (queryParams.Any())
            endpoint += $"?{string.Join("&", queryParams)}";

        return _transport.DeleteAsync<EmptyResponse>(endpoint).GetAwaiter().GetResult();
    }

    public async Task<ApiResponse<EmptyResponse>> DeleteObjectAsync(DeleteObjectRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/objects/{request.Collection}/{request.Id}";
        var queryParams = new List<string>();

        if (request.ConsistencyLevel.HasValue)
            queryParams.Add($"consistency_level={request.ConsistencyLevel.Value}");

        if (queryParams.Any())
            endpoint += $"?{string.Join("&", queryParams)}";

        return await _transport.DeleteAsync<EmptyResponse>(endpoint, cancellationToken).ConfigureAwait(false);
    }

    public ApiResponse<WeaviateObject> Validate(ValidateObjectRequest request)
    {
        var endpoint = $"/objects/{request.Collection}/{request.Id}/validate";
        return _transport.PostAsync<WeaviateObject, ValidateObjectRequest>(endpoint, request).GetAwaiter().GetResult();
    }

    public async Task<ApiResponse<WeaviateObject>> ValidateAsync(ValidateObjectRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/objects/{request.Collection}/{request.Id}/validate";
        return await _transport.PostAsync<WeaviateObject, ValidateObjectRequest>(endpoint, request, cancellationToken).ConfigureAwait(false);
    }

    public ApiResponse<WeaviateObject> Check(ValidateObjectRequest request)
    {
        return Validate(request);
    }

    public Task<ApiResponse<WeaviateObject>> CheckAsync(ValidateObjectRequest request, CancellationToken cancellationToken = default)
    {
        return ValidateAsync(request, cancellationToken);
    }
}
