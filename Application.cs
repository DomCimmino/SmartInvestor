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
        foreach (var company in companies)
        {
            if (filteredCompanies.Any(c => c.Cik == company.Cik)) continue;
            var companyFact = await edgarService.GetCompanyFacts(company.Cik ?? string.Empty);
            if (companyFact?.HasAllNonNullProperties() != true) continue;
            company.CompanyHistoryData = companyFact;
            company.JsonCompanyHistoryData = JsonConvert.SerializeObject(company.CompanyHistoryData);
            filteredCompanies.Add(company);
        }

        await sqLiteService.InsertCompanies(filteredCompanies);
    }

    private async Task UploadCompanyDtos()
    {
        var companyDtos = new List<CompanyDto>();
        var companies = await sqLiteService.GetCompanies();
        var pricesPerShare = FinancialIndicatorCalculator.GetPricesPerShare(companies
            .Select(c => c.Ticker)
            .Where(ticker => !string.IsNullOrEmpty(ticker) && !string.IsNullOrWhiteSpace(ticker)).ToList());
        var filteredCompanies = companies
            .Where(c => !string.IsNullOrEmpty(c.Ticker) && !string.IsNullOrWhiteSpace(c.Ticker) && pricesPerShare.ContainsKey(c.Ticker))
            .ToList();

        foreach (var company in filteredCompanies)
        {
            var marketCap = company.CompanyHistoryData?.Facts?.DocumentAndEntityInformation?.EntityPublicFloat?.Unit
                ?.Usd
                ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value ?? -1;

            var outstandingShares = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy
                ?.CommonStockSharesOutstanding?.Unit?.Shares
                ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value ?? -1;

            var pricePerShare = pricesPerShare.GetValueOrDefault(company.Ticker ?? string.Empty);

            var earningsPerShareValues = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy?.EarningsPerShare
                ?.Unit?.UsdAndShares
                ?.Where(x => x is { Form: "10-K", FiscalYear: 2019 or 2018 or 2017, Value: not null })
                .Select(x => (double?)x.Value)
                .ToList();

            var assets = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy?.Assets?.Unit?.Usd
                ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value ?? -1;

            var currentAssets = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy?.CurrentAssets?.Unit?.Usd
                ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value ?? -1;

            var currentLiabilities = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy?.Liabilities?.Unit
                ?.Usd
                ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value ?? -1;

            companyDtos.Add(new CompanyDto
            {
                Cik = company.Cik,
                Name = company.Name,
                Ticker = company.Ticker,
                MarketCap = marketCap,
                CurrentRatio = FinancialIndicatorCalculator.CurrentRatio(currentAssets, currentLiabilities),
                PriceEarningsRatio =
                    FinancialIndicatorCalculator.PriceEarningsRatio(pricePerShare, earningsPerShareValues ?? []),
                PriceBookValue = FinancialIndicatorCalculator.PriceBookValue(pricePerShare, assets,
                    currentLiabilities, outstandingShares)
            });
        }

        await sqLiteService.InsertCompanyDtos(companyDtos);
    }
}