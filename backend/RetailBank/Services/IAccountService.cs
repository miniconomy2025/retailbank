using RetailBank.Models.Ledger;

namespace RetailBank.Services;

public interface IAccountService
{
    Task<UInt128> CreateTransactionalAccount(ulong salary);
    Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountType? code, uint limit, ulong timestampMax);
    Task<LedgerAccount?> GetAccount(UInt128 accountId);
    Task<IEnumerable<LedgerTransfer>> GetAccountTransfers(UInt128 accountId, uint limit, ulong timestampMax, TransferSide? side);
    Task<IEnumerable<LedgerAccount>> GetAccountLoans(UInt128 accountId);
    Task<UInt128> GetTotalVolume();
}
