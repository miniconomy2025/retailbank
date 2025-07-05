using RetailBank.Models.Ledger;

namespace RetailBank.Services;

public interface ITransferService
{
    public Task<LedgerTransfer?> GetTransfer(UInt128 id);
    public Task<IEnumerable<LedgerTransfer>> GetTransfers(uint limit, ulong timestampMax);

    public Task<UInt128> Transfer(UInt128 fromAccount, UInt128 toAccount, UInt128 amountCents, ulong? reference);
    public Task PaySalary(UInt128 account);
}
