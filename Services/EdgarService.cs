using System.Diagnostics;
using System.IO.Compression;
using AutoMapper;
using Newtonsoft.Json;
using SmartInvestor.HttpManager;
using SmartInvestor.Models;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor.Services;

public class EdgarService(IHttpClientFactory clientFactory, IMapper mapper) : IEdgarService
{
    private Dictionary<string, string>? _fileCache;
    private string? _extractDataDirectory;

    public async Task<List<Company>> GetCompanies()
    {
        try
        {
            var response = await clientFactory.GetHttpClient().GetAsync("files/company_tickers_exchange.json")
                .ConfigureAwait(false);
            if (response is { IsSuccessStatusCode: false, Content: null }) return [];

            var contentString = await ReadResponseContentAsync(response).ConfigureAwait(false);
            if (string.IsNullOrEmpty(contentString)) return [];

            var companyData = JsonConvert.DeserializeObject<CompanyData>(contentString);
            return companyData?.Data?
                .Where(item => item.Count >= 4)
                .Select(mapper.Map<Company>)
                .ToList() ?? [];
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in GetCompanies: {e.Message}");
            throw;
        }
    }

    public async Task DownloadCompaniesFacts()
    {
        var baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Resources");
        _extractDataDirectory = Path.Combine(baseDirectory, "ExtractedData");
        var zipPath = Path.Combine(baseDirectory, "companyfacts.zip");

        Directory.CreateDirectory(baseDirectory);
        Directory.CreateDirectory(_extractDataDirectory);

        if (!Directory.EnumerateFileSystemEntries(_extractDataDirectory).Any())
        {
            var response = await clientFactory.GetHttpClient()
                .GetAsync("Archives/edgar/daily-index/xbrl/companyfacts.zip").ConfigureAwait(false);
            if (response is { IsSuccessStatusCode: true, Content: not null })
            {
                await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                await using var fileStream = File.Create(zipPath);
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            ZipFile.ExtractToDirectory(zipPath, _extractDataDirectory, true);
            InitializeFileCache();
        }
    }

    public async Task<CompanyFacts?> GetCompanyFacts(string cik)
    {
        if (_fileCache == null) InitializeFileCache();
        if (_fileCache == null || !_fileCache.TryGetValue($"CIK{cik}", out var filePath)) return null;
        var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
        return string.IsNullOrEmpty(content) ? null : JsonConvert.DeserializeObject<CompanyFacts>(content);
    }

    private void InitializeFileCache()
    {
        if (_extractDataDirectory == null) throw new ArgumentNullException(nameof(_extractDataDirectory));
        _fileCache = Directory.EnumerateFiles(_extractDataDirectory, "CIK*.json")
            .ToDictionary(Path.GetFileNameWithoutExtension, path => path);
    }

    private static async Task<string> ReadResponseContentAsync(HttpResponseMessage response)
    {
        if (!response.Content.Headers.ContentEncoding.Contains("gzip"))
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        await using var compressedStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var reader = new StreamReader(decompressionStream);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }
}