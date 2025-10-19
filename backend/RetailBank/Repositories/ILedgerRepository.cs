using RetailBank.Models.Ledger;

namespace RetailBank.Repositories;

public interface ILedgerRepository
{
    public const uint LedgerId = 1;
    public const ushort TransferCode = 1;

    Task CreateAccount(LedgerAccount account);
    Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountType? code, UInt128? debitAccountId, uint limit, ulong cursorMax);
    Task<LedgerAccount?> GetAccount(UInt128 accountId);
    Task<IEnumerable<LedgerTransfer>> GetAccountTransfers(UInt128 id, uint limit, ulong cursorMax, ulong? reference, TransferSide? side);
    Task<IEnumerable<LedgerTransfer>> GetTransfers(uint limit, ulong cursorMax, ulong? reference);
    Task<LedgerTransfer?> GetTransfer(UInt128 id);
    Task<UInt128> Transfer(LedgerTransfer ledgerTransfer);
    Task<IEnumerable<UInt128>> TransferLinked(IEnumerable<LedgerTransfer> ledgerTransfers);
    Task<(UInt128, UInt128)> BalanceAndCloseCredit(UInt128 debitAccountId, UInt128 creditAccountId);
    Task InitialiseInternalAccounts();
}
