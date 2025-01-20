namespace Weaviate.Client;

public class BatchOutput : WeaviateObject
{
    public const string Verbose = "verbose";
    public const string Minimal = "minimal";

    public object? Result { get; set; }
    public object[]? Errors { get; set; }
    private string _status = BatchResultStatus.Success.ToWeaviateString();
    public string Status 
    { 
        get => _status;
        set => _status = value ?? BatchResultStatus.Success.ToWeaviateString();
    }
}
