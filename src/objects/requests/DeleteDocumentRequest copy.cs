using Newtonsoft.Json;

public class DeleteDocumentRequest : Request
{

    public DeleteDocumentRequest(string FileName) : base(FileName) { }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}