using SmartInvestor.Models.DTOs;
using SmartInvestor.Services.Interfaces;
using SQLite;

namespace SmartInvestor.Services;

public class SqLiteService : ISqLiteService
{
    private const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.SharedCache;

    private static readonly string DatabasePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Resources", "smart_investor.db3");

    private SQLiteAsyncConnection? _database;

    public async Task InitDatabase()
    {
        if (_database is not null) return;

        _database = new SQLiteAsyncConnection(DatabasePath, Flags);
        try
        {
            if (_database.TableMappings.All(x => x.MappedType.Name != nameof(CompanyDto)))
                await _database.CreateTableAsync(typeof(CompanyDto)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in initialization database: {e.Message}");
            throw;
        }
    }

    public async Task<bool> InsertCompanies(List<CompanyDto> companyDto)
    {
        await _database.DeleteAllAsync<CompanyDto>();
        return await _database.InsertAsync(companyDto) == companyDto.Count;
    }

    public async Task<bool> IsCompanyUploaded(string cik)
    {
        return await _database.FindAsync<CompanyDto>(cik) != null;
    }
}