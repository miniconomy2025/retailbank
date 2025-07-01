using System.Security.Cryptography;
using RetailBank.Extensions;
using RetailBank.Models;
using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Services;

public class LoanService(ILedgerRepository ledgerRepository) : ILoanService
{
    private const ushort InterestRate = 10;
    
    public async Task<ulong> CreateLoanAccount(ulong debitAccountNumber, ulong loanAmount)
    {
        var accountNumber = GenerateLoanAccountNumber();

        await ledgerRepository.CreateAccount(
            accountNumber,
            LedgerAccountCode.Loan,
            AccountFlags.CreditsMustNotExceedDebits,
            debitAccountNumber,
            CalculateInstallment(loanAmount, InterestRate, 60)
        );

        await ledgerRepository.TransferLinked([
            new LedgerTransfer(accountNumber, debitAccountNumber, loanAmount),
            new LedgerTransfer((ulong)LedgerAccountId.LoanControl, (ulong)BankId.Retail, loanAmount),
        ]);

        return accountNumber;
    }

    public async Task ChargeInterest(ulong loanAccountId)
    {
        var loanAccount = await ledgerRepository.GetAccount(loanAccountId)
            ?? throw new AccountNotFoundException(loanAccountId);

        if (loanAccount.Code != (ushort)LedgerAccountCode.Loan)
            throw new InvalidAccountException();

        var balance = loanAccount.BalancePosted();
        var interest = balance / 12 * InterestRate / 100;

        await ledgerRepository.Transfer(new LedgerTransfer(
            loanAccount.Id,
            (ulong)LedgerAccountId.InterestIncome,
            (UInt128)interest
        ));
    }

    public async Task PayInstallment(ulong loanAccountId)
    {
        var loanAccount = await ledgerRepository.GetAccount(loanAccountId)
            ?? throw new AccountNotFoundException(loanAccountId);
        
        if (loanAccount.Code != (ushort)LedgerAccountCode.Loan)
            throw new InvalidAccountException();
        
        var installment = loanAccount.UserData64;

        var loanDebitAccountId = loanAccount.UserData128;
        var loanDebitAccount = await ledgerRepository.GetAccount(loanDebitAccountId)
            ?? throw new AccountNotFoundException(loanDebitAccountId);

        var loanBalance = loanAccount.BalancePosted();
        var amountDue = Int128.Min(installment, loanBalance);

        if (-loanDebitAccount.BalancePosted() < amountDue)
        {
            // they have missed their payment their account is struck down by the wrath of god himself
            await ledgerRepository.BalanceAndCloseCredit((ulong)LedgerAccountId.BadDebts, loanAccount.Id);
            return;
        }

        await ledgerRepository.TransferLinked([
            new LedgerTransfer((ulong)BankId.Retail, (ulong)LedgerAccountId.LoanControl, (UInt128)amountDue),
            new LedgerTransfer(loanDebitAccountId, loanAccount.Id, (UInt128)amountDue),
        ]);
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
