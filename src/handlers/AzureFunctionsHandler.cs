using System.Text;
using Newtonsoft.Json;

public class AzureFunctionsHandler
{
    private readonly HttpClient httpClient = new HttpClient();

    public async Task<URLReply?> GetDocumentURL(string FileName)
    {
        URLRequest urlRequest = new URLRequest(FileName);
        StringContent data = new StringContent(urlRequest.ToString(), Encoding.UTF8, "application/json");

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Program.FUNCTION_GET_URL);
        request.Content = data;

        using (HttpResponseMessage response = await httpClient.SendAsync(request))
        {
            var statusCode = response.StatusCode;
            if ((int)statusCode != 200)
            {
                Console.WriteLine($"AzureFunctionsHandler got response {(int)statusCode}");
                return null;
            }
            return await GetRequest(response.Content.ReadAsStream());
        }
    }

    public async Task<bool> PurgeCache(string FileName)
    {
        URLRequest urlRequest = new URLRequest(FileName);
        StringContent data = new StringContent(urlRequest.ToString(), Encoding.UTF8, "application/json");

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, Program.FUNCTION_PURGE_URL);
        request.Content = data;

        using (HttpResponseMessage response = await httpClient.SendAsync(request))
        {
            var statusCode = response.StatusCode;
            if ((int)statusCode != 204) // Function sends 204, since we get no data back.
            {
                Console.WriteLine($"AzureFunctionsHandler got response {(int)statusCode}");
                return false;
            }
            return true;
        }
    }

    private async Task<URLReply?> GetRequest(Stream stream)
    {
        string json = await new StreamReader(stream).ReadToEndAsync();
        if (String.IsNullOrEmpty(json))
        {
            return null;
        }
        Console.WriteLine($"Response body was {json}");
        return JsonConvert.DeserializeObject<URLReply>(json);
    }
}