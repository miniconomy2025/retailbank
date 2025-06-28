namespace RetailBank.Services;

using System.Security.Cryptography;
using RetailBank.Models;
using RetailBank.Repositories;
using TigerBeetle;

public class LoanService(Client tbClient, ILedgerRepository ledgerRepository) : ILoanService
{
    private readonly ushort interestRate = 69;
    public async Task<ulong> CreateLoanAccount(ulong loanAmount, ulong userAccountNo)
    {
        var accountNumber = GenerateLoanAccountNumber();
        await ledgerRepository.CreateAccount(accountNumber, userData64: CalculateInstallment(loanAmount, (decimal)(interestRate / 100), 60), code: (ushort)AccountCode.Loan, accountFlags: AccountFlags.CreditsMustNotExceedDebits);

        await ledgerRepository.Transfer(ID.Create(), accountNumber, userAccountNo, loanAmount);
        await ledgerRepository.Transfer(ID.Create(), (ushort)LedgerAccountCode.LoanControlAccount, (ushort)BankCode.Retail, loanAmount);

        return accountNumber;
    }

    public async Task ComputeInterest(ulong loanAccountNo)
    {
        var loanAccount = await tbClient.LookupAccountAsync(loanAccountNo) ?? throw new AccountNotFoundException(loanAccountNo);
        if (loanAccount.Code != (ushort)AccountCode.Loan)
        {
            throw new InvalidAccountException();
        }

        var balance = loanAccount.DebitsPosted - loanAccount.CreditsPosted;
        var interest = balance / 12 * interestRate / 100;

        await ledgerRepository.Transfer(ID.Create(), (ushort)loanAccountNo, (ushort)LedgerAccountCode.InterestIncomeAccount, interest);
    }


    public async Task PayInstallment(ulong loanAccountNo)
    {
        var loanAccount = await tbClient.LookupAccountAsync(loanAccountNo) ?? throw new AccountNotFoundException(loanAccountNo);
        var installment = loanAccount.UserData64;

        await ledgerRepository.Transfer(ID.Create(), (ushort)AccountCode.Bank, (ushort)loanAccountNo, installment);
    }

    private static uint CalculateInstallment(decimal principal, decimal annualRatePercent, int months)
    {
        var monthlyRate = annualRatePercent / 100 / 12;
        return (uint)Math.Ceiling(principal * monthlyRate / (1 - (decimal)Math.Pow(1 + (double)monthlyRate, -months)));
    }

    // 13 digits starting with "1000"
    private static ulong GenerateLoanAccountNumber()
    {
        var number = 1_0000_0000_0000ul + (ulong)RandomNumberGenerator.GetInt32(10_0000_0000);
        return number;
    }
}