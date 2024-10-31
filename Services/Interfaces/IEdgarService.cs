namespace SmartInvestor.Services.Interfaces;

public interface IEdgarService
{
    Task<string> GetCompanyFacts(string cik);
}