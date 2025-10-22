using Microsoft.Extensions.Options;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;
using RetailBank.Repositories;
using RetailBank.Services;

namespace RetailBank.Tests.Integration;

public class IntegrationTestFixture : IDisposable
{
    public TigerBeetleClientProvider ClientProvider { get; }
    public ILedgerRepository LedgerRepository { get; }

    public IntegrationTestFixture()
    {
        var connectionOptions = Options.Create(new ConnectionStrings
        {
            TigerBeetle = "127.0.0.1:4000"
        });

        ClientProvider = new TigerBeetleClientProvider(connectionOptions);
        LedgerRepository = new LedgerRepository(ClientProvider);

        InitializeSystemAccounts().Wait();
    }

    private async Task InitializeSystemAccounts()
    {
        try
        {
            // create standard accounts if they don't exist (they shouldn't)
            await LedgerRepository.CreateAccount(new LedgerAccount(
                (ulong)LedgerAccountId.FeeIncome,
                LedgerAccountType.Internal,
                null
            ));

            await LedgerRepository.CreateAccount(new LedgerAccount(
                (ulong)LedgerAccountId.InterestIncome,
                LedgerAccountType.Internal,
                null
            ));

            await LedgerRepository.CreateAccount(new LedgerAccount(
                (ulong)LedgerAccountId.BadDebts,
                LedgerAccountType.Internal,
                null
            ));

            await LedgerRepository.CreateAccount(new LedgerAccount(
                (ulong)Bank.Retail,
                LedgerAccountType.Internal,
                null
            ));
        }
        catch
        {
            // meh, its okeh
        }
    }

    public void ResetClient()
    {
        ClientProvider.ResetClient();
    }

    public void Dispose()
    {
        // just dispose of the underlying connection object properly
        ClientProvider.Client.Dispose();
    }
}
