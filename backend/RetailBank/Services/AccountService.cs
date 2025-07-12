using System.Security.Cryptography;
using RetailBank.Models.Ledger;
using RetailBank.Repositories;

namespace RetailBank.Services;

public class AccountService(ILedgerRepository ledgerRepository)
{
    private const uint BatchMax = 8189;

    public async Task<UInt128> CreateTransactionalAccount(ulong salary)
    {
        var id = GenerateTransactionalAccountNumber();

        await ledgerRepository.CreateAccount(new LedgerAccount(id, LedgerAccountType.Transactional, new DebitOrder((ulong)Bank.Retail, salary)));

        return id;
    }

    public async Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountType? code, uint limit, ulong timestampMax)
    {
        return await ledgerRepository.GetAccounts(code, null, limit, timestampMax);
    }

    public async Task<LedgerAccount?> GetAccount(UInt128 accountId)
    {
        return await ledgerRepository.GetAccount(accountId);
    }

    public async Task<IEnumerable<LedgerTransfer>> GetAccountTransfers(UInt128 accountId, uint limit, ulong timestampMax, TransferSide? side)
    {
        return await ledgerRepository.GetAccountTransfers(accountId, limit, timestampMax, side);
    }

    public async Task<IEnumerable<LedgerAccount>> GetAccountLoans(UInt128 accountId)
    {
        return await ledgerRepository.GetAccounts(LedgerAccountType.Loan, accountId, BatchMax, 0);
    }

    public async Task<UInt128> GetTotalVolume()
    {
        UInt128 volume = 0;
        foreach (var variant in Enum.GetValues<LedgerAccountId>())
        {
            volume += (await GetAccount((ulong)variant))?.DebitsPosted ?? 0;
        }
        foreach (var variant in Enum.GetValues<Bank>())
        {
            volume += (await GetAccount((ulong)variant))?.DebitsPosted ?? 0;
        }

        return volume;
    }

    // 12 digits starting with "1000"
    private static ulong GenerateTransactionalAccountNumber()
    {
        var number = 1000_0000_0000ul + (ulong)RandomNumberGenerator.GetInt32(1_0000_0000);
        return number;
    }
}
