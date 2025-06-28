using TigerBeetle;
namespace RetailBank.Services;

public interface ITransactionService
{
    public Task<UInt128> CreateAccount(UInt64 SalaryCents);
    public Task InternalTransfer(UInt128 fromAccount, UInt128 toAccount, UInt128 amount);
    public Task ExternalTransfer(UInt128 fromAccount, UInt128 ExternalAccountId, UInt128 amount);

}