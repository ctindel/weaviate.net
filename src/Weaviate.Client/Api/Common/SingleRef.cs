namespace Weaviate.Client;

public class SingleRef
{
    public SingleRef(string collection, string id)
    {
        if (string.IsNullOrEmpty(collection))
            throw new ArgumentNullException(nameof(collection));
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        Beacon = $"weaviate://localhost/{collection}/{id}";
        Href = $"/v1/objects/{collection}/{id}";
    }

    public string Beacon { get; }
    public string Href { get; }
}
