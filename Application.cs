using Newtonsoft.Json;
using SmartInvestor.Helpers;
using SmartInvestor.Models;
using SmartInvestor.Models.DTOs;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor;

public class Application(ISqLiteService sqLiteService, IEdgarService edgarService)
{
    public async Task Init()
    {
        await sqLiteService.InitDatabase();
        await edgarService.DownloadCompaniesFacts();
        if (!await sqLiteService.HasCompanies()) await UploadCompanies();
        if (!await sqLiteService.HasCompanyDtos()) await UploadCompanyDtos();
    }

    private async Task UploadCompanies()
    {
        var companies = await edgarService.GetCompanies();
        var filteredCompanies = new List<Company>();
        await Parallel.ForEachAsync(companies, async (company, _) =>
        {
            var companyFact = await edgarService.GetCompanyFacts(company.Cik ?? string.Empty);
            if (companyFact?.HasAllNonNullProperties() == true)
            {
                company.CompanyHistoryData = companyFact;
                company.JsonCompanyHistoryData = JsonConvert.SerializeObject(company.CompanyHistoryData);
                filteredCompanies.Add(company);
            }
        });
        await sqLiteService.InsertCompanies(filteredCompanies);
    }

    private async Task UploadCompanyDtos()
    {
        var companyDtos = new List<CompanyDto>();
        var companies = await sqLiteService.GetCompanies();
        var pricesPerShare = FinancialIndicatorCalculator.GetPricesPerShareBatch(companies
            .Select(c => c.Ticker)
            .Where(ticker => !string.IsNullOrEmpty(ticker)).ToList());

        foreach (var company in companies)
        {
            var marketCap = company.CompanyHistoryData?.Facts?.DocumentAndEntityInformation?.EntityPublicFloat?.Unit
                ?.Usd
                ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value;

            var outstandingShares = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy
                ?.CommonStockSharesOutstanding?.Unit?.Shares
                ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value;

            var pricePerShare = pricesPerShare.GetValueOrDefault(company.Ticker ?? string.Empty);

            var earningsPerShareValues = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy?.EarningsPerShare
                ?.Unit?.UsdAndShares
                ?.Where(x => x is { Form: "10-K", FiscalYear: 2019 or 2018 or 2017, Value: not null })
                .Select(x => (double?)x.Value)
                .ToList();

            var assets = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy?.Assets?.Unit?.Usd
                ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value;

            var currentAssets = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy?.CurrentAssets?.Unit?.Usd
                ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value;

            var currentLiabilities = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy?.Liabilities?.Unit
                ?.Usd
                ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value;

            companyDtos.Add(new CompanyDto
            {
                Cik = company.Cik,
                Name = company.Name,
                Ticker = company.Ticker,
                MarketCap = marketCap,
                CurrentRatio = FinancialIndicatorCalculator.CurrentRatio(currentAssets ?? -1, currentLiabilities ?? -1),
                PriceEarningsRatio =
                    FinancialIndicatorCalculator.PriceEarningsRatio(pricePerShare ?? -1, earningsPerShareValues ?? []),
                PriceBookValue = FinancialIndicatorCalculator.PriceBookValue(pricePerShare ?? -1, assets ?? -1,
                    currentLiabilities ?? -1, outstandingShares ?? -1)
            });
        }

        await sqLiteService.InsertCompanyDtos(companyDtos);
    }
}