using AutoMapper;
using Newtonsoft.Json;
using SmartInvestor.Helpers;
using SmartInvestor.Models;
using SmartInvestor.Models.DTOs;
using SmartInvestor.Repositories.Interfaces;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor.Services;

public class CompanyService(ICompanyRepository companyRepository, IEdgarRepository edgarRepository, IMapper mapper)
    : ICompanyService
{
    public async Task InitializeDatabaseAndDownloadData()
    {
        await companyRepository.InitDatabase();
        await edgarRepository.DownloadCompaniesFacts();
    }

    public async Task UploadCompanies()
    {
        if (await companyRepository.HasCompanies()) return;

        var companies = await edgarRepository.GetCompanies();
        var existingCiks = new HashSet<string>();
        var filteredCompanies = new List<Company>();

        foreach (var company in companies)
        {
            if (!existingCiks.Add(company.Cik ?? string.Empty)) continue;

            var companyFact = await edgarRepository.GetCompanyFacts(company.Cik ?? string.Empty);
            if (companyFact?.HasAllNonNullProperties() != true) continue;

            company.CompanyHistoryData = companyFact;
            company.JsonCompanyHistoryData = JsonConvert.SerializeObject(company.CompanyHistoryData);
            filteredCompanies.Add(company);
        }

        await companyRepository.InsertCompanies(filteredCompanies);
    }

    public async Task UploadCompanyDtos()
    {
        if (await companyRepository.HasCompanyDtos()) return;

        var companies = await companyRepository.GetCompanies();
        var tickers = companies.Select(c => c.Ticker).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
        var pricesPerShare = companyRepository.GetPricesPerShare(tickers);

        var companyDtos = companies
            .Where(c => !string.IsNullOrEmpty(c.Ticker) && pricesPerShare.ContainsKey(c.Ticker))
            .Select(company => MapCompany(company, pricesPerShare.GetValueOrDefault(company.Ticker ?? string.Empty)))
            .ToList();

        await companyRepository.InsertCompanyDtos(companyDtos);
    }

    private CompanyDto MapCompany(Company company,  double pricePerShare)
    {
        company.CompanyHistoryData = JsonConvert.DeserializeObject<CompanyFacts>(company.JsonCompanyHistoryData ?? string.Empty);

        var financialData = company.CompanyHistoryData?.Facts?.FinancialReportingTaxonomy ?? new FinancialReportingTaxonomy();
        var documentInfo = company.CompanyHistoryData?.Facts?.DocumentAndEntityInformation;
        var earningsPerShareValues = financialData.EarningsPerShare?.Unit?.UsdAndShares
            ?.Where(x => x is { Form: "10-K", FiscalYear: 2019 or 2018 or 2017, Value: not null })
            .Select(x => (double?)x.Value)
            .ToList();
        var companyDto = mapper.Map<CompanyDto>(company);
        
        companyDto.MarketCap = FinancialIndicatorCalculator.GetValueForYearAndForm(documentInfo?.EntityPublicFloat ?? new BasicFact());
        companyDto.CurrentRatio = FinancialIndicatorCalculator.CurrentRatio(
            FinancialIndicatorCalculator.GetValueForYearAndForm(financialData.CurrentAssets ?? new BasicFact()),
            FinancialIndicatorCalculator.GetValueForYearAndForm(financialData.Liabilities ?? new BasicFact())
        );
        companyDto.PriceEarningsRatio = FinancialIndicatorCalculator.PriceEarningsRatio(pricePerShare, earningsPerShareValues ?? []);
        companyDto.PriceBookValue = FinancialIndicatorCalculator.PriceBookValue(
            pricePerShare,
            FinancialIndicatorCalculator.GetValueForYearAndForm(financialData.Assets ?? new BasicFact()),
            FinancialIndicatorCalculator.GetValueForYearAndForm(financialData.Liabilities ?? new BasicFact()),
            FinancialIndicatorCalculator.GetValueForYearAndForm(financialData.CommonStockSharesOutstanding ?? new BasicFact())
        );
        companyDto.DividendsGrowthYears = FinancialIndicatorCalculator.GetGrowthYears(financialData.PaidDividends ?? new BasicFact());
        companyDto.EarningsGrowthPercentage = FinancialIndicatorCalculator.GetEarningsGrowthPercentage(financialData.EarningsPerShare ?? new BasicFact());
        companyDto.EarningsPerShareGrowthYears = FinancialIndicatorCalculator.GetGrowthYears(financialData.EarningsPerShare ?? new BasicFact());

        return companyDto;
    }
}