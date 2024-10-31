using Microsoft.Extensions.DependencyInjection;
using SmartInvestor.HttpManager;
using SmartInvestor.Services;
using SmartInvestor.Services.Interfaces;

namespace SmartInvestor;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var application = ConfigureServices().BuildServiceProvider().GetService<Application>();
        if(application == null) return;
        await application.Init();
    }

    private static ServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton<Application, Application>()
            .AddSingleton<IHttpClientFactory, HttpClientFactory>()
            .AddSingleton<IEdgarService, EdgarService>();
        return services;
    }
}