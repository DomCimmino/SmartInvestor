using Python.Runtime;
using SmartInvestor.Models;

namespace SmartInvestor.Helpers;

public static class FinancialIndicatorCalculator
{
    public static double GetValueForYearAndForm(BasicFact basicFact)
    {
        var units = CheckBasicFacts(basicFact);

        if (units.Count == 0) return 1;

        var matchingUnit = units
            .FirstOrDefault(x => x is { FiscalYear: Constants.ReferenceYear, Form: Constants.ReferenceForm });

        return matchingUnit?.Value ?? 1;
    }

    public static int GetGrowthYears(BasicFact basicFact)
    {
        var units = CheckBasicFacts(basicFact);
        if (units.Count == 0) return 0;

        var groupedByFiscalYear = units.Where(u => u is { Form: Constants.ReferenceForm, FiscalYear: < Constants.ReferenceYear })
            .GroupBy(u => u.FiscalYear ?? -1).ToDictionary(
                g => g.Key,
                g => g.Sum(u => u.Value ?? 0)
            );

        var sortedYears = groupedByFiscalYear.OrderBy(kv => kv.Key).ToList();
        
        var differentialGrowthScore = 0;
        for (var i = 1; i < sortedYears.Count; i++)
        {
            var previousValue = sortedYears[i - 1].Value;
            var currentValue = sortedYears[i].Value;
            var difference = currentValue - previousValue;
            
            differentialGrowthScore += difference switch
            {
                > 0 => 1,  
                0 => 0,    
                < 0 => -2  
            };
        }
        
        var baseGrowthScore = groupedByFiscalYear.Select(fiscalYear => fiscalYear.Value)
            .Select(totalValue => totalValue switch
            {
                > 0 => 1,
                0 => 0,
                < 0 => -2
            })
            .Sum();
        
        return baseGrowthScore + differentialGrowthScore;
    }

    public static int GetEarningsGrowthPercentage(BasicFact basicFact)
    {
        var units = CheckBasicFacts(basicFact);
        if (units.Count == 0) return 0;
        
        var orderedValues = units.Where(u => u is { Form: Constants.ReferenceForm, FiscalYear: < Constants.ReferenceYear })
            .GroupBy(u => u.FiscalYear ?? -1).ToDictionary(
                g => g.Key,
                g => g.Sum(u => u.Value ?? 0)
            )
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value)
            .ToList();
        
        if (orderedValues.Count < 2) return 0;
        
        var groupSize = orderedValues.Count >= 6 ? 3 :
            orderedValues.Count >= 4 ? 2 : 1;
        
        var firstAverage = orderedValues.Take(groupSize).Average();
        var lastAverage = orderedValues.TakeLast(groupSize).Average();
        
        return firstAverage != 0
            ? (int)((lastAverage - firstAverage) / Math.Abs(firstAverage) * 100)
            : 0;
    }

    public static double? CurrentRatio(double currentAssets, double currentLiabilities)
    {
        return double.IsNaN(currentAssets / currentLiabilities) ? 0 : currentAssets / currentLiabilities;
    }

    public static Dictionary<string, double> GetPricesPerShare(List<string?> symbols)
    {
        var prices = new Dictionary<string, double>();
        PythonEngine.Initialize();
        using (Py.GIL())
        {
            dynamic yf = Py.Import("yfinance");

            var historyData = yf.download(symbols, start: "2019-01-02", end: "2019-01-03");

            foreach (var symbol in symbols)
            {
                try
                {
                    var tickerData = historyData["Open"].get(symbol);
                    if (tickerData == null) continue;

                    var price = tickerData.loc["2019-01-02"].As<double>();

                    if (!string.IsNullOrEmpty(symbol) && !double.IsNaN(price))
                    {
                        prices[symbol] = price;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process ticker '{symbol}': {ex.Message}");
                }
            }
        }

        return prices;
    }

    public static double? PriceEarningsRatio(double pricePerShare, List<double?> earningsPerShare)
    {
        if (earningsPerShare.Count == 0) return 0;
        return pricePerShare / earningsPerShare.Average();
    }

    public static double PriceBookValue(double pricePerShare, double totalAssets, double totalLiabilities,
        double numberOfOutstandingShares)
    {
        return pricePerShare / ((totalAssets - totalLiabilities) / numberOfOutstandingShares);
    }

    private static List<BasicUnit> CheckBasicFacts(BasicFact basicFact)
    {
        if (basicFact.Unit == null) return [];
        return basicFact.Unit.Shares ?? basicFact.Unit.Usd ?? basicFact.Unit.UsdAndShares ?? [];
    }
}