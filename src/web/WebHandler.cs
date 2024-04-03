using System.Net;
using System.Text;
using Newtonsoft.Json;

public class WebHandler
{

    private WebApplicationBuilder builder;
    private WebApplication app;

    private ProcessRequest handle;
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

        handle = new ProcessRequest(new AzureStorageHandler(), new AzureFunctionsHandler(), new FileIndexHandler(new AzureCosmosHandler()));

        BuildMappings();
        app.Run();
    }

    private void BuildMappings()
    {
        app.MapGet("/", async context =>
        {
            await handle.TestRoot(context);
        })
        .WithName("TestRoot");

        app.MapPut("/document/upload/", async context =>
        {
            await handle.UploadDocument(context);
        })
        .WithName("UploadDocument");

        app.MapGet("/document/download/", async context =>
        {
            await handle.DownloadDocument(context);
        })
        .WithName("DownloadDocument");

        app.MapGet("/document/get/", async context =>
        {
            await handle.GetDocumentByName(context);
        })
        .WithName("GetDocumentByName");

        app.MapGet("/document/getall/", async context =>
        {
            await handle.GetAllDocuments(context);
        })
        .WithName("GetAllDocuments");

        app.MapPut("/document/delete/", async context =>
        {
            await handle.DeleteDocument(context);
        })
        .WithName("DeleteDocument");

        app.MapPost("/document/update/tags/", async context =>
        {
            await handle.UpdateTags(context);
        })
        .WithName("UpdateTags");

        app.MapPost("/document/update/name/", async context =>
        {
            await handle.UpdateName(context);
        })
        .WithName("UpdateName");
    }
}