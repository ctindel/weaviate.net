namespace Weaviate.Client;

public class DeleteClassRequest
{
    public string Class { get; set; }

    public DeleteClassRequest(string className)
    {
        Class = className;
    }
}
