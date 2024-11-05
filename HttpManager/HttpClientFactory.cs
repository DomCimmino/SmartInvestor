using System.Net;
using System.Net.Http.Headers;

namespace SmartInvestor.HttpManager;

public class HttpClientFactory : IHttpClientFactory
{
    public HttpClient GetHttpClient()
    {
        var httpClient = new HttpClient
        {
            DefaultRequestVersion = HttpVersion.Version20,
            BaseAddress = new Uri("https://www.sec.gov/"),
            Timeout = TimeSpan.FromMinutes(30)
        };
        httpClient.DefaultRequestHeaders.Add("User-Agent", "client application");
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.AcceptEncoding.Clear();
        httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("it-IT"));
        return httpClient;
    }
}