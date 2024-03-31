public class AzureFunctionsHandler
{
    public AzureFunctionsHandler() { }

    public URLReply GetDocumentURL(string FileName)
    {
        return new URLReply("/dev/null", "https://www.google.com/");
    }
}