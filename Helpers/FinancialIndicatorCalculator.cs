namespace SmartInvestor.Helpers;

public static class FinancialIndicatorCalculator
{
    public static double CurrentRatio(double currentAssets, double currentLiabilities)
    {
        return currentAssets / currentLiabilities;
    }

    public static double PriceEarningsRatio(double pricePerShare, List<double> earningsPerShare)
    {
        return pricePerShare / (earningsPerShare.Sum() / earningsPerShare.Count);
    }

    public static double PriceBookValue(double pricePerShare, double totalAssets, double totalLiabilities,
        double numberOfOutstandingShares)
    {
        return pricePerShare / ((totalAssets - totalLiabilities) / numberOfOutstandingShares);
    }
}