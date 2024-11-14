using SQLite;

namespace SmartInvestor.Models.DTOs;

[Table(Constants.CompanyDtosTable)]
public class CompanyDto
{
    [PrimaryKey] [MaxLength(10)] public string? Cik { get; init; }
    [NotNull] public string? Name { get; init; }
    [NotNull] public string? Ticker { get; init; }
    [NotNull] public double? MarketCap { get; set; }
    [NotNull] public double? CurrentRatio { get; set; }
    [NotNull] public double? PriceBookValue { get; set; }
    [NotNull] public double? PriceEarningsRatio { get; set; }
    [NotNull] public int? EarningsGrowthPercentage { get; set; }
    [NotNull] public int? DividendsGrowthYears { get; set; }
    [NotNull] public int? EarningsPerShareGrowthYears { get; set; }
}