using Newtonsoft.Json;

public class GetDocumentRequest : Request
{

    public GetDocumentRequest(string FileName) : base(FileName) { }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}