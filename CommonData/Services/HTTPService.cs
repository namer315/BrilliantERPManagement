using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace CommonData.Services;

public class HTTPService
{
    //private readonly HttpClient _httpClient = new HttpClient();

    public async Task<string> PostAsync(string url , string jsonPayload, AuthenticationHeaderValue authenticationHeaderValue = null)
    {
        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post , url);

        request.Headers.Authorization = authenticationHeaderValue;
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
}
