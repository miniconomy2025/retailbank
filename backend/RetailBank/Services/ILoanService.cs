public interface ILoanService
{
    public Task<ulong> CreateLoanAccount(ulong loanAmount, ulong userAccountNumber);
}
