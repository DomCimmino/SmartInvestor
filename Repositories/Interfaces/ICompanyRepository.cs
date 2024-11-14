using SmartInvestor.Models;
using SmartInvestor.Models.DTOs;

namespace SmartInvestor.Repositories.Interfaces;

public interface ICompanyRepository
{
    Task InitDatabase();
    Task<List<Company>> GetCompanies();
    Task<bool> HasCompanies();
    Task<bool> HasCompanyDtos();
    Task InsertCompanies(List<Company> companies);
    Task InsertCompanyDtos(List<CompanyDto> companyDtos);
}