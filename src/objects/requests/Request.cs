public abstract class Request
{

    public string FileName { get; }

    public Request(string FileName)
    {
        this.FileName = FileName;
    }

}