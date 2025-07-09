namespace RetailBank.Services;

public interface ILoanService
{
    public Task<UInt128> CreateLoanAccount(UInt128 debitAccountNumber, ulong loanAmount);
    public Task PayInstallment(UInt128 loanAccountId);
}
