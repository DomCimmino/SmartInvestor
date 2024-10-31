using System.Net;
using System.Net.Http.Headers;

namespace SmartInvestor.HttpManager;

public class HttpClientFactory : IHttpClientFactory
{
    private readonly object _lock = new();
    private HttpClient? _httpClient;

    public HttpClient GetHttpClient()
    {
        lock (_lock)
        {
            if (_httpClient != null) return _httpClient;
            _httpClient = new HttpClient
            {
                DefaultRequestVersion = HttpVersion.Version20,
                BaseAddress = new Uri("https://data.sec.gov/")
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "client application");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("it-IT"));
            return _httpClient;
        }
    }
}