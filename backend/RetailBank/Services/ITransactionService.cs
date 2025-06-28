using TigerBeetle;

namespace RetailBank.Services;

public interface ITransactionService
{
    public Task<Account?> GetAccount(ulong accountId);
    public Task<Transfer[]> GetAccountTransfers(ulong accountId);
    public Task<ulong> CreateSavingAccount(ulong salaryCents);
    public Task<ulong> CreateLoanAccount();
    public Task Transfer(ulong fromAccount, ulong toAccount, UInt128 amountCents);
}
