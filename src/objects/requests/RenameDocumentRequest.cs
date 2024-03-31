using Newtonsoft.Json;

public class RenameDocumentRequest : Request
{
    public string NewName { get; }

    public RenameDocumentRequest(string OldName, string NewName) : base(OldName)
    {
        this.NewName = NewName;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}