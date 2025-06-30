using RetailBank.Models;
using RetailBank.Models.Dtos;
using TigerBeetle;

namespace RetailBank.Services;

public interface IAccountService
{
    public Task<Account?> GetAccount(ulong accountId);
    public Task<Transfer[]> GetAccountTransfers(ulong accountId, uint limit, ulong timestampMax, TransferSide side);
    public Task<ulong> CreateTransactionalAccount(ulong salaryCents);
    public Task<List<Account>> GetAllAccountsByCodeAsync(LedgerAccountCode code);
}