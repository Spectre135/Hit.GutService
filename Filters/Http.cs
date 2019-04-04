using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

public class Http
{
    public static HttpClient GetHttpClient(string url, string token)
    {
        HttpClientHandler handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        HttpClient client = new HttpClient(handler)
        {
            BaseAddress = new Uri(url)
        };
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}