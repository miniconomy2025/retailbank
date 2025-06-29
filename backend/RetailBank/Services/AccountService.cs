using System.Security.Cryptography;
using RetailBank.Models;
using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Services;

public class AccountService(ITigerBeetleClientProvider tbClientProvider, ILedgerRepository ledgerRepository) : IAccountService
{
    public async Task<ulong> CreateSavingAccount(ulong salaryCents)
    {
        var accountNumber = GenerateSavingsAccountNumber();
        await ledgerRepository.CreateAccount(accountNumber, LedgerAccountCode.Transactional, userData64: salaryCents, accountFlags: AccountFlags.DebitsMustNotExceedCredits);
        return accountNumber;
    }

    public async Task<Account?> GetAccount(ulong accountId)
    {
        return await tbClientProvider.Client.LookupAccountAsync(accountId);
    }

    public async Task<List<Account>> GetAllAccountsByCodeAsync(LedgerAccountCode code)
    {
        var allAccounts = new List<Account>();
        ulong nextTimestamp = 0;
        const uint batchSize = 1000;

        while (true)
        {
            var filter = new QueryFilter
            {
                Limit = batchSize,
                TimestampMin = nextTimestamp,
                Code = (ushort)code
            };

            var accounts = await tbClientProvider.Client.QueryAccountsAsync(filter);

            if (accounts.Length == 0)
                break;

            allAccounts.AddRange(accounts);
            nextTimestamp = accounts.Max(a => a.Timestamp) + 1;

            if (accounts.Length < batchSize)
                break;
        }

        return allAccounts;
    }

    public async Task<Transfer[]> GetAccountTransfers(ulong accountId, uint limit, ulong timestampMax)
    {
        var filter = new AccountFilter();
        filter.AccountId = accountId;
        filter.Limit = limit;
        filter.TimestampMax = timestampMax;
        filter.Flags = AccountFilterFlags.Reversed | AccountFilterFlags.Debits | AccountFilterFlags.Credits;
        
        return await tbClientProvider.Client.GetAccountTransfersAsync(filter);
    }

    public async Task<UInt128?> GetAccountBalance(ulong accountId)
    {
        var account = await GetAccount(accountId);
        return account?.CreditsPosted - account?.DebitsPosted;
    }

    // 12 digits starting with "1000"
    private static ulong GenerateSavingsAccountNumber()
    {
        var number = 1000_0000_0000ul + (ulong)RandomNumberGenerator.GetInt32(1_0000_0000);
        return number;
    }
}