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
        var companiesDto = new List<CompanyDto>();
        foreach (var company in companies)
        {
            if (await sqLiteService.IsCompanyUploaded(company.Cik ?? string.Empty)) continue;
            var companyFact = await edgarService.GetCompanyFacts(company.Cik ?? string.Empty);
            if (companyFact is
                {
                    Cik: not null,
                    EntityName: not null,
                    Facts:
                    {
                        DocumentAndEntityInformation.EntityPublicFloat: not null,
                        FinancialReportingTaxonomy:
                        {
                            Assets: not null,
                            Liabilities: not null,
                            CurrentAssets: not null,
                            EarningsPerShare: not null,
                            CommonStockSharesOutstanding: not null
                        }
                    }
                })
            {
                companiesDto.Add(mapper.Map<CompanyDto>(companyFact));
            }
        }

        await sqLiteService.InsertCompanies(companiesDto);
    }
}