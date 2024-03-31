using Newtonsoft.Json;

public class FileReference
{

    public string FileName { get; }
    public List<Tag> Tags { get; private set; }
    public FileReference(string FileName, List<Tag>? tags)
    {
        this.FileName = FileName;
        if (tags is null)
            this.Tags = new List<Tag>(0);
        else
            this.Tags = tags;
    }


    public FileReference(string FileName) : this(FileName, new List<Tag>(0)) { }

    public FileReference(CosmosFileReference fr) : this(fr.id, fr.Tags) { }

    public void SetTags(List<Tag> Tags)
    {
        if (Tags != null)
            this.Tags = Tags;
    }


    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}