using TigerBeetle;

namespace RetailBank.Services;

public interface ITransactionService
{
    public Task Transfer(ulong fromAccount, ulong toAccount, UInt128 amountCents);
    public Task PaySalary(Account account);
    public Task<Transfer[]> GetTransfers(uint limit, ulong timestampMax);
    public Task<Transfer?> GetTransfer(UInt128 transferId);
}
