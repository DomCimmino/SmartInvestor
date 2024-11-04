using Newtonsoft.Json;

namespace SmartInvestor.Models;

public class Unit
{
    [JsonProperty("shares")] public List<BasicUnit>? Shares { get; set; }
    [JsonProperty("USD")] public List<BasicUnit>? Usd { get; set; }
}

public class BasicUnit
{
    [JsonProperty("start")] public DateTime? StartDate { get; set; }
    [JsonProperty("end")] public DateTime? EndDate { get; set; }
    [JsonProperty("val")] public long? Value { get; set; }
    [JsonProperty("accn")] public string? AccessionNumber { get; set; }
    [JsonProperty("fy")] public int FiscalYear { get; set; }
    [JsonProperty("fp")] public string? FiscalPeriod { get; set; }
    [JsonProperty("form")] public string? Form { get; set; }
    [JsonProperty("filed")] public DateTime? FilingDate { get; set; }
    [JsonProperty("frame")] public string? CalendarYear { get; set; }
}