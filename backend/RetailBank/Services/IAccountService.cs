using RetailBank.Models;
using RetailBank.Models.Dtos;

namespace RetailBank.Services;

public interface IAccountService
{
    public Task<ulong> CreateTransactionalAccount(ulong salary);
    public Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountCode code);
    public Task<LedgerAccount?> GetAccount(ulong accountId);
    public Task<IEnumerable<TransferEvent>> GetAccountTransfers(ulong accountId, uint limit, ulong timestampMax, TransferSide side);
}
