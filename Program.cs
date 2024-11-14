using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using SmartInvestor.HttpManager;
using SmartInvestor.Mapper;
using SmartInvestor.Repositories;
using SmartInvestor.Repositories.Interfaces;
using SmartInvestor.Services;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var application = ConfigureServices().BuildServiceProvider().GetService<Application>();
        if (application == null) return;
        await application.Init();
    }

    private static ServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        var config = new MapperConfiguration(cfg => { cfg.AddProfile<MappingProfiles>(); });
        var mapper = config.CreateMapper();
        services
            .AddSingleton<Application, Application>()
            .AddSingleton(mapper)
            .AddSingleton<IHttpClientFactory, HttpClientFactory>()
            .AddSingleton<ICompanyRepository, CompanyRepository>()
            .AddSingleton<IEdgarRepository, EdgarRepository>()
            .AddSingleton<ICompanyService, CompanyService>();
        return services;
    }
}