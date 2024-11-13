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
            .Where(c => !string.IsNullOrEmpty(c.Ticker) && !string.IsNullOrWhiteSpace(c.Ticker) &&
                        pricesPerShare.ContainsKey(c.Ticker))
            .ToList();

        foreach (var company in filteredCompanies)
        {
            company.CompanyHistoryData =
                JsonConvert.DeserializeObject<CompanyFacts>(company.JsonCompanyHistoryData ?? string.Empty);

            var documentAndEntityInformation = company.CompanyHistoryData?.Facts?.DocumentAndEntityInformation;
            var financialReportingTaxonomy = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy;

            var marketCap = FinancialIndicatorCalculator.GetValueForYearAndForm(
                documentAndEntityInformation?.EntityPublicFloat ?? new BasicFact(),
                Constants.ReferenceYear, Constants.ReferenceForm);

            var outstandingShares = FinancialIndicatorCalculator.GetValueForYearAndForm(
                financialReportingTaxonomy?.CommonStockSharesOutstanding ?? new BasicFact(),
                Constants.ReferenceYear, Constants.ReferenceForm);

            var assets = FinancialIndicatorCalculator.GetValueForYearAndForm(
                financialReportingTaxonomy?.Assets ?? new BasicFact(),
                Constants.ReferenceYear, Constants.ReferenceForm);

            var currentAssets = FinancialIndicatorCalculator.GetValueForYearAndForm(
                financialReportingTaxonomy?.CurrentAssets ?? new BasicFact(),
                Constants.ReferenceYear, Constants.ReferenceForm);

            var currentLiabilities = FinancialIndicatorCalculator.GetValueForYearAndForm(
                financialReportingTaxonomy?.Liabilities ?? new BasicFact(),
                Constants.ReferenceYear, Constants.ReferenceForm);

            var dividendGrowthYears =
                FinancialIndicatorCalculator.GetGrowthYears(
                    financialReportingTaxonomy?.PaidDividends ?? new BasicFact(), Constants.ReferenceForm);

            var pricePerShare = pricesPerShare.GetValueOrDefault(company.Ticker ?? string.Empty);

            var earningsPerShareValues = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy?.EarningsPerShare
                ?.Unit?.UsdAndShares
                ?.Where(x => x is { Form: "10-K", FiscalYear: 2019 or 2018 or 2017, Value: not null })
                .Select(x => (double?)x.Value)
                .ToList();

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
                    currentLiabilities, outstandingShares),
                DividendsGrowthYears = dividendGrowthYears
            });
        }

        await sqLiteService.InsertCompanyDtos(companyDtos);
    }
}