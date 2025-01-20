namespace Weaviate.Client;

public class Group
{
    public GroupType Type { get; set; }
    public float Force { get; set; }

    public override string ToString()
    {
        return $"{{type:\"{Type}\",force:{Force}}}";
    }
}
