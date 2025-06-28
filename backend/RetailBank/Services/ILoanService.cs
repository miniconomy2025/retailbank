using TigerBeetle;

namespace RetailBank.Services;

public interface ILoanService
{
    public Task<ulong> CreateLoanAccount(ulong loanAmount, ulong userAccountNo);
    public Task PayInstallment(Account loanAccount);
    public Task ComputeInterest(Account loanAccount);
}
