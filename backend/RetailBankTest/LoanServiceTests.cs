using Microsoft.Extensions.Options;
using Moq;
using RetailBank.Exceptions;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;
using RetailBank.Repositories;
using RetailBank.Services;

namespace RetailBankTest;

public class LoanServiceTests
{
    [Fact]
    public async Task CreateLoanAccount_WithValidTransactionalAccount_CreatesLoanAccountAndTransfersAmount()
    {
        // Arrange
        var debitAccountNumber = new UInt128(0, 12345);
        var loanAmount = 100000ul;
        
        var mockLedgerRepository = new Mock<ILedgerRepository>();
        var mockOptions = new Mock<IOptions<LoanOptions>>();
        
        // Setup the debit account (the account that will receive the loan)
        var debitAccount = new LedgerAccount(
            Id: debitAccountNumber,
            AccountType: LedgerAccountType.Transactional,
            DebitOrder: null,
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 50000,
            CreditsPending: 0,
            CreditsPosted: 50000,
            Cursor: 0
        );
        
        // Setup loan options
        var loanOptions = new LoanOptions
        {
            AnnualInterestRatePercentage = 10.0m,
            LoanPeriodMonths = 60
        };
        mockOptions.Setup(o => o.Value).Returns(loanOptions);
        
        // Mock GetAccount to return the debit account
        mockLedgerRepository
            .Setup(r => r.GetAccount(debitAccountNumber))
            .ReturnsAsync(debitAccount);
        
        // Mock CreateAccount to succeed
        mockLedgerRepository
            .Setup(r => r.CreateAccount(It.IsAny<LedgerAccount>()))
            .Returns(Task.CompletedTask);
        
        // Mock Transfer to succeed
        mockLedgerRepository
            .Setup(r => r.Transfer(It.IsAny<LedgerTransfer>()))
            .ReturnsAsync(new UInt128(0, 1));
        
        var loanService = new LoanService(mockLedgerRepository.Object, mockOptions.Object);
        
        // Act
        var loanAccountNumber = await loanService.CreateLoanAccount(debitAccountNumber, loanAmount);
        
        // Assert
        Assert.NotEqual(UInt128.Zero, loanAccountNumber);
        
        // Verify GetAccount was called
        mockLedgerRepository.Verify(
            r => r.GetAccount(debitAccountNumber),
            Times.Once
        );
        
        // Verify CreateAccount was called with correct account type and debit order
        mockLedgerRepository.Verify(
            r => r.CreateAccount(It.Is<LedgerAccount>(acc => 
                acc.AccountType == LedgerAccountType.Loan &&
                acc.DebitOrder != null &&
                acc.DebitOrder.DebitAccountId == debitAccountNumber
            )),
            Times.Once
        );
        
        // Verify Transfer was called with the loan amount
        mockLedgerRepository.Verify(
            r => r.Transfer(It.Is<LedgerTransfer>(t => 
                t.CreditAccountId == debitAccountNumber &&
                t.Amount == loanAmount
            )),
            Times.Once
        );
    }
}

