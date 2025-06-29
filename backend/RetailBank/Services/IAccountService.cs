using RetailBank.Models;
using TigerBeetle;

namespace RetailBank.Services;

public interface IAccountService
{
    public Task<Account?> GetAccount(ulong accountId);
    public Task<UInt128?> GetAccountBalance(ulong accountId);
    public Task<Transfer[]> GetAccountTransfers(ulong accountId, uint limit, ulong timestampMax);
    public Task<ulong> CreateSavingAccount(ulong salaryCents);
    public Task<List<Account>> GetAllAccountsByCodeAsync(LedgerAccountCode code);
}