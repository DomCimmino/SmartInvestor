using Newtonsoft.Json;

namespace SmartInvestor.Models;

public class CompanyFacts
{
    [JsonProperty("cik")] public string? Cik { get; set; }
    [JsonProperty("entityName")] public string? EntityName { get; set; } 
    [JsonProperty("facts")] public Facts? Facts { get; set; }
    
    public bool HasAllNonNullProperties() =>
        Cik != null && EntityName != null && Facts?.HasAllNonNullProperties() == true;
}