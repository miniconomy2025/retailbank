using RetailBank.Models.Ledger;

namespace RetailBank.Services;

public interface ITransferService
{
    Task<LedgerTransfer?> GetTransfer(UInt128 id);
    Task<IEnumerable<LedgerTransfer>> GetTransfers(uint limit, ulong timestampMax);
    Task<UInt128> Transfer(UInt128 fromAccount, UInt128 toAccount, UInt128 amountCents, ulong reference);
    Task PaySalary(UInt128 account);
}
