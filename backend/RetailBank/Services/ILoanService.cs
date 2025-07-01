namespace RetailBank.Services;

public interface ILoanService
{
    public Task<ulong> CreateLoanAccount(ulong debitAccountNumber, ulong loanAmount);
    public Task ChargeInterest(ulong loanAccountId);
    public Task PayInstallment(ulong loanAccountId);
}
