using SmartInvestor.Models;
using SmartInvestor.Models.DTOs;
using SmartInvestor.Repositories.Interfaces;
using SQLite;

namespace SmartInvestor.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private SQLiteAsyncConnection? _database;

    public async Task InitDatabase()
    {
        if (_database is not null) return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        try
        {
            if (_database.TableMappings.All(x => x.MappedType.Name != nameof(CompanyDto)) &&
                _database.TableMappings.All(x => x.MappedType.Name != nameof(Company)))
            {
                await _database.CreateTableAsync(typeof(CompanyDto)).ConfigureAwait(false);
                await _database.CreateTableAsync(typeof(Company)).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in initialization database: {e.Message}");
            throw;
        }
    }

    public async Task<List<Company>> GetCompanies()
    {
        if (_database is null) return [];
        return await _database.Table<Company>().ToListAsync();
    }

    public async Task<bool> HasCompanies()
    {
        if (_database is null) return false;
        return await _database.Table<Company>().CountAsync() > 0;
    }

    public async Task<bool> HasCompanyDtos()
    {
        if (_database is null) return false;
        return await _database.Table<CompanyDto>().CountAsync() > 0;
    }

    public async Task InsertCompanies(List<Company> companies)
    {
        if (_database is null) return;
        await _database.DeleteAllAsync<Company>();
        await _database.InsertAllAsync(companies);
    }

    public async Task InsertCompanyDtos(List<CompanyDto> companyDtos)
    {
        if (_database is null) return;
        await _database.DeleteAllAsync<CompanyDto>();
        await _database.InsertAllAsync(companyDtos);
    }
}