using Newtonsoft.Json;
using SmartInvestor.HttpManager;
using SmartInvestor.Models;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor.Services;

public class EdgarService(IHttpClientFactory clientFactory) : IEdgarService
{
    public async Task<CompanyFacts?> GetCompanyFacts(string cik)
    {
        using var client = clientFactory.GetHttpClient();
        try
        {
            var factsResponse = await client
                .GetAsync($"api/xbrl/companyfacts/CIK{cik}.json");
            return factsResponse is not { IsSuccessStatusCode: true, Content: not null }
                ? null
                : JsonConvert.DeserializeObject<CompanyFacts>(await factsResponse.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}