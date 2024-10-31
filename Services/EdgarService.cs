using SmartInvestor.HttpManager;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor.Services;

public class EdgarService(IHttpClientFactory clientFactory) : IEdgarService
{
    public async Task<string> GetCompanyFacts(string cik)
    {
        using var client = clientFactory.GetHttpClient();
        try
        {
            var factsResponse = await client
                .GetAsync($"api/xbrl/companyfacts/CIK{cik}.json");
            if (factsResponse.IsSuccessStatusCode)
            {
                return await factsResponse.Content.ReadAsStringAsync();
            }

            return string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}