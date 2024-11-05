using System.IO.Compression;
using System.Net;
using AutoMapper;
using Newtonsoft.Json;
using SmartInvestor.HttpManager;
using SmartInvestor.Models;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor.Services;

public class EdgarService(IHttpClientFactory clientFactory, IMapper mapper) : IEdgarService
{
    private string? _extractDataDirectory;

    public async Task<List<Company>> GetCompanies()
    {
        try
        {
            var companiesResponse =
                await clientFactory.GetHttpClient().GetAsync("files/company_tickers_exchange.json");
            if (companiesResponse is not { IsSuccessStatusCode: true, Content: not null }) return [];
            var contentString = await ReadResponseContentAsync(companiesResponse);
            if (string.IsNullOrEmpty(contentString)) return [];
            var companyData = JsonConvert.DeserializeObject<CompanyData>(contentString);
            return (from item in companyData?.Data ?? [] where item.Count >= 4 select mapper.Map<Company>(item))
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task DownloadCompaniesFacts()
    {
        var runningPath = AppDomain.CurrentDomain.BaseDirectory;
        var zipPath = $"{Path.GetFullPath(Path.Combine(runningPath, @"..\..\..\"))}Resources\\companyfacts.zip";
        var resourcesDirectory = $"{Path.GetFullPath(Path.Combine(runningPath, @"..\..\..\"))}Resources";
        _extractDataDirectory = $"{Path.GetFullPath(Path.Combine(runningPath, @"..\..\..\"))}Resources\\ExtractedData";

        if (!Directory.Exists(resourcesDirectory)) Directory.CreateDirectory(resourcesDirectory);
        if (!Directory.Exists(_extractDataDirectory)) Directory.CreateDirectory(_extractDataDirectory);

        if (Directory.GetFiles(_extractDataDirectory).Length == 0)
        {
            var companiesFactsResponse = await clientFactory.GetHttpClient()
                .GetAsync("Archives/edgar/daily-index/xbrl/companyfacts.zip");
            if (companiesFactsResponse is { IsSuccessStatusCode: true, Content: not null })
            {
                await using var stream = await companiesFactsResponse.Content.ReadAsStreamAsync();
                await using var fileStream = File.Create(zipPath);
                await stream.CopyToAsync(fileStream);
            }

            ZipFile.ExtractToDirectory(zipPath, _extractDataDirectory);
        }
    }

    public async Task<CompanyFacts?> GetCompanyFacts(string cik)
    {
        if (!Directory.Exists(_extractDataDirectory)) return null;
        var jsonFiles = Directory.GetFiles(_extractDataDirectory, $"CIK{cik}.json");
        if (jsonFiles.Length == 0) return null;
        var jsonContent = await File.ReadAllTextAsync(jsonFiles.First());
        return string.IsNullOrEmpty(jsonContent) ? null : JsonConvert.DeserializeObject<CompanyFacts>(jsonContent);
    }


    private static async Task<string> ReadResponseContentAsync(HttpResponseMessage response)
    {
        if (!response.Content.Headers.ContentEncoding.Contains("gzip"))
            return await response.Content.ReadAsStringAsync();
        await using var compressedStream = await response.Content.ReadAsStreamAsync();
        await using var decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var reader = new StreamReader(decompressionStream);
        return await reader.ReadToEndAsync();
    }
}