using System.Runtime.CompilerServices;
using Python.Runtime;

namespace SmartInvestor.Helpers;

public static class FinancialIndicatorCalculator
{
    public static double CurrentRatio(double currentAssets, double currentLiabilities)
    {
        return currentAssets / currentLiabilities;
    }

    public static Dictionary<string, double?> GetPricesPerShareBatch(List<string?> symbols)
    {
        var prices = new Dictionary<string, double?>();
        PythonEngine.Initialize();
        using (Py.GIL())
        {
            dynamic yf = Py.Import("yfinance");
            
            var sum = 0d;
            var count = 0d;
            
            foreach (var symbol in symbols)
            {
                var ticker = yf.Ticker(symbol);
                var history = ticker.history($"{DateTime.UtcNow.Year - 2019}y");
                
                foreach (var item in history.itertuples())
                {
                    var year = item.Index.year.As<int>();
                    if (year != 2019) continue;
                    var close = item.Close.As<double>();
                    sum += double.IsNaN(close) ? 0 : close;
                    count++;
                }
                if (count <= 0) prices[symbol] = null;
                prices[symbol] =  sum / count;
                
                sum = 0;
                count = 0;
            }

            return prices;
        }
    }

    public static double? PriceEarningsRatio(double pricePerShare, List<double?> earningsPerShare)
    {
        return pricePerShare / earningsPerShare.Average();
    }

    public static double PriceBookValue(double pricePerShare, long totalAssets, long totalLiabilities,
        double numberOfOutstandingShares)
    {
        return pricePerShare / ((totalAssets - totalLiabilities) / numberOfOutstandingShares);
    }
}