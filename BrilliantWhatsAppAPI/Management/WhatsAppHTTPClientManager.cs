using System.Net.Http.Headers;
using System.Text;

namespace BrilliantWhatsAppAPI.Management;

public class WhatsAppHTTPClientManager
{
    private readonly string _accessToken;

    public WhatsAppHTTPClientManager(string accessToken)
    {
        _accessToken = accessToken;
    }

    public async Task<string> PostAsync(string url , string jsonPayload)
    {
        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post , url);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer" , _accessToken);
        request.Content = new StringContent(jsonPayload , Encoding.UTF8 , "application/json");

        var response = await client.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        Console.WriteLine(responseBody);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP request failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{responseBody}");
        }

        return responseBody;
    }

    public async Task<string> GetAsync(string url)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer" , _accessToken);

        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Error fetching data: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> UploadMediaAsync(string url , byte[] fileBytes , string filename , string mimeType)
    {
        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post , url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer" , _accessToken);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("whatsapp") , "messaging_product");
        form.Add(new StringContent(mimeType) , "type");
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        form.Add(fileContent , "file" , filename);

        request.Content = form;
        var response = await client.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        Console.WriteLine(responseBody);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"HTTP request failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{responseBody}");
        }

        return responseBody;
    }
}
