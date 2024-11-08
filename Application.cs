using SmartInvestor.Helpers;
using SmartInvestor.Models;
using SmartInvestor.Models.DTOs;
using SmartInvestor.Services.Interfaces;
using IMapper = AutoMapper.IMapper;

namespace SmartInvestor;

public class Application(ISqLiteService sqLiteService, IEdgarService edgarService, IMapper mapper)
{
    public async Task Init()
    {
        await sqLiteService.InitDatabase();
        await edgarService.DownloadCompaniesFacts();
        await UploadCompaniesIntoSqlite();
    }

    private async Task UploadCompaniesIntoSqlite()
    {
        var companies = await edgarService.GetCompanies();
        var symbols = companies.Select(c => c.Ticker).Where(ticker => !string.IsNullOrEmpty(ticker)).ToList();
        var pricesPerShare = FinancialIndicatorCalculator.GetPricesPerShareBatch(symbols);
        var companiesDto = new List<CompanyDto>();

        await Parallel.ForEachAsync(companies, async (company, _) =>
        {
            if (await sqLiteService.IsCompanyUploaded(company.Cik ?? string.Empty)) return;
            var companyFact = await edgarService.GetCompanyFacts(company.Cik ?? string.Empty);
            if (companyFact?.HasAllNonNullProperties() == true)
            {
                var pricePerShare = pricesPerShare.GetValueOrDefault(company.Ticker);
                companiesDto.Add(MapCompanyFactIntoCompanyDto(companyFact, company.Ticker ?? string.Empty, pricePerShare));
            } 
        });

        await sqLiteService.InsertCompanies(companiesDto);
    }

    private CompanyDto MapCompanyFactIntoCompanyDto(CompanyFacts facts, string companySymbol, double? pricePerShare)
    {
        var marketCap = facts.Facts?.DocumentAndEntityInformation?.EntityPublicFloat?.Unit?.Usd
            ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value;

        var outstandingShares = facts.Facts?.FinancialReportingTaxonomy?.CommonStockSharesOutstanding?.Unit?.Shares
            ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value;

        var earningsPerShareValues = facts.Facts?.FinancialReportingTaxonomy?.EarningsPerShare?.Unit?.UsdAndShares
            ?.Where(x => x is { Form: "10-K", FiscalYear: 2019 or 2018 or 2017, Value: not null })
            .Select(x => (double?)x.Value)
            .ToList();

        var assets = facts.Facts?.FinancialReportingTaxonomy?.Assets?.Unit?.Usd
            ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value;

        var currentAssets = facts.Facts?.FinancialReportingTaxonomy?.CurrentAssets?.Unit?.Usd
            ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value;

        var currentLiabilities = facts.Facts?.FinancialReportingTaxonomy?.Liabilities?.Unit?.Usd
            ?.FirstOrDefault(x => x is { FiscalYear: 2019, Form: "10-K" })?.Value;

        var companyDto = new CompanyDto
        {
            Cik = facts.Cik,
            Name = facts.EntityName,
            Ticker = companySymbol,
            MarketCap = marketCap,
            CurrentRatio = FinancialIndicatorCalculator.CurrentRatio(currentAssets ?? -1, currentLiabilities ?? -1),
            PriceEarningsRatio =
                FinancialIndicatorCalculator.PriceEarningsRatio(pricePerShare ?? -1, earningsPerShareValues ?? []),
            PriceBookValue = FinancialIndicatorCalculator.PriceBookValue(pricePerShare ?? -1, assets ?? -1,
                currentLiabilities ?? -1, outstandingShares ?? -1)
        };

        return companyDto;
    }
}