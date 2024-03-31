using System.Net;
using System.Text;
using Newtonsoft.Json;

public class WebHandler
{

    private WebApplicationBuilder builder;
    private WebApplication app;
    private AzureStorageHandler storageHandler;
    private AzureFunctionsHandler functionHandler;

    private FileIndexHandler indexHandler;
    public WebHandler(string[] args)
    {
        builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();
        storageHandler = new AzureStorageHandler();
        functionHandler = new AzureFunctionsHandler();
        indexHandler = new FileIndexHandler(new AzureCosmosHandler());
        BuildMappings();
        app.Run();
    }

    private void BuildMappings()
    {

        app.MapGet("/", async context =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await WriteLines(context.Response.Body, "CONNECTED");
        })
        .WithName("TestRoot");

        app.MapPut("/document/upload/", async context =>
        {
            UploadDocumentRequest? request = await GetRequest<UploadDocumentRequest>(context.Request);
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
        })
        .WithName("UploadDocument");

        app.MapGet("/document/download/", async context =>
        {
            GetDocumentRequest? request = await GetRequest<GetDocumentRequest>(context.Request);
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
            URLReply url = functionHandler.GetDocumentURL(requestedFile.FileName);
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await WriteLines(context.Response.Body, url.ToString());
        })
        .WithName("DownloadDocument");

        app.MapGet("/document/get/", async context =>
        {
            GetDocumentRequest? request = await GetRequest<GetDocumentRequest>(context.Request);
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
            await WriteLines(context.Response.Body, requestedFile.ToString());
        })
        .WithName("GetDocumentByID");

        app.MapGet("/document/getall/", async context =>
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await WriteLines(context.Response.Body, JsonConvert.SerializeObject(indexHandler.GetFiles()));

        })
        .WithName("GetAllDocuments");

        app.MapPut("/document/delete/", async context =>
        {
            DeleteDocumentRequest? request = await GetRequest<DeleteDocumentRequest>(context.Request);
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
        })
        .WithName("DeleteDocument");

        app.MapPost("/document/update/tags/", async context =>
        {
            UpdateTagsRequest? request = await GetRequest<UpdateTagsRequest>(context.Request);
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

        })
        .WithName("UpdateTags");

        app.MapPost("/document/update/name/", async context =>
        {
            RenameDocumentRequest? request = await GetRequest<RenameDocumentRequest>(context.Request);
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
        })
        .WithName("UpdateName");
    }

    private async Task<T?> GetRequest<T>(HttpRequest? request) where T : Request
    {
        if (request is null || request.ContentType == null)
        {
            Console.WriteLine("The request or request ContentType is null");
            return default(T);
        }
        if (!request.ContentType.Contains("application/json"))
        {
            Console.WriteLine($"Wrong ContentType. Content Type is {request.ContentType}");
            return default(T);
        }
        string json = await ReadLines(request.Body);
        try
        {
            T? obj = JsonConvert.DeserializeObject<T>(json);
            if (obj is null)
            {
                Console.WriteLine("JSON decoded object returned null.");
                return default(T);
            }
            return obj;
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception decoding JSON string.");
            Console.WriteLine(e.ToString());
            return default(T);
        }
    }

    private async Task WriteLines(Stream stream, string reply)
    {
        await using (var writer = new StreamWriter(stream, Encoding.UTF8))
        {
            await writer.WriteAsync(reply);
        }
    }

    private static async Task<string> ReadLines(Stream stream)
    {
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        {
            return await reader.ReadToEndAsync();
        }
    }
}