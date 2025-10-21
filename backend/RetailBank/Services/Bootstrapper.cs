using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using RetailBank.Models.Options;
using RetailBank.Repositories;

namespace RetailBank.Services;

public static class Bootstrapper
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddOptions<ConnectionStrings>().BindConfiguration(ConnectionStrings.Section);
        services.AddOptions<SimulationOptions>().BindConfiguration(SimulationOptions.Section);
        services.AddOptions<LoanOptions>().BindConfiguration(LoanOptions.Section);
        services.AddOptions<InterbankTransferOptions>().BindConfiguration(InterbankTransferOptions.Section);
        services.AddOptions<TransferOptions>().BindConfiguration(TransferOptions.Section);

        services.AddHttpClient<IInterbankClient, InterbankClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "RetailBank/1.0");
            client.DefaultRequestHeaders.Add("Client-Id", "retail-bank");
        });

        return services
            .AddSingleton<TigerBeetleClientProvider>()
            .AddSingleton<ILedgerRepository, LedgerRepository>()
            .AddSingleton<IdempotencyCache>()
            .AddSingleton<AccountService>()
            .AddSingleton<ILoanService, LoanService>()
            .AddSingleton<TransferService>()
            .AddSingleton<SimulationControllerService>();
    }
}
