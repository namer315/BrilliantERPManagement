using CommonData.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using WhatsAppData.DTO.Webhooks;

namespace WhatsAppBusiness.WhatsApp;

public class WhatsAppBE
{
    /// <summary>
    /// {WHATSAPP_BUSINESS_ACCOUNT_ID} or {WABA_ID}
    /// </summary>
    private string _WhatsAppBusinessAccountId = "1550103273123935";
    private readonly string _phoneNumberId = "1124475747418242";
    private readonly string _accessToken = "EAARC64ekrboBRvMRrQ2gF3MZC1hopZCdrPoHiptdpr9TTq10ugRMX9ZAgzzolhs3kSqdAIAHojZA6Pog3lkwxHjKKmEkNZCLbWAzHpoJCH0QFvlttYPZCUeJVRuTygGdi3NgTJcTvxzX4JRkL3iBsCr5A0QbUZAaPrG7EvCayZBZA2X1znRe54pQsSZAhz7ZBB4nwZDZD";

    private readonly static HTTPService _HTTPService = new HTTPService();

    private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<HttpResponseMessage> PostAsync(string subURL , string jsonPayload)
    {
        string url = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/{subURL}";

        return await _HTTPService.PostAsync(url , jsonPayload , new AuthenticationHeaderValue("Bearer" , _accessToken));
    }
    public async Task<T> PostAsync<T>(string subURL , string jsonPayload)
    {
        HttpResponseMessage response = await PostAsync(subURL , jsonPayload);
        string responseBody = await response.Content.ReadAsStringAsync();

        #if DEBUG
        Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        Console.WriteLine(responseBody);
        #endif

        if (!response.IsSuccessStatusCode)
        {
            //throw new HttpRequestException(
            //    $"HTTP request failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{responseBody}");
            ErrorResponseDTO errorResponse = JsonSerializer.Deserialize<ErrorResponseDTO>(responseBody , _serializerOptions);
            // throw new WhatsAppApiException(errorResponse?.Error);
            throw new CommonData.Exceptions.AppException(
                    errorResponse.Error.Message ?? "WhatsApp Cloud API returned an error." ,
                    CommonData.Exceptions.AppErrorType.ExternalService ,
                    code: errorResponse.Error.Code.ToString() ,
                    httpStatusCode: (int?)response.StatusCode,
                    details: new Dictionary<string , object?>
                    {
                        ["type"] = errorResponse.Error.Type ,
                        ["title"] = errorResponse.Error.Title ,
                        ["fbtraceId"] = errorResponse.Error.FbTraceId ,
                        ["href"] = errorResponse.Error.Href ,
                        ["details"] = errorResponse.Error.ErrorData?.Details ,
                    });
        }

        return JsonSerializer.Deserialize<T>(responseBody , _serializerOptions);
    }

    public async Task<string> GetWABAAsync(string subURL)
    {
        // --- Input validation ---
        if (string.IsNullOrWhiteSpace(_WhatsAppBusinessAccountId))
            throw new ArgumentNullException(nameof(_WhatsAppBusinessAccountId) , "WABA ID cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(_accessToken))
            throw new ArgumentNullException(nameof(_accessToken) , "Access token cannot be null or empty.");

        string url = $"https://graph.facebook.com/v22.0/{_WhatsAppBusinessAccountId}/{subURL}";

        return await _HTTPService.GetAsync(url , new AuthenticationHeaderValue("Bearer" , _accessToken));
    }

    public async Task<T> GetWABAAsync<T>(string subURL)
    {
        string responce = await GetWABAAsync(subURL);

        return JsonSerializer.Deserialize<T>(responce , new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
    }


    // Local helper: upload raw bytes to WhatsApp and return the media ID.
    public async Task<string> UploadMediaAsync(byte[] fileBytes , string fileName , string mimeType)
    {
        string url = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/media";
        using MultipartFormDataContent formDataContent = _HTTPService.CreateMultipartFormDataContent(fileBytes , fileName , mimeType, "whatsapp" , "messaging_product");
        HttpResponseMessage response = await _HTTPService.UploadMediaAsync(url , formDataContent , new AuthenticationHeaderValue("Bearer" , _accessToken));
        string responseBody = await response.Content.ReadAsStringAsync();

#if DEBUG
        Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        Console.WriteLine(responseBody);
#endif

        if (!response.IsSuccessStatusCode)
        {
            //throw new HttpRequestException(
            //    $"HTTP request failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{responseBody}");
            ErrorResponseDTO errorResponse = JsonSerializer.Deserialize<ErrorResponseDTO>(responseBody , _serializerOptions);
            // throw new WhatsAppApiException(errorResponse?.Error);
            throw new CommonData.Exceptions.AppException(
                    errorResponse.Error.Message ?? "WhatsApp Cloud API returned an error." ,
                    CommonData.Exceptions.AppErrorType.ExternalService ,
                    code: errorResponse.Error.Code.ToString() ,
                    httpStatusCode: (int?)response.StatusCode ,
                    details: new Dictionary<string , object?>
                    {
                        ["type"] = errorResponse.Error.Type ,
                        ["title"] = errorResponse.Error.Title ,
                        ["fbtraceId"] = errorResponse.Error.FbTraceId ,
                        ["href"] = errorResponse.Error.Href ,
                        ["details"] = errorResponse.Error.ErrorData?.Details ,
                    });
        }
        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement.GetProperty("id").GetString();
    }
}
