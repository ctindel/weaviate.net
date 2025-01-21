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
        _transport.DebugLoggingEnabled = true;  // Enable debug logging temporarily

        // Debug log each object in the batch
        Console.WriteLine("[DEBUG] Batch request objects:");
        foreach (var obj in request.Objects)
        {
            // Always ensure moduleConfig is properly set up
            if (obj.ModuleConfig == null)
            {
                obj.ModuleConfig = new Dictionary<string, Dictionary<string, object>>
                {
                    ["text2vec-ollama"] = new Dictionary<string, object>
                    {
                        { "model", "mxbai-embed-large" },
                        { "apiEndpoint", "http://host.docker.internal:11434" },
                        { "skip", false },
                        { "vectorizePropertyName", true },
                        { "vectorizeClassName", false }
                    }
                };
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
            };

            Console.WriteLine($"[DEBUG] Object ID: {obj.Id}");
            Console.WriteLine($"[DEBUG] Collection: {obj.Collection}");
            Console.WriteLine($"[DEBUG] ModuleConfig: {System.Text.Json.JsonSerializer.Serialize(obj.ModuleConfig, options)}");
            Console.WriteLine($"[DEBUG] Properties: {System.Text.Json.JsonSerializer.Serialize(obj.Properties, options)}");
            Console.WriteLine($"[DEBUG] Full object: {System.Text.Json.JsonSerializer.Serialize(obj, options)}");
            Console.WriteLine("---");
        }
        Console.Out.Flush();

        var response = _transport.Post<BatchResponse<WeaviateObjectResponse>, CreateObjectsBatchRequest>("/v1/batch/objects", request);

        // Log the response details
        if (response.Result != null)
        {
            Console.WriteLine($"[DEBUG] Batch creation response - Successful: {response.Result.Successful}");
            if (response.Result.Objects != null)
            {
                var failedObjects = response.Result.Objects.Where(o => o.Errors != null && o.Errors.Any()).ToList();
                if (failedObjects.Any())
                {
                    Console.WriteLine($"[DEBUG] Failed objects count: {failedObjects.Count}");
                    foreach (var failure in failedObjects)
                    {
                        Console.WriteLine($"[DEBUG] Failed object - ID: {failure.Id}, Error: {string.Join(", ", failure.Errors ?? Array.Empty<string>())}");
                    }
                }
            }
        }
        if (response.Error != null && response.Error.Error != null)
        {
            foreach (var error in response.Error.Error)
            {
                Console.WriteLine($"[DEBUG] Error: {error.Message}");
            }
        }
        Console.Out.Flush();

        _transport.DebugLoggingEnabled = false;  // Disable debug logging

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
            $"weaviate://host.docker.internal/{fromCollection}/{fromId}/{referenceProperty}",
            $"weaviate://host.docker.internal/{toCollection}/{toId}");
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
