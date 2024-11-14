using SmartInvestor.Models;

namespace SmartInvestor.Repositories.Interfaces;

public interface IEdgarRepository
{
    Task<List<Company>> GetCompanies();
    Task DownloadCompaniesFacts();
    Task<CompanyFacts?> GetCompanyFacts(string cik);
}