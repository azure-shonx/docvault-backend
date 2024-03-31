using Newtonsoft.Json;

public class UpdateTagsRequest : Request
{

    public List<Tag> Tags { get; }

    public UpdateTagsRequest(string FileName, List<Tag> Tags) : base(FileName)
    {
        this.Tags = Tags;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}