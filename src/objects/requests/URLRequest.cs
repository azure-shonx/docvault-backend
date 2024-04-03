using Newtonsoft.Json;

public class URLRequest : Request
{

    public URLRequest(string FileName) : base(FileName) { }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}