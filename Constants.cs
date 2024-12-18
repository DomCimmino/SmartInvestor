using SQLite;

namespace SmartInvestor;

public static class Constants
{
    public const string CompaniesTable = "Companies";
    public const string CompanyDtosTable = "CompanyDtos";
    public const int ReferenceYear = 2019;
    public const string ReferenceForm = "10-K";
    public const string BaseUri = "https://www.sec.gov/";
    public const string CompaniesApi = "files/company_tickers_exchange.json";
    public const string CompanyFactsApi = "Archives/edgar/daily-index/xbrl/companyfacts.zip";

    public static readonly string OptimizationScriptPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Optimization", "main.py");

    public static readonly string DatabasePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Resources", "smart_investor.db3");

    public const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.SharedCache;
}