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
            var historyData = yf.download(symbols, period: $"{DateTime.UtcNow.Year - 2019}y");

            foreach (var symbol in symbols)
            {
                if (string.IsNullOrEmpty(symbol)) continue;
                var tickerData = historyData[symbol];
                if (tickerData == null || tickerData?.empty)
                {
                    var ticker = yf.Ticker(symbol);
                    tickerData = ticker.history(period: "max");

                    if (tickerData.empty)
                    {
                        prices[symbol] = null;
                        continue;
                    }
                }

                double sum = 0;
                double count = 0;

                foreach (var item in tickerData?.itertuples() ?? new List<dynamic>())
                {
                    var year = item.Index.year.As<int>();
                    if (year != 2019) continue;

                    var close = item.Close.As<double>();
                    sum += double.IsNaN(close) ? 0 : close;
                    count++;
                }

                prices[symbol] = count > 0 ? sum / count : null;
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