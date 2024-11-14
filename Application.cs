using SmartInvestor.Services.Interfaces;

namespace SmartInvestor;

public class Application(ICompanyService companyService)
{
    public async Task Init()
    {
        await companyService.InitializeDatabaseAndDownloadData();
        await companyService.UploadCompanies();
        await companyService.UploadCompanyDtos();
    }
}