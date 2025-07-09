namespace RetailBank.Services;

public interface ILoanService
{
    Task<UInt128> CreateLoanAccount(UInt128 debitAccountNumber, ulong loanAmount);
    Task ChargeInterest(UInt128 loanAccountId);
    Task PayInstallment(UInt128 loanAccountId);
}
