using SmartInvestor.Models;

namespace SmartInvestor.Services.Interfaces;

public interface IEdgarService
{
    Task<CompanyFacts?> GetCompanyFacts(string cik);
}