// Copyright (C) 2023 Weaviate - https://weaviate.io
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// ReSharper disable once CheckNamespace
namespace Weaviate.Client;

/// <summary>
/// Collection configuration operations for managing Weaviate collections and their properties.
/// </summary>
public class SchemaApi
{
    private readonly Transport _transport;

    internal SchemaApi(Transport transport) => _transport = transport;

    /// <summary>
    /// Gets all collection configurations.
    /// </summary>
    /// <returns>The complete schema containing all collection configurations.</returns>
    public ApiResponse<WeaviateSchema> GetSchema()
	    => _transport.GetAsync<WeaviateSchema>("/v1/schema").GetAwaiter().GetResult();

    /// <summary>
    /// Gets all collection configurations asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The complete schema containing all collection configurations.</returns>
    public async Task<ApiResponse<WeaviateSchema>> GetSchemaAsync(CancellationToken cancellationToken = default)
	    => await _transport.GetAsync<WeaviateSchema>("/v1/schema", cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets a collection configuration by name.
    /// </summary>
    /// <param name="request">The request containing the collection name.</param>
    /// <returns>The collection configuration.</returns>
    public ApiResponse<WeaviateCollection> GetCollection(GetCollectionRequest request)
	    => _transport.GetAsync<WeaviateCollection>($"/v1/schema/{request.Name}").GetAwaiter().GetResult();

    [Obsolete("Use GetCollection instead. This method will be removed in v5.")]
    public ApiResponse<WeaviateCollection> GetClass(GetClassRequest request)
        => GetCollection(new GetCollectionRequest(request.Class));

    /// <summary>
    /// Gets a collection configuration by name asynchronously.
    /// </summary>
    /// <param name="request">The request containing the collection name.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The collection configuration.</returns>
    public async Task<ApiResponse<WeaviateCollection>> GetCollectionAsync(GetCollectionRequest request, CancellationToken cancellationToken = default)
	    => await _transport.GetAsync<WeaviateCollection>($"/v1/schema/{request.Name}", cancellationToken).ConfigureAwait(false);

    [Obsolete("Use GetCollectionAsync instead. This method will be removed in v5.")]
    public async Task<ApiResponse<WeaviateCollection>> GetClassAsync(GetClassRequest request, CancellationToken cancellationToken = default)
        => await GetCollectionAsync(new GetCollectionRequest(request.Class), cancellationToken);

    public ApiResponse<Shard[]> GetShards(GetShardsRequest request)
	    => _transport.GetAsync<Shard[]>($"/v1/schema/{request.CollectionName}/shards").GetAwaiter().GetResult();

    public async Task<ApiResponse<Shard[]>> GetShardsAsync(GetShardsRequest request, CancellationToken cancellationToken = default)
	    => await _transport.GetAsync<Shard[]>($"/v1/schema/{request.CollectionName}/shards", cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Creates a new collection with the specified configuration.
    /// </summary>
    /// <param name="request">The request containing the collection configuration.</param>
    /// <returns>The created collection configuration.</returns>
    public ApiResponse<WeaviateCollection> CreateCollection(CreateCollectionRequest request)
	    => _transport.PostAsync<WeaviateCollection, WeaviateCollection>("/v1/schema", request).GetAwaiter().GetResult();

    [Obsolete("Use CreateCollection instead. This method will be removed in v5.")]
    public ApiResponse<WeaviateCollection> CreateSchemaClass(CreateClassRequest request)
        => CreateCollection(new CreateCollectionRequest(request.Class));

    /// <summary>
    /// Creates a new collection with the specified configuration asynchronously.
    /// </summary>
    /// <param name="request">The request containing the collection configuration.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The created collection configuration.</returns>
    public async Task<ApiResponse<WeaviateCollection>> CreateCollectionAsync(CreateCollectionRequest request, CancellationToken cancellationToken = default)
	    => await _transport.PostAsync<WeaviateCollection, WeaviateCollection>("/v1/schema", request, cancellationToken).ConfigureAwait(false);

    [Obsolete("Use CreateCollectionAsync instead. This method will be removed in v5.")]
    public async Task<ApiResponse<WeaviateCollection>> CreateSchemaClassAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
        => await CreateCollectionAsync(new CreateCollectionRequest(request.Class), cancellationToken);

    /// <summary>
    /// Updates a shard's status in a collection.
    /// </summary>
    /// <param name="request">The request containing the collection name, shard name, and new status.</param>
    /// <returns>The updated shard status.</returns>
    public ApiResponse<ShardStatus> UpdateShard(UpdateShardRequest request)
    {
	    var response = _transport.PutAsync<StatusContainer, StatusContainer>(
            $"/schema/{request.CollectionName}/shards/{request.Shard}", new()
            {
                Status = request.Status
            }).GetAwaiter().GetResult();
	    return GetResponse(response);
    }

    /// <summary>
    /// Updates a shard's status in a collection asynchronously.
    /// </summary>
    /// <param name="request">The request containing the collection name, shard name, and new status.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The updated shard status.</returns>
    public async Task<ApiResponse<ShardStatus>> UpdateShardAsync(UpdateShardRequest request, CancellationToken cancellationToken = default)
    {
	    var response = await _transport.PutAsync<StatusContainer, StatusContainer>(
			    $"/schema/{request.CollectionName}/shards/{request.Shard}", new() { Status = request.Status }, cancellationToken).ConfigureAwait(false);
	    return GetResponse(response);
    }

    /// <summary>
    /// Deletes a collection by name.
    /// </summary>
    /// <param name="request">The request containing the collection name.</param>
    /// <returns>The API response indicating success or failure.</returns>
    public ApiResponse<object> DeleteCollection(DeleteCollectionRequest request)
	    => _transport.DeleteAsync<object>($"/v1/schema/{request.Name}").GetAwaiter().GetResult();

    [Obsolete("Use DeleteCollection instead. This method will be removed in v5.")]
    public ApiResponse<object> DeleteClass(DeleteClassRequest request)
        => DeleteCollection(new DeleteCollectionRequest(request.Class));

    /// <summary>
    /// Deletes a collection by name asynchronously.
    /// </summary>
    /// <param name="request">The request containing the collection name.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The API response indicating success or failure.</returns>
    public async Task<ApiResponse<object>> DeleteCollectionAsync(DeleteCollectionRequest request, CancellationToken cancellationToken = default)
	    => await _transport.DeleteAsync<object>($"/v1/schema/{request.Name}", cancellationToken).ConfigureAwait(false);

    [Obsolete("Use DeleteCollectionAsync instead. This method will be removed in v5.")]
    public async Task<ApiResponse<object>> DeleteClassAsync(DeleteClassRequest request, CancellationToken cancellationToken = default)
        => await DeleteCollectionAsync(new DeleteCollectionRequest(request.Class), cancellationToken);

    /// <summary>
    /// Creates a new property in a collection.
    /// </summary>
    /// <param name="request">The request containing the collection name and property configuration.</param>
    /// <returns>The created property configuration.</returns>
    public ApiResponse<Property> CreateProperty(CreatePropertyRequest request)
	    => _transport.PostAsync<Property, Property>($"/v1/schema/{request.CollectionName}/properties", request.Property ?? throw new ArgumentNullException(nameof(request.Property))).GetAwaiter().GetResult();

    /// <summary>
    /// Creates a new property in a collection asynchronously.
    /// </summary>
    /// <param name="request">The request containing the collection name and property configuration.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The created property configuration.</returns>
    public async Task<ApiResponse<Property>> CreatePropertyAsync(CreatePropertyRequest request, CancellationToken cancellationToken = default)
	    => await _transport.PostAsync<Property, Property>($"/v1/schema/{request.CollectionName}/properties", request.Property ?? throw new ArgumentNullException(nameof(request.Property)), cancellationToken).ConfigureAwait(false);

    public void UpdateShards(UpdateShardsRequest request)
    {
	    var shards = GetShards(new(request.CollectionName));
	    foreach (var shard in shards.Result!)
		    if (shard.Name != null)
			    UpdateShard(new(request.CollectionName, shard.Name, request.Status));
    }

    public async Task UpdateShardsAsync(UpdateShardsRequest request, CancellationToken cancellationToken = default)
    {
	    var shards = await GetShardsAsync(new(request.CollectionName), cancellationToken).ConfigureAwait(false);
	    foreach (var shard in shards.Result!)
		    if (shard.Name != null)
			    await UpdateShardAsync(new(request.CollectionName, shard.Name, request.Status), cancellationToken).ConfigureAwait(false);
    }

    public void DeleteAllCollections()
    {
        var schema = GetSchema();
        if (schema?.Result?.Collections == null) return;
        
        foreach (var collection in schema.Result.Collections)
        {
            if (collection?.Name != null)
                DeleteCollection(new(collection.Name));
        }
    }

    [Obsolete("Use DeleteAllCollections instead. This method will be removed in v5.")]
    public void DeleteAllClasses()
        => DeleteAllCollections();

    public async Task DeleteAllCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var schema = await GetSchemaAsync(cancellationToken).ConfigureAwait(false);
        if (schema?.Result?.Collections == null) return;
        
        foreach (var collection in schema.Result.Collections)
        {
            if (collection?.Name != null)
                await DeleteCollectionAsync(new(collection.Name), cancellationToken).ConfigureAwait(false);
        }
    }

    [Obsolete("Use DeleteAllCollectionsAsync instead. This method will be removed in v5.")]
    public async Task DeleteAllClassesAsync(CancellationToken cancellationToken = default)
        => await DeleteAllCollectionsAsync(cancellationToken);

    private static ApiResponse<ShardStatus> GetResponse(ApiResponse<StatusContainer> response) =>
	    new()
	    {
		    Error = response.Error,
		    Uri = response.Uri,
		    HttpStatusCode = response.HttpStatusCode,
		    HttpMethod = response.HttpMethod,
		    RequestBody = response.RequestBody,
		    ResponseBody = response.ResponseBody,
		    Result = response.Result!.Status
	    };

    private class StatusContainer
    {
        public ShardStatus Status { get; set; }
    }
}
