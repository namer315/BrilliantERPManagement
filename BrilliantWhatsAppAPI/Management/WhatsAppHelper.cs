using BrilliantWhatsAppAPI.DTO;

namespace BrilliantWhatsAppAPI.Management;

public class WhatsAppHelper
{
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

    public async Task<string> GetAllTemplatesAsync()
    {
        var url = $"https://graph.facebook.com/v17.0/{_phoneNumberId}/message_templates";
        return await _httpClientHelper.GetAsync(url);
    }
}
