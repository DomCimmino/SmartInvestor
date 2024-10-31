using Microsoft.Extensions.DependencyInjection;

namespace SmartInvestor;

internal static class Program
{
    private static void Main(string[] args)
    {
        ConfigureServices().BuildServiceProvider().GetService<Application>()?.Init();
    }

    private static ServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton<Application, Application>();
        return services;
    }
}