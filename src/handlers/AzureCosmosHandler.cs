using Azure.Identity;
using Microsoft.Azure.Cosmos;

public class AzureCosmosHandler
{

    private CosmosClient Client;
    private Database Database;
    private Container Container;

    public AzureCosmosHandler()
    {
        Client = new CosmosClient("https://shonx-document-vault.documents.azure.com/", new DefaultAzureCredential());
        Database = Client.GetDatabase("documents");
        Container = Database.GetContainer("documents");
    }

    public async Task<FileReference?> GetFile(string FileName)
    {
        string sql = "SELECT * FROM documents c WHERE c.id = @id";
        var query = new QueryDefinition(sql).WithParameter("@id", FileName);
        using (FeedIterator<CosmosFileReference> feed = Container.GetItemQueryIterator<CosmosFileReference>(query))
        {
            FeedResponse<CosmosFileReference> response = await feed.ReadNextAsync();
            IEnumerator<CosmosFileReference> enumerator = response.GetEnumerator();
            return new FileReference(enumerator.Current);
        }
    }

    public async Task UpdateFile(FileReference file)
    {
        await DeleteFile(file);
        await SaveFile(file);
    }
    public Task<ItemResponse<CosmosFileReference>> SaveFile(FileReference file)
    {
        CosmosFileReference cfr = new CosmosFileReference(file);
        return Container.CreateItemAsync(cfr, new PartitionKey(cfr.id));
    }

    public Task<ItemResponse<CosmosFileReference>> DeleteFile(FileReference file)
    {
        CosmosFileReference cfr = new CosmosFileReference(file);
        return Container.DeleteItemAsync<CosmosFileReference>(cfr.id, new PartitionKey(cfr.id));
    }

    internal async Task<List<FileReference>?> GetIndex()
    {
        try
        {
            List<FileReference> Files = new List<FileReference>(0);
            string sql = "SELECT * FROM documents c";
            var query = new QueryDefinition(sql);
            using (FeedIterator<CosmosFileReference> feed = Container.GetItemQueryIterator<CosmosFileReference>(query))
            {
                if (!feed.HasMoreResults)
                    return Files; // No Files?
                FeedResponse<CosmosFileReference> response = await feed.ReadNextAsync();
                IEnumerator<CosmosFileReference> enumerator = response.GetEnumerator();
                while (enumerator.MoveNext())
                    Files.Add(new FileReference(enumerator.Current));

            }
            return Files;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            Console.WriteLine("Unable to contact CosmosDB.");
            return null;
        }
    }
}