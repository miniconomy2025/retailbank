using RetailBank.Models.Ledger;

namespace RetailBank.Repositories;

public interface ILedgerRepository
{
    public Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountType? code, UInt128? debitAccountId, uint limit, ulong timestampMax);
    public Task<LedgerAccount?> GetAccount(UInt128 id);
    public Task<IEnumerable<LedgerTransfer>> GetAccountTransfers(UInt128 id, uint limit, ulong timestampMax, TransferSide? side);
    public Task<LedgerTransfer?> GetTransfer(UInt128 id);
    public Task<IEnumerable<LedgerTransfer>> GetTransfers(uint limit, ulong timestampMax);

    public Task CreateAccount(LedgerAccount account);

    public Task<UInt128> Transfer(LedgerTransfer transfer);
    public Task<IEnumerable<UInt128>> TransferLinked(IEnumerable<LedgerTransfer> transfers);

    public Task<(UInt128, UInt128)> BalanceAndCloseCredit(UInt128 debitAccountId, UInt128 creditAccountId);
}
