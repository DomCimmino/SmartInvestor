using AutoMapper;
using SmartInvestor.Helpers;
using SmartInvestor.Models;
using SmartInvestor.Models.DTOs;

namespace SmartInvestor.Mapper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateCompanyMap();
        CreateDtoMap();
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

    private void CreateDtoMap()
    {
        // To-Do complete mapping
        CreateMap<CompanyFacts, CompanyDto>()
            .ForMember(dest => dest.Cik,
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Cik) ? src.Cik.PadLeft(10, '0') : string.Empty))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.EntityName));
    }
}