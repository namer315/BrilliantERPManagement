using CommonData.Services;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WhatsAppData.DTO.WhatsApp;

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

    public async Task<string> PostAsync(string subURL , string jsonPayload)
    {
        string url = $"https://graph.facebook.com/v22.0/{_phoneNumberId}/{subURL}";

        return await _HTTPService.PostAsync(url , jsonPayload , new AuthenticationHeaderValue("Bearer" , _accessToken));
    }
    public async Task<T> PostAsync<T>(string subURL , string jsonPayload)
    {
        string responce = await PostAsync(subURL , jsonPayload);

        return JsonSerializer.Deserialize<T>(responce, new JsonSerializerOptions()
        {
           PropertyNameCaseInsensitive = true
        });
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

        return JsonSerializer.Deserialize<T>(responce, new JsonSerializerOptions()
        {
           PropertyNameCaseInsensitive = true
        });
    }



}
