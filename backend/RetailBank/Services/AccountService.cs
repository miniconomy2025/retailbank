using System.Security.Cryptography;
using RetailBank.Models.Dtos;
using RetailBank.Models.Ledger;
using RetailBank.Repositories;

namespace RetailBank.Services;

public class AccountService(ILedgerRepository ledgerRepository) : IAccountService
{
    public async Task<UInt128> CreateTransactionalAccount(ulong salary)
    {
        var id = GenerateTransactionalAccountNumber();

        await ledgerRepository.CreateAccount(
            id,
            LedgerAccountCode.Transactional,
            TigerBeetle.AccountFlags.DebitsMustNotExceedCredits,
            0,
            salary
        );

        return id;
    }

    public async Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountCode? code, uint limit, ulong timestampMax)
    {
        return await ledgerRepository.GetAccounts(code, limit, timestampMax);
    }

    public async Task<LedgerAccount?> GetAccount(UInt128 accountId)
    {
        return await ledgerRepository.GetAccount(accountId);
    }

    public async Task<IEnumerable<LedgerTransfer>> GetAccountTransfers(UInt128 accountId, uint limit, ulong timestampMax, TransferSide? side)
    {
        return await ledgerRepository.GetAccountTransfers(accountId, limit, timestampMax, side);
    }

    // 12 digits starting with "1000"
    private static ulong GenerateTransactionalAccountNumber()
    {
        var number = 1000_0000_0000ul + (ulong)RandomNumberGenerator.GetInt32(1_0000_0000);
        return number;
    }
}
