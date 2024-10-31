using SmartInvestor.Helpers;

namespace SmartInvestor;

public class Application
{
    public void Init()
    {
        var runningPath = System.AppDomain.CurrentDomain.BaseDirectory;
        var filePath = $"{Path.GetFullPath(Path.Combine(runningPath, @"..\..\..\"))}Resources\\constituents.csv";
        var companies = CsvParser.GetCompanies(filePath);
    }
}