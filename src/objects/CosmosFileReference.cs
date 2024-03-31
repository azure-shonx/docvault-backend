using Newtonsoft.Json;

public class CosmosFileReference
{

    public string id { get; } // It's called 'id', but it's really 'FileName'
    public List<Tag> Tags { get; private set; }

    [JsonConstructor]
    public CosmosFileReference(string id, List<Tag> Tags)
    {
        this.id = id;
        this.Tags = Tags;
    }

    public CosmosFileReference(FileReference fr) : this(fr.FileName, fr.Tags) { }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}