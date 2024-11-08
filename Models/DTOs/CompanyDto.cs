using SQLite;

namespace SmartInvestor.Models.DTOs;

public class CompanyDto
{
    [NotNull] [PrimaryKey] public string? Cik { get; init; }
    [NotNull] public string? Name { get; init; }
    [NotNull] public string? Ticker { get; init; }
    [NotNull] public double? MarketCap { get; init; }
    [NotNull] public double? CurrentRatio { get; init; }
    [NotNull] public double? PriceBookValue { get; init; }
    [NotNull] public double? PriceEarningsRatio { get; init; }
    [NotNull] public string? EarningsJson { get; init; }
    [NotNull] public string? DividendsJson { get; init; }
    [NotNull] public string? EarningsPerShareJson { get; init; }
}