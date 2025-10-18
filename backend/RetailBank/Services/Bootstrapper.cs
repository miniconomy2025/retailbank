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
        services.AddOptions<TransferOptions>().BindConfiguration(TransferOptions.Section);

        services.AddHttpClient<InterbankClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "RetailBank/1.0");
            client.DefaultRequestHeaders.Add("Client-Id", "retail-bank");
        });

        // services.AddHttpClient<InterbankClient>(client =>
        // {
        //     client.Timeout = TimeSpan.FromSeconds(30);
        //     client.DefaultRequestHeaders.Add("User-Agent", "RetailBank/1.0");
        // }).ConfigurePrimaryHttpMessageHandler(provider =>
        // {
        //     var options = provider.GetRequiredService<IOptions<InterbankTransferOptions>>();
        //     var handler = new HttpClientHandler
        //     {
        //         ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        //         ClientCertificateOptions = ClientCertificateOption.Manual
        //     };
        //     handler.ClientCertificates.Add(new X509Certificate2(
        //         options.Value.ClientCertificatePath
        //     ));
        //     return handler;
        // });

        return services
            .AddSingleton<TigerBeetleClientProvider>()
            .AddSingleton<IdempotencyCache>()
            .AddSingleton<AccountService>()
            .AddSingleton<LoanService>()
            .AddSingleton<TransferService>()
            .AddSingleton<SimulationControllerService>();
    }
}
