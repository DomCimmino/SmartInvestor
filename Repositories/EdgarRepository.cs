using System.IO.Compression;
using AutoMapper;
using Newtonsoft.Json;
using SmartInvestor.HttpManager;
using SmartInvestor.Models;
using SmartInvestor.Repositories.Interfaces;

namespace SmartInvestor.Repositories;

public class EdgarRepository(IHttpClientFactory clientFactory, IMapper mapper) : IEdgarRepository
{
    private string? _extractDataDirectory;
    private Dictionary<string, string>? _fileCache;

    public async Task<List<Company>> GetCompanies()
    {
        try
        {
            if (_fileCache == null) InitializeFileCache();

            var response = await clientFactory.GetHttpClient().GetAsync(Constants.CompaniesApi)
                .ConfigureAwait(false);
            if (response is { IsSuccessStatusCode: false, Content: null }) return [];

            var contentString = await ReadResponseContentAsync(response).ConfigureAwait(false);
            if (string.IsNullOrEmpty(contentString)) return [];

            var companies = JsonConvert.DeserializeObject<CompanyData>(contentString);
            return companies?.Data?
                .Where(item =>
                    item.Count >= 4 && (bool)_fileCache?.ContainsKey($"{item[0]}".PadLeft(10, '0').Insert(0, "CIK")))
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

        if (!Directory.Exists(baseDirectory)) Directory.CreateDirectory(baseDirectory);
        if (!Directory.Exists(_extractDataDirectory)) Directory.CreateDirectory(_extractDataDirectory);

        if (!Directory.EnumerateFileSystemEntries(_extractDataDirectory).Any())
        {
            Console.WriteLine("Start download companies facts");
            var response = await clientFactory.GetHttpClient()
                .GetAsync(Constants.CompanyFactsApi).ConfigureAwait(false);
            if (response is { IsSuccessStatusCode: true, Content: not null })
            {
                Console.WriteLine("End download companies facts");
                await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                await using var fileStream = File.Create(zipPath);
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }
            
            Console.WriteLine("Start unzip companies facts");
            ZipFile.ExtractToDirectory(zipPath, _extractDataDirectory, true);
            Console.WriteLine("End unzip companies facts");
            InitializeFileCache();
        }
    }

    public async Task<CompanyFacts?> GetCompanyFacts(string cik)
    {
        if (_fileCache == null || _fileCache.Count == 0) InitializeFileCache();
        if (_fileCache == null || !_fileCache.TryGetValue($"CIK{cik}", out var filePath)) return null;
        var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
        return string.IsNullOrEmpty(content) ? null : JsonConvert.DeserializeObject<CompanyFacts>(content);
    }

    private void InitializeFileCache()
    {
        if (_extractDataDirectory == null) throw new ArgumentNullException(nameof(_extractDataDirectory));
        _fileCache = Directory.EnumerateFiles(_extractDataDirectory, "CIK*.json")
            .ToDictionary(Path.GetFileNameWithoutExtension , path => path);
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