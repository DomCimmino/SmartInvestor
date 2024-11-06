using System.Net;
using SmartInvestor.Models;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor;

public class Application(IEdgarService edgarService)
{
    private Dictionary<string, CompanyFacts> _data = new();

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
            async (company, _) =>
            {
                if (!_data.ContainsKey(company.Cik ?? string.Empty))
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
                            _data.Add(company.Cik ?? string.Empty, companyFact);
                        }
                    }
                }
            });
    }
}