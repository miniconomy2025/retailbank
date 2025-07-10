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
        services.AddOptions<InterbankNotificationOptions>().BindConfiguration(InterbankNotificationOptions.Section);

        services.AddHttpClient<IInterbankClient, InterbankClient>().ConfigurePrimaryHttpMessageHandler(provider =>
        {
            var options = provider.GetRequiredService<IOptions<InterbankNotificationOptions>>();
            
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
            .AddSingleton<ITigerBeetleClientProvider, TigerBeetleClientProvider>()
            .AddSingleton<IIdempotencyCache, IdempotencyCache>()
            .AddSingleton<IAccountService, AccountService>()
            .AddSingleton<ILoanService, LoanService>()
            .AddSingleton<ITransferService, TransferService>()
            .AddSingleton<ISimulationControllerService, SimulationControllerService>();
    }
}
