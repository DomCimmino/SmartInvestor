using Python.Runtime;
using SmartInvestor.Models;

namespace SmartInvestor.Helpers;

public static class FinancialIndicatorCalculator
{
    public static double GetValueForYearAndForm(BasicFact basicFact, int year, string form)
    {
        var units = CheckBasicFacts(basicFact);

        if (units.Count == 0) return -1;

        var matchingUnit = units?
            .FirstOrDefault(x => x.FiscalYear == year && x.Form == form);

        return matchingUnit?.Value ?? -1;
    }

    public static int GetGrowthYears(BasicFact basicFact, string form)
    {
        var units = CheckBasicFacts(basicFact);
        if (units.Count == 0) return 0;

        var groupedByFiscalYear = units.Where(u => u.Form == form && u.FiscalYear < Constants.ReferenceYear)
            .GroupBy(u => u.FiscalYear ?? -1).ToDictionary(
                g => g.Key,
                g => g.Sum(u => u.Value ?? 0)
            );

        return groupedByFiscalYear.Select(fiscalYear => fiscalYear.Value)
            .Select(totalValue => totalValue switch
            {
                > 0 => 1,
                0 => 0,
                < 0 => -1
            })
            .Sum();
    }

    public static double CurrentRatio(double currentAssets, double currentLiabilities)
    {
        return currentAssets / currentLiabilities;
    }

    public static Dictionary<string, double> GetPricesPerShare(List<string?> symbols)
    {
        var prices = new Dictionary<string, double>();
        PythonEngine.Initialize();
        using (Py.GIL())
        {
            dynamic yf = Py.Import("yfinance");

            var historyData = yf.download(symbols, start: "2019-01-01", end: "2019-12-31");

            foreach (var symbol in symbols)
            {
                try
                {
                    var tickerData = historyData["Close"].get(symbol);
                    if (tickerData == null) continue;

                    double sum = 0;
                    var count = 0;

                    foreach (var item in tickerData.items())
                    {
                        var close = item[1].As<double>();
                        if (double.IsNaN(close)) continue;
                        sum += close;
                        count++;
                    }

                    if (count > 0 && symbol != null)
                    {
                        prices[symbol] = sum / count;
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