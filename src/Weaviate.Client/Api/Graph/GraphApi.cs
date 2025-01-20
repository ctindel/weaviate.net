using System.Text.Json.Nodes;

namespace Weaviate.Client;

public class GraphApi
{
    private readonly Transport _transport;
    private readonly DbVersionSupport _dbVersionSupport;

    public GraphApi(Transport transport, DbVersionSupport dbVersionSupport)
    {
        _transport = transport;
        _dbVersionSupport = dbVersionSupport;
    }

    public ApiResponse<GraphResponse> Get(GetRequest request)
    {
        return _transport.Post<GraphResponse, GetRequest>("/graphql", request);
    }

    public ApiResponse<GraphResponse> Aggregate(AggregateRequest request)
    {
        return _transport.Post<GraphResponse, AggregateRequest>("/graphql", request);
    }

    public ApiResponse<GraphResponse> Explore(ExploreRequest request)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(request.Collection))
            queryParams.Add($"collection={request.Collection}");
        if (!string.IsNullOrEmpty(request.Query))
            queryParams.Add($"q={request.Query}");
        if (request.Limit.HasValue)
            queryParams.Add($"limit={request.Limit}");
        if (request.Certainty.HasValue)
            queryParams.Add($"certainty={request.Certainty}");
        if (request.Distance.HasValue)
            queryParams.Add($"distance={request.Distance}");

        var endpoint = "/objects/explore";
        if (queryParams.Any())
            endpoint += $"?{string.Join("&", queryParams)}";

        var response = _transport.GetAsync<ExploreResponse[]>(endpoint).GetAwaiter().GetResult();
        var result = response.Result ?? Array.Empty<ExploreResponse>();
        
        var exploreObj = new JsonObject();
        exploreObj["Explore"] = JsonValue.Create(result);

        var getObj = new JsonObject();
        getObj["Get"] = exploreObj;

        return new ApiResponse<GraphResponse>
        {
            HttpStatusCode = response.HttpStatusCode,
            Error = response.Error,
            Result = new GraphResponse { Data = getObj }
        };
    }
}
