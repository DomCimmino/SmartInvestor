using Python.Runtime;

namespace SmartInvestor.Helpers;

public static class FinancialIndicatorCalculator
{
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

    public static double PriceBookValue(double pricePerShare, long totalAssets, long totalLiabilities,
        double numberOfOutstandingShares)
    {
        return pricePerShare / ((totalAssets - totalLiabilities) / numberOfOutstandingShares);
    }
}