using AutoMapper;
using SmartInvestor.Models;
using SmartInvestor.Models.DTOs;

namespace SmartInvestor.Mapper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateCompanyMapProfile();
        CreateCompanyDtoMapProfile();
    }

    private void CreateCompanyMapProfile()
    {
        CreateMap<List<object>, Company>()
            .ConvertUsing(source => new Company
            {
                Cik = source[0].ToString()!.PadLeft(10, '0'),
                Name = source[1].ToString() ?? string.Empty,
                Ticker = source[2].ToString() ?? string.Empty
            });
    }

    private void CreateCompanyDtoMapProfile()
    {
        CreateMap<Company, CompanyDto>()
            .ForMember(dest => dest.Cik, opt => opt.MapFrom(src => src.Cik))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Ticker, opt => opt.MapFrom(src => src.Ticker))
            .ReverseMap();
    }
}