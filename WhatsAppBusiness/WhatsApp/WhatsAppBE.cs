using CommonData.Services;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

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



}
