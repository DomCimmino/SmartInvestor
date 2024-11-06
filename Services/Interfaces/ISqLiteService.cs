using SmartInvestor.Models.DTOs;
using SQLite;

namespace SmartInvestor.Services.Interfaces;

public interface ISqLiteService
{
    Task InitDatabase();
    Task<bool> InsertCompanies(List<CompanyDto> companyDto);
    Task<bool> IsCompanyUploaded(string cik);
}