using CommonData.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AIDBAgentBusiness;
    
public class AIBE
{
    private readonly HTTPService _HTTPService = new HTTPService();
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _endpoint;
    private readonly string _model;
    private readonly string _apiKey;

    private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    public AIBE(
        string baseUrl ,
        string apiKey ,
        string model ,
        string endpoint = "/chat/completions")
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer" , apiKey);

        _baseUrl = baseUrl;
        _endpoint = endpoint;
        _model = model;
        _apiKey = apiKey;
    }

    public async Task<string> SendPromptAsync(string prompt , string systemPrompt = "You are a helpful assistant.")
    {
        var payload = new
        {
            model = _model ,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                },
                new{
                    role = "user",
                    content = prompt
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);

        HttpResponseMessage response = await _HTTPService.PostAsync(_baseUrl + _endpoint , json , new AuthenticationHeaderValue("Bearer" , _apiKey));

        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {

        }

        //return JsonSerializer.Deserialize<T>(responseBody , _serializerOptions);
        return responseBody;
        //using var content = new StringContent(
        //json ,
        //Encoding.UTF8 ,
        //"application/json");

        //var response =
        //await _httpClient.PostAsync(_endpoint , content);

        //response.EnsureSuccessStatusCode();

        //var responseJson =
        //await response.Content.ReadAsStringAsync();

        //using var doc = JsonDocument.Parse(responseJson);

        //return doc.RootElement
        //.GetProperty("choices")[0]
        //.GetProperty("message")
        //.GetProperty("content")
        //.GetString() ?? string.Empty;
    }
}
