using Newtonsoft.Json;

public class UploadDocumentRequest : Request
{

    public string Base64Document { get; }
    public List<Tag> Tags { get; }


    public UploadDocumentRequest(string FileName, string Base64Document, List<Tag> Tags) : base(FileName)
    {
        this.Base64Document = Base64Document;
        this.Tags = Tags;
    }

    public byte[] DecodeObject()
    {
        return Convert.FromBase64String(Base64Document);
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}