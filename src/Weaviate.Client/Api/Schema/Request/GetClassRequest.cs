namespace Weaviate.Client;

public class GetClassRequest
{
    public string ClassName { get; set; }
    
    [Obsolete("Use ClassName instead. This property will be removed in v5.")]
    public string Class { get => ClassName; set => ClassName = value; }

    public GetClassRequest(string className)
    {
        ClassName = className;
    }
}
