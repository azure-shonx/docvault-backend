using Newtonsoft.Json;

public class URLReply : Request
{
    public string URL { get; }

    public URLReply(string FileName, string URL) : base(FileName)
    {
        this.URL = URL;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}