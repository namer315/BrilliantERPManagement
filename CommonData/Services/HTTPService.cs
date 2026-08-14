using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace CommonData.Services;

public class HTTPService
{
    //private readonly HttpClient _httpClient = new HttpClient();

    public async Task<HttpResponseMessage> PostAsync(string url , string jsonPayload, AuthenticationHeaderValue authenticationHeaderValue = null)
    {
        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post , url);

        request.Headers.Authorization = authenticationHeaderValue;
        request.Content = new StringContent(jsonPayload , Encoding.UTF8 , "application/json");

        var response = await client.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        //Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        //Console.WriteLine(responseBody);

        //if (!response.IsSuccessStatusCode)
        //{
        //    throw new HttpRequestException(
        //        $"HTTP request failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{responseBody}");
        //}

        return response;
    }

    /// <summary>Simple GET with optional auth header.</summary>
    public async Task<string> GetAsync(string url , AuthenticationHeaderValue authenticationHeaderValue = null)
    {
        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get , url);

        request.Headers.Authorization = authenticationHeaderValue;

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

    /// <summary>GET with query parameters appended to the URL.</summary>
    public async Task<string> GetAsync(string url , Dictionary<string , string> queryParams , AuthenticationHeaderValue authenticationHeaderValue = null)
    {
        var queryString = string.Join("&" ,
            queryParams.Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));

        var fullUrl = url.Contains('?') ? $"{url}&{queryString}" : $"{url}?{queryString}";

        return await GetAsync(fullUrl , authenticationHeaderValue);
    }
}
