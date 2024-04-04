using Newtonsoft.Json;

public class RenameDocumentRequest : Request
{
    public string NewName { get; }

    public RenameDocumentRequest(string FileName, string NewName) : base(FileName)
    {
        this.NewName = NewName;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}