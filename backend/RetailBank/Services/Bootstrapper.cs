using RetailBank.Models.Options;

namespace RetailBank.Services;

public static class Bootstrapper
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddOptions<ConnectionStrings>().BindConfiguration(ConnectionStrings.Section);
        services.AddOptions<SimulationOptions>().BindConfiguration(SimulationOptions.Section);

        return services
            .AddSingleton<ITigerBeetleClientProvider, TigerBeetleClientProvider>()
            .AddSingleton<IAccountService, AccountService>()
            .AddSingleton<ILoanService, LoanService>()
            .AddSingleton<ITransferService, TransferService>()
            .AddSingleton<ISimulationControllerService, SimulationControllerService>();
    }
}
