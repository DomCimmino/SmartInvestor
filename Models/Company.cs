using Newtonsoft.Json;

namespace SmartInvestor.Models;

public class Company
{
    public string? Cik { get; init; }
    public string? Name { get; init; }
    public string? Ticker { get; init; }
}

public class CompanyData
{
    [JsonProperty("data")] public List<List<object>>? Data { get; set; }
}