using AutoMapper;
using SmartInvestor.Models;
using SmartInvestor.Models.DTOs;

namespace SmartInvestor.Mapper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateCompanyMap();
    }

    private void CreateCompanyMap()
    {
        CreateMap<List<object>, Company>()
            .ConvertUsing(source => new Company
            {
                Cik = source[0].ToString()!.PadLeft(10, '0'),
                Name = source[1].ToString() ?? string.Empty,
                Ticker = source[2].ToString() ?? string.Empty
            });
    }
}