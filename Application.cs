using System.Diagnostics;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor;

public class Application(ICompanyService companyService)
{
    public async Task Init()
    {
        await companyService.InitializeDatabaseAndDownloadData();
        await companyService.UploadCompanies();
        await companyService.UploadCompanyDtos();
        StartOptimization();
    }

    private static void StartOptimization()
    {
        var processInfo = new ProcessStartInfo
        {
            FileName =  FindPythonInPath(),
            Arguments = Constants.OptimizationScriptPath,
            UseShellExecute = true
        };

        Process.Start(processInfo);
    }

    private static string FindPythonInPath()
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in pathEnv.Split(';'))
        {
            if(!dir.Contains("Python39")) continue;
            
            var potentialPythonPath = Path.Combine(dir, "python.exe");

            if (File.Exists(potentialPythonPath))
            {
                return potentialPythonPath;
            }
        }
        return string.Empty;
    }
}