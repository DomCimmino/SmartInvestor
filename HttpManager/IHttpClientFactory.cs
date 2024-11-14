namespace SmartInvestor.HttpManager;

public interface IHttpClientFactory
{
    HttpClient GetHttpClient();
}