using RetailBank.Models;
using RetailBank.Models.Dtos;
using TigerBeetle;

namespace RetailBank.Repositories;

public interface ILedgerRepository
{
    public Task<Account?> GetAccount(UInt128 id);
    public Task<Transfer[]> GetAccountTransfers(UInt128 id, uint limit, ulong timestampMax, TransferSide side);
    public Task<IEnumerable<Account>> GetAccounts(LedgerAccountCode code);
    public Task<Transfer?> GetTransfer(UInt128 id);
    public Task<Transfer[]> GetTransfers(uint limit, ulong timestampMax);

    public Task CreateAccount(
        UInt128 accountId,
        LedgerAccountCode code,
        AccountFlags flags = AccountFlags.None,
        UInt128 userData128 = default,
        ulong userData64 = default,
        uint userData32 = default
    );

    public Task<UInt128> Transfer(LedgerTransfer simpleTransfer);
    public Task<IEnumerable<UInt128>> TransferLinked(IEnumerable<LedgerTransfer> simpleTransfers);

    public Task<UInt128> StartTransfer(LedgerTransfer simpleTransfer);
    public Task<UInt128> PostPendingTransfer(UInt128 pendingId, LedgerTransfer simpleTransfer);
    public Task<UInt128> VoidPendingTransfer(UInt128 pendingId, LedgerTransfer simpleTransfer);

    public Task<(UInt128, UInt128)> BalanceAndCloseCredit(UInt128 debitAccountId, UInt128 creditAccountId);
}
