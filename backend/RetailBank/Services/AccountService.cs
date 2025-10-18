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

    public async Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountType? code, uint limit, ulong cursorMax)
    {
        return await ledgerRepository.GetAccounts(code, null, limit, cursorMax);
    }

    public async Task<LedgerAccount?> GetAccount(UInt128 accountId)
    {
        return await ledgerRepository.GetAccount(accountId);
    }

    public async Task<IEnumerable<LedgerTransfer>> GetAccountTransfers(UInt128 accountId, uint limit, ulong cursorMax, ulong? reference, TransferSide? side)
    {
        return await ledgerRepository.GetAccountTransfers(accountId, limit, cursorMax, reference, side);
    }

    public async Task<IEnumerable<LedgerAccount>> GetAccountLoans(UInt128 accountId)
    {
        return await ledgerRepository.GetAccounts(LedgerAccountType.Loan, accountId, BatchMax, 0);
    }

    // 12 digits starting with "1000"
    private static ulong GenerateTransactionalAccountNumber()
    {
        var number = 1000_0000_0000ul + (ulong)RandomNumberGenerator.GetInt32(1_0000_0000);
        return number;
    }
}
