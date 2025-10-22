namespace RetailBank.Services;

public interface ILoanService
{
    Task<UInt128> CreateLoanAccount(UInt128 debitAccountNumber, ulong loanAmount);
    Task PayInstallment(UInt128 loanAccountId);
}
