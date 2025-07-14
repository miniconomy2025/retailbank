using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using RetailBank.Exceptions;
using RetailBank.Extensions;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;
using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Services;

public class LoanService(LedgerRepository ledgerRepository, IOptions<LoanOptions> options)
{
    public async Task<UInt128> CreateLoanAccount(UInt128 debitAccountNumber, ulong loanAmount)
    {
        {
            var debitAccount = await ledgerRepository.GetAccount(debitAccountNumber) ?? throw new AccountNotFoundException(debitAccountNumber);

            if (debitAccount.AccountType != LedgerAccountType.Transactional)
                throw new InvalidAccountException(debitAccount.AccountType, LedgerAccountType.Transactional);
        }

        var accountNumber = GenerateLoanAccountNumber();

        var installment = CalculateInstallment(loanAmount, options.Value.AnnualInterestRatePercentage, options.Value.LoanPeriodMonths);

        await ledgerRepository.CreateAccount(
            new LedgerAccount(
                accountNumber,
                LedgerAccountType.Loan,
                new DebitOrder(debitAccountNumber, installment)
            )
        );

        await ledgerRepository.TransferLinked([
            new LedgerTransfer(ID.Create(), accountNumber, debitAccountNumber, loanAmount, 0, TransferType.Transfer),
            new LedgerTransfer(ID.Create(), (ulong)LedgerAccountId.LoanControl, (ulong)Bank.Retail, loanAmount, 0, TransferType.Transfer),
        ]);

        return accountNumber;
    }

    public async Task PayInstallment(UInt128 loanAccountId)
    {
        var loanAccount = await ledgerRepository.GetAccount(loanAccountId) ?? throw new AccountNotFoundException(loanAccountId);
        
        if (loanAccount.AccountType != LedgerAccountType.Loan)
            throw new InvalidAccountException(loanAccount.AccountType, LedgerAccountType.Loan);

        if (loanAccount.DebitOrder == null)
            return;
        
        var installment = loanAccount.DebitOrder.Amount;

        var loanDebitAccountId = loanAccount.DebitOrder.DebitAccountId;
        var loanDebitAccount = await ledgerRepository.GetAccount(loanDebitAccountId)
            ?? throw new AccountNotFoundException(loanDebitAccountId);

        var loanBalance = loanAccount.BalancePosted;
        var amountDue = Int128.Min(installment, loanBalance);

        var interestDue = (Int128)((decimal)loanAccount.BalancePosted * options.Value.AnnualInterestRatePercentage / 12.0m / 100.0m);

        if (-loanDebitAccount.BalancePosted < amountDue)
        {
            // they have missed their payment their account is struck down by the wrath of god himself
            await ledgerRepository.BalanceAndCloseCredit((ulong)LedgerAccountId.BadDebts, loanAccount.Id);
            return;
        }

        await ledgerRepository.TransferLinked([
            new LedgerTransfer(ID.Create(), (ulong)Bank.Retail, (ulong)LedgerAccountId.LoanControl, (UInt128)(amountDue - interestDue), 0, TransferType.Transfer),
            new LedgerTransfer(ID.Create(), loanDebitAccountId, loanAccount.Id, (UInt128)(amountDue - interestDue), 0, TransferType.Transfer),
            new LedgerTransfer(ID.Create(), loanDebitAccountId, (ulong)LedgerAccountId.InterestIncome, (UInt128)interestDue, 0, TransferType.Transfer)
        ]);
    }

    private static uint CalculateInstallment(ulong principal, decimal annualRatePercent, uint months)
    {
        var monthlyRate = annualRatePercent / 100.0m / 12.0m;
        var denominator = 1 - (1.0m + monthlyRate).Pow((int)-months);
        return (uint)Math.Ceiling(principal * monthlyRate / denominator);
    }

    // 13 digits starting with "1000"
    private static ulong GenerateLoanAccountNumber()
    {
        var number = 1_0000_0000_0000ul + (ulong)RandomNumberGenerator.GetInt32(10_0000_0000);
        return number;
    }
}
