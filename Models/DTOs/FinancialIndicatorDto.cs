using Newtonsoft.Json;
using SQLite;

namespace SmartInvestor.Models.DTOs;

public class FinancialIndicatorDto
{
    [JsonProperty("year")] [NotNull] public int? Year { get; set; }
    [JsonProperty("value")] [NotNull] public double? Value { get; set; }
}