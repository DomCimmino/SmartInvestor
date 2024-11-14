namespace SmartInvestor.Services.Interfaces;

public interface ICompanyService
{
    Task InitializeDatabaseAndDownloadData();
    Task UploadCompanies();
    Task UploadCompanyDtos();
}