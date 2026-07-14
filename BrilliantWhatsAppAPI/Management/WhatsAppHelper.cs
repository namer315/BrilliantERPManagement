using BrilliantWhatsAppAPI.DTO;

namespace BrilliantWhatsAppAPI.Management;

public class WhatsAppHelper
{
    /// <summary>
    /// {WHATSAPP_BUSINESS_ACCOUNT_ID} or {WABA_ID}
    /// </summary>
    private string _WhatsAppBusinessAccountId = "1550103273123935";
    private readonly string _phoneNumberId = "1124475747418242";
    private readonly string _accessToken = "EAARC64ekrboBRvMRrQ2gF3MZC1hopZCdrPoHiptdpr9TTq10ugRMX9ZAgzzolhs3kSqdAIAHojZA6Pog3lkwxHjKKmEkNZCLbWAzHpoJCH0QFvlttYPZCUeJVRuTygGdi3NgTJcTvxzX4JRkL3iBsCr5A0QbUZAaPrG7EvCayZBZA2X1znRe54pQsSZAhz7ZBB4nwZDZD";

    private readonly WhatsAppManager _payloadBuilder = new();
    private readonly WhatsAppHTTPClientManager _httpClientHelper;

    public WhatsAppHelper()
    {
        _httpClientHelper = new WhatsAppHTTPClientManager(_accessToken);
    }

    public async Task<string> SendMessage(string to , string message)
    {
        Console.WriteLine($"Sending message to {to}: {message}");

        var url = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/messages";
        var payload = _payloadBuilder.BuildTextMessagePayload(to , message);

        return await _httpClientHelper.PostAsync(url , payload);
    }

    public async Task<string> SendTemplateMessageAsync(
        string to ,
        string templateName ,
        IList<tTemplateParameter> parameterList = null ,
        string languageCode = "en_US")
    {
        var url = $"https://graph.facebook.com/v25.0/{_phoneNumberId}/messages";
        var payload = _payloadBuilder.BuildTemplateMessagePayload(to , templateName , parameterList , languageCode);

        return await _httpClientHelper.PostAsync(url , payload);
    }

    //public async Task<string> GetAllTemplatesAsync()
    //{
    //    var url = $"https://graph.facebook.com/v17.0/{_phoneNumberId}/message_templates";
    //    return await _httpClientHelper.GetAsync(url);
    //}

    /// <summary>
    /// Fetches every message template provisioned under the configured WhatsApp Business Account.
    /// This is the canonical way to audit your template library — names, statuses, categories,
    /// languages, and component structures are all returned in a single call.
    /// </summary>
    /// <param name="fields">
    /// A comma-separated list of template properties to include in the response.
    /// Defaults to <c>"name,status,category,language,components"</c>.
    /// </param>
    /// <param name="limit">
    /// Maximum number of templates to return per page. The Cloud API caps this at 100;
    /// this method defaults to a conservative 50.
    /// </param>
    /// <param name="ct">A token to cancel the request mid-flight if the caller gives up.</param>
    /// <returns>
    /// The raw JSON response body from the Graph API as a string. Successful responses
    /// contain a <c>"data"</c> array whose elements represent individual template objects.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the internal WABA ID or access token has not been configured.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Thrown when the API refuses the request — for example, an expired token (401),
    /// insufficient permissions (403), or a malformed URL (404).
    /// </exception>
    /// <exception cref="TaskCanceledException">
    /// Thrown when the request times out or the caller cancels via <paramref name="ct"/>.
    /// </exception>
    public async Task<object> GetAllTemplatesAsync(
        string fields = "name,status,category,language,components" ,
        int limit = 50 ,
        CancellationToken ct = default)
    {
        // --- Input validation ---
        if (string.IsNullOrWhiteSpace(_WhatsAppBusinessAccountId))
            throw new ArgumentNullException(nameof(_WhatsAppBusinessAccountId) , "WABA ID cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(_accessToken))
            throw new ArgumentNullException(nameof(_accessToken) , "Access token cannot be null or empty.");

        // --- Build the URL ---
        var url = $"https://graph.facebook.com/v22.0/{_WhatsAppBusinessAccountId}/message_templates"
                + $"?fields={Uri.EscapeDataString(fields)}"
                + $"&limit={limit}";

        return await _httpClientHelper.GetAsync(url);

    }
}
