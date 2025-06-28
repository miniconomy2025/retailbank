using TigerBeetle;

namespace RetailBank.Services;
public interface IAccountService
{
    public Task<Account?> GetAccount(ulong accountId);
    public Task<Transfer[]> GetAccountTransfers(ulong accountId);
    public Task<ulong> CreateSavingAccount(ulong salaryCents);
}