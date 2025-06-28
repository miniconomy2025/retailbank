namespace RetailBank.Services;

using System.Net;
using System.Security.Cryptography;
using RetailBank.Models;
using RetailBank.Repositories;
using TigerBeetle;

public class LoanService(ILedgerRepository ledgerRepository) : ILoanService
{
    private readonly ushort interestRate = 10;
    public async Task<ulong> CreateLoanAccount(ulong loanAmount, ulong userAccountNo)
    {
        var accountNumber = GenerateLoanAccountNumber();
        await ledgerRepository.CreateAccount(accountNumber, userData128:userAccountNo, userData64: CalculateInstallment(loanAmount, interestRate, 60), code: (ushort)AccountCode.Loan, accountFlags: AccountFlags.CreditsMustNotExceedDebits);
        await ledgerRepository.Transfer(ID.Create(), accountNumber, userAccountNo, loanAmount);
        await ledgerRepository.Transfer(ID.Create(), (ushort)LedgerAccountCode.LoanControlAccount, (ushort)BankCode.Retail, loanAmount);
        return accountNumber;
    }

    public async Task ComputeInterest(Account loanAccount)
    {
        if (loanAccount.Code != (ushort)AccountCode.Loan)
        {
            throw new InvalidAccountException();
        }

        var balance = loanAccount.DebitsPosted - loanAccount.CreditsPosted;
        var interest = balance / 12 * interestRate / 100;

        await ledgerRepository.Transfer(ID.Create(), (ulong)loanAccount.Id, (ushort)LedgerAccountCode.InterestIncomeAccount, interest);
    }


    public async Task PayInstallment(Account loanAccount)
    {
        var installment = loanAccount.UserData64;
        var balance = loanAccount.DebitsPosted - loanAccount.CreditsPosted;
    
        if (balance < installment)
        {
            await ledgerRepository.Transfer(ID.Create(), (ushort)AccountCode.Bank, (ulong)loanAccount.Id, balance);
        }
        else
        {
            await ledgerRepository.Transfer(ID.Create(), (ushort)AccountCode.Bank, (ulong)loanAccount.Id, installment);
        }
    }

    private static uint CalculateInstallment(ulong principal, float annualRatePercent, int months)
    {
        var monthlyRate = annualRatePercent / 100 / 12;
        var denominator = 1 - Math.Pow(1 + (double)monthlyRate, -months);
        return (uint)Math.Ceiling(principal * monthlyRate / denominator);
    }

    // 13 digits starting with "1000"
    private static ulong GenerateLoanAccountNumber()
    {
        var number = 1_0000_0000_0000ul + (ulong)RandomNumberGenerator.GetInt32(10_0000_0000);
        return number;
    }
}