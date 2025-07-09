using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using RetailBank.Exceptions;
using RetailBank.Models.Ledger;
using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Services;

public class LoanService(ILedgerRepository ledgerRepository) : ILoanService
{
    private const ushort InterestRate = 10;
    private const ushort LoanPeriod = 60;

    public async Task<UInt128> CreateLoanAccount(UInt128 debitAccountNumber, ulong loanAmount)
    {
        {
            var debitAccount = await ledgerRepository.GetAccount(debitAccountNumber) ?? throw new AccountNotFoundException(debitAccountNumber);

            if (debitAccount.AccountType != LedgerAccountType.Transactional)
                throw new InvalidAccountException(debitAccount.AccountType, LedgerAccountType.Transactional);
        }

        var accountNumber = GenerateLoanAccountNumber();

        var installment = CalculateInstallment(loanAmount, InterestRate, LoanPeriod);

        await ledgerRepository.CreateAccount(
            new LedgerAccount(
                accountNumber,
                LedgerAccountType.Loan,
                new DebitOrder(debitAccountNumber, installment)
            )
        );

        await ledgerRepository.TransferLinked([
            new LedgerTransfer(ID.Create(), accountNumber, debitAccountNumber, loanAmount),
            new LedgerTransfer(ID.Create(), (ulong)LedgerAccountId.LoanControl, (ulong)BankId.Retail, loanAmount),
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

        var interestDue = loanAccount.BalancePosted * InterestRate / 12 / 100;

        if (-loanDebitAccount.BalancePosted < amountDue)
        {
            // they have missed their payment their account is struck down by the wrath of god himself
            await ledgerRepository.BalanceAndCloseCredit((ulong)LedgerAccountId.BadDebts, loanAccount.Id);
            return;
        }

        await ledgerRepository.TransferLinked([
            new LedgerTransfer(ID.Create(), (ulong)BankId.Retail, (ulong)LedgerAccountId.LoanControl, (UInt128)(amountDue - interestDue)),
            new LedgerTransfer(ID.Create(), loanDebitAccountId, loanAccount.Id, (UInt128)(amountDue - interestDue)),
            new LedgerTransfer(ID.Create(), (ulong)BankId.Retail, (ulong)LedgerAccountId.InterestIncome, (UInt128)interestDue)
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
