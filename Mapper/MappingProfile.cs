using AutoMapper;
using SmartInvestor.Models;

namespace SmartInvestor.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<List<object>, Company>()
            .ConvertUsing(source => new Company
            {
                Cik = source[0].ToString()!.PadLeft(10,'0'),
                Name = source[1].ToString() ?? string.Empty,
                Ticker = source[2].ToString() ?? string.Empty
            });
    }
}