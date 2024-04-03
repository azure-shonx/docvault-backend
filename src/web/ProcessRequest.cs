using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Newtonsoft.Json;

public class ProcessRequest
{

    private readonly AzureStorageHandler storageHandler;
    private readonly AzureFunctionsHandler functionHandler;
    private readonly FileIndexHandler indexHandler;

    public ProcessRequest(AzureStorageHandler storageHandler, AzureFunctionsHandler functionHandler, FileIndexHandler indexHandler)
    {
        this.storageHandler = storageHandler;
        this.functionHandler = functionHandler;
        this.indexHandler = indexHandler;
    }

    public async Task TestRoot(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        await WebUtil.WriteLines(context.Response.Body, "CONNECTED");
    }

    public async Task UploadDocument(HttpContext context)
    {
        UploadDocumentRequest? request = await WebUtil.GetRequest<UploadDocumentRequest>(context.Request);
        if (request is null)
        {
            Console.WriteLine("UploadDocumentRequest is null.");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        FileReference? requestedFile = indexHandler.GetFile(request.FileName);
        if (requestedFile != null) // File already exists
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            return;
        }
        FileReference file = new FileReference(request.FileName, request.Tags);
        storageHandler.SaveDocument(request.FileName, request.DecodeObject());
        indexHandler.AddFile(file);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
    }

    public async Task DownloadDocument(HttpContext context)
    {
        GetDocumentRequest? request = await WebUtil.GetRequest<GetDocumentRequest>(context.Request);
        if (request is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        URLReply? url = await functionHandler.GetDocumentURL(request.FileName);
        if (url is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        await WebUtil.WriteLines(context.Response.Body, url.ToString());
    }

    public async Task GetDocumentByName(HttpContext context)
    {
        GetDocumentRequest? request = await WebUtil.GetRequest<GetDocumentRequest>(context.Request);
        if (request is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        FileReference? requestedFile = indexHandler.GetFile(request.FileName);
        if (requestedFile is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        await WebUtil.WriteLines(context.Response.Body, requestedFile.ToString());
    }

    public async Task GetAllDocuments(HttpContext context)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        await WebUtil.WriteLines(context.Response.Body, JsonConvert.SerializeObject(indexHandler.GetFiles()));
    }
    public async Task DeleteDocument(HttpContext context)
    {
        DeleteDocumentRequest? request = await WebUtil.GetRequest<DeleteDocumentRequest>(context.Request);
        if (request is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        FileReference? requestedFile = indexHandler.GetFile(request.FileName);
        if (requestedFile is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        storageHandler.DeleteDocument(requestedFile.FileName);
        indexHandler.RemoveFile(requestedFile);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
    }
    public async Task UpdateTags(HttpContext context)
    {
        UpdateTagsRequest? request = await WebUtil.GetRequest<UpdateTagsRequest>(context.Request);
        if (request is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        FileReference? requestedFile = indexHandler.GetFile(request.FileName);
        if (requestedFile is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        requestedFile.SetTags(request.Tags);
        indexHandler.UpdateFile(requestedFile);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
    }

    public async Task UpdateName(HttpContext context)
    {
        RenameDocumentRequest? request = await WebUtil.GetRequest<RenameDocumentRequest>(context.Request);
        if (request is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        FileReference? requestedFile = indexHandler.GetFile(request.FileName);
        if (requestedFile is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        FileReference fUpdate = new FileReference(request.NewName, requestedFile.Tags);
        storageHandler.RenameDocument(request.FileName, request.NewName);
        indexHandler.RenameFile(request.FileName, fUpdate);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
    }
}