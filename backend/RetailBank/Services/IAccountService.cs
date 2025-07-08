using RetailBank.Models.Ledger;

namespace RetailBank.Services;

public interface IAccountService
{
    public Task<UInt128> CreateTransactionalAccount(ulong salary);
    public Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountType? code, uint limit, ulong timestampMax);
    public Task<LedgerAccount?> GetAccount(UInt128 accountId);
    public Task<IEnumerable<LedgerTransfer>> GetAccountTransfers(UInt128 accountId, uint limit, ulong timestampMax, TransferSide? side);
    public Task<UInt128> GetTotalVolume();
}
