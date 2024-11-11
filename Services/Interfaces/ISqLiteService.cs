using SmartInvestor.Models;
using SmartInvestor.Models.DTOs;

namespace SmartInvestor.Services.Interfaces;

public interface ISqLiteService
{
    Task InitDatabase();
    Task<List<Company>> GetCompanies();
    Task<bool> HasCompanies();
    Task<bool> HasCompanyDtos();
    Task<bool> InsertCompanies(List<Company> companies);
    Task<bool> InsertCompanyDtos(List<CompanyDto> companyDtos);
    Task<bool> IsCompanyUploaded(string cik);
}