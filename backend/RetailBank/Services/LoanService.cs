using System.Numerics;
using System.Security.Cryptography;
using RetailBank.Models;
using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Services;

public class LoanService(ILedgerRepository ledgerRepository, IAccountService accountService) : ILoanService
{
    private const ushort InterestRate = 10;
    
    public async Task<ulong> CreateLoanAccount(ulong loanAmount, ulong userAccountNo)
    {
        var accountNumber = GenerateLoanAccountNumber();
        await ledgerRepository.CreateAccount(accountNumber, LedgerAccountCode.Loan, userData128:userAccountNo, userData64: CalculateInstallment(loanAmount, InterestRate, 60), accountFlags: AccountFlags.CreditsMustNotExceedDebits);
        await ledgerRepository.Transfer(ID.Create(), accountNumber, userAccountNo, loanAmount, transferFlags:TransferFlags.Linked);
        await ledgerRepository.Transfer(ID.Create(), (ushort)LedgerAccountId.LoanControl, (ushort)BankCode.Retail, loanAmount);
        return accountNumber;
    }

    public async Task ProcessInterest(Account loanAccount)
    {
        if (loanAccount.Code != (ushort)LedgerAccountCode.Loan)
        {
            throw new InvalidAccountException();
        }

        var balance = loanAccount.DebitsPosted - loanAccount.CreditsPosted;
        var interest = balance / 12 * InterestRate / 100;

        await ledgerRepository.Transfer(ID.Create(), (ulong)loanAccount.Id, (ushort)LedgerAccountId.InterestIncome, interest);
    }


    public async Task PayInstallment(Account loanAccount)
    {
        var installment = loanAccount.UserData64;
        var loanDebitAccountId = loanAccount.UserData128;
        var balance = (Int128)loanAccount.DebitsPosted - (Int128)loanAccount.CreditsPosted;
        var amountDue = Int128.Min(installment, balance);
        if ((await accountService.GetAccountBalance((ulong)loanDebitAccountId)) < (UInt128)balance)
        {
            // they have missed their payment their account is struck down by the wrath of god himself
            await ledgerRepository.Transfer(ID.Create(), (ushort)LedgerAccountId.BadDebts, (ulong)loanAccount.Id, (UInt128)amountDue, transferFlags: TransferFlags.Pending & TransferFlags.ClosingCredit);
            return;
        }
        await ledgerRepository.Transfer(ID.Create(), (ushort)LedgerAccountCode.Bank, (ulong)LedgerAccountId.LoanControl, (UInt128)amountDue, transferFlags:TransferFlags.Linked);
        await ledgerRepository.Transfer(ID.Create(), (ulong)loanDebitAccountId, (ulong)loanAccount.Id, (UInt128)amountDue);
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
