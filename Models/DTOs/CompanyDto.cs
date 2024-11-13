using SQLite;

namespace SmartInvestor.Models.DTOs;

public class CompanyDto
{
    [PrimaryKey] public string? Cik { get; init; }
    [NotNull] public string? Name { get; init; }
    [NotNull] public string? Ticker { get; init; }
    [NotNull] public double? MarketCap { get; init; }
    [NotNull] public double? CurrentRatio { get; init; }
    [NotNull] public double? PriceBookValue { get; init; }
    [NotNull] public double? PriceEarningsRatio { get; init; }
    [NotNull] public int? EarningsGrowthYears { get; init; }
    [NotNull] public int? DividendsGrowthYears { get; init; }
    [NotNull] public int? EarningsPerShareGrowthYears { get; init; }
}