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
    public static string OptimizationScriptPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Optimization", "find_best_company.py");
}