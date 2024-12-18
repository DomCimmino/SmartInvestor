using Newtonsoft.Json;
using SQLite;

namespace SmartInvestor.Models;

[Table(Constants.CompaniesTable)]
public class Company
{
    [PrimaryKey] [MaxLength(10)]  public string? Cik { get; init; }
    [NotNull] public string? Name { get; init; }
    [NotNull] public string? Ticker { get; init; }
    [Ignore] public CompanyFacts? CompanyHistoryData { get; set; }
    [NotNull] public string? JsonCompanyHistoryData { get; set; }
}

public class CompanyData
{
    [JsonProperty("data")] public List<List<object>>? Data { get; set; }
}