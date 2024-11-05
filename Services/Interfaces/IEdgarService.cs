using SmartInvestor.Models;

namespace SmartInvestor.Services.Interfaces;

public interface IEdgarService
{
    Task<List<Company>> GetCompanies();
    Task DownloadCompaniesFacts();
    Task<CompanyFacts?> GetCompanyFacts(string cik);
}