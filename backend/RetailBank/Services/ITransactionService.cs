namespace RetailBank.Services;

public interface ITransactionService
{
    public Task Transfer(ulong fromAccount, ulong toAccount, UInt128 amountCents);
    public Task PaySalary(Account account);
}
