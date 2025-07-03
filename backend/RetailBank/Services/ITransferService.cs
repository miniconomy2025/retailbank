using RetailBank.Models.Dtos;

namespace RetailBank.Services;

public interface ITransferService
{
    public Task<TransferEvent?> GetTransfer(UInt128 id);
    public Task<IEnumerable<TransferEvent>> GetTransfers(uint limit, ulong timestampMax);

    public Task<UInt128> Transfer(ulong fromAccount, ulong toAccount, UInt128 amountCents);
    public Task PaySalary(ulong account);
}
