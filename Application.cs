using SmartInvestor.Helpers;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor;

public class Application(IEdgarService edgarService)
{
    public async Task Init()
    {
        var runningPath = AppDomain.CurrentDomain.BaseDirectory;
        var filePath = $"{Path.GetFullPath(Path.Combine(runningPath, @"..\..\..\"))}Resources\\constituents.csv";
        var companies = CsvParser.GetCompanies(filePath);
        var companyFact =
            await edgarService.GetCompanyFacts(companies.FirstOrDefault(c => c.Name?.StartsWith('M') == true)?.Cik ??
                                               string.Empty);
    }
}