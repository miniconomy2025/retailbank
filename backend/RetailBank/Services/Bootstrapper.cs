using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using RetailBank.Models.Options;

namespace RetailBank.Services;

public static class Bootstrapper
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddOptions<ConnectionStrings>().BindConfiguration(ConnectionStrings.Section);
        services.AddOptions<SimulationOptions>().BindConfiguration(SimulationOptions.Section);
        services.AddOptions<LoanOptions>().BindConfiguration(LoanOptions.Section);
        services.AddOptions<InterbankTransferOptions>().BindConfiguration(InterbankTransferOptions.Section);

        services.AddHttpClient<InterbankClient>().ConfigurePrimaryHttpMessageHandler(provider =>
        {
            var options = provider.GetRequiredService<IOptions<InterbankTransferOptions>>();
            
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ClientCertificates.Add(X509Certificate2.CreateFromPemFile(
                options.Value.ClientCertificatePath,
                options.Value.ClientCertificateKeyPath
            ));
            return handler;
        });

        return services
            .AddSingleton<TigerBeetleClientProvider>()
            .AddSingleton<IdempotencyCache>()
            .AddSingleton<AccountService>()
            .AddSingleton<LoanService>()
            .AddSingleton<TransferService>()
            .AddSingleton<SimulationControllerService>();
    }
}
