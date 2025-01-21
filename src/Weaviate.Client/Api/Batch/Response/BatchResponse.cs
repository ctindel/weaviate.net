namespace Weaviate.Client;

public class BatchResponse<T> where T : WeaviateObject
{
    public BatchResults Results { get; set; } = new();
    public string Output { get; set; } = BatchOutput.Minimal;
    public T[]? Objects { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }

    public bool HasErrors => Objects?.Any(o => o is WeaviateObjectResponse resp && 
        (resp.Result?.Status == "FAILED" || (resp.Result?.Errors?.Error?.Length ?? 0) > 0)) ?? false;

    public static BatchResponse<T> FromObjectArray(T[] objects)
    {
        var response = new BatchResponse<T> { Objects = objects };
        if (objects != null)
        {
            response.Failed = objects.Count(o => o is WeaviateObjectResponse resp && resp.Result?.Status == "FAILED");
            response.Successful = objects.Length - response.Failed;
            response.Results = new BatchResults
            {
                Failed = response.Failed,
                Successful = response.Successful
            };
        }
        return response;
    }
}
