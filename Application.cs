using SmartInvestor.Models;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor;

public class Application(IEdgarService edgarService)
{
    private Dictionary<Company, CompanyFacts> _data = new();

    public async Task Init()
    {
        await edgarService.DownloadCompaniesFacts();
        await LoadCompaniesAndCompaniesFacts();
    }

    private async Task LoadCompaniesAndCompaniesFacts()
    {
        var companies = await edgarService.GetCompanies();

        const int maxDegreeOfParallelism = 50;

        await Parallel.ForEachAsync(companies, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
            async (company, token) =>
            {
                var companyFact = await edgarService.GetCompanyFacts(company.Cik ?? string.Empty);
                if (companyFact is
                    {
                        Facts.FinancialReportingTaxonomy:
                        {
                            Assets: not null,
                            Liabilities: not null,
                            CurrentAssets: not null,
                            EarningsPerShare: not null,
                            CommonStockSharesOutstanding: not null
                        }
                    })
                {
                    lock (_data)
                    {
                        _data.Add(company, companyFact);
                    }
                }
            });
    }
}