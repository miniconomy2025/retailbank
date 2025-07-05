using RetailBank.Models.Ledger;
using TigerBeetle;

namespace RetailBank.Repositories;

public interface ILedgerRepository
{
    public Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountCode? code, uint limit, ulong timestampMax);
    public Task<LedgerAccount?> GetAccount(UInt128 id);
    public Task<IEnumerable<LedgerTransfer>> GetAccountTransfers(UInt128 id, uint limit, ulong timestampMax, TransferSide? side);
    public Task<LedgerTransfer?> GetTransfer(UInt128 id);
    public Task<IEnumerable<LedgerTransfer>> GetTransfers(uint limit, ulong timestampMax);

    public Task CreateAccount(
        UInt128 accountId,
        LedgerAccountCode code,
        AccountFlags flags = AccountFlags.None,
        UInt128 userData128 = default,
        ulong userData64 = default,
        uint userData32 = default
    );

    public Task<UInt128> Transfer(LedgerTransfer transfer);
    public Task<IEnumerable<UInt128>> TransferLinked(IEnumerable<LedgerTransfer> transfers);

    public Task<(UInt128, UInt128)> BalanceAndCloseCredit(UInt128 debitAccountId, UInt128 creditAccountId);
}
