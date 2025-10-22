using Microsoft.Extensions.Options;
using RetailBank.Exceptions;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;
using RetailBank.Services;

namespace RetailBank.Tests.Integration;

[Collection("Integration")]
public class LoanServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly LoanService _loanService;
    private readonly AccountService _accountService;
    private readonly TransferService _transferService;
    
    public LoanServiceIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        
        var loanOptions = Options.Create(new LoanOptions
        {
            AnnualInterestRatePercentage = 12.0m,
            LoanPeriodMonths = 24
        });
        
        var transferOptions = Options.Create(new TransferOptions
        {
            TransferFeePercent = 1.0m,
            DepositFeePercent = 0.5m
        });
        
        var simOptions = Options.Create(new SimulationOptions
        {
            TimeScale = 1000
        });
        
        _loanService = new LoanService(_fixture.LedgerRepository, loanOptions);
        _accountService = new AccountService(_fixture.LedgerRepository);
        _transferService = new TransferService(
            _fixture.LedgerRepository,
            null!, // InterbankClient not needed for these tests
            transferOptions,
            simOptions
        );
    }
    
    [Fact]
    public async Task CreateLoanAccount_ShouldCreateLoanAndTransferFunds()
    {
        // Arrange
        var debitAccountId = await _accountService.CreateTransactionalAccount(5000_00ul);
        await _transferService.PaySalary(debitAccountId); // Add initial balance
        
        var loanAmount = 10000_00ul; // $10,000
        
        // Act
        var loanAccountId = await _loanService.CreateLoanAccount(debitAccountId, loanAmount);
        
        // Assert
        Assert.NotEqual(0u, loanAccountId);
        
        var loanAccount = await _fixture.LedgerRepository.GetAccount(loanAccountId);
        Assert.NotNull(loanAccount);
        Assert.Equal(LedgerAccountType.Loan, loanAccount.AccountType);
        Assert.NotNull(loanAccount.DebitOrder);
        Assert.Equal(debitAccountId, loanAccount.DebitOrder.DebitAccountId);
        
        // Verify loan was credited (debit account so positive balance )
        Assert.Equal((Int128)loanAmount, loanAccount.BalancePosted);
        
        // Verify debit account received funds
        var debitAccount = await _fixture.LedgerRepository.GetAccount(debitAccountId);
        Assert.NotNull(debitAccount);
        // Debit account should have a credit balance so check for < 0
        Assert.True(debitAccount.BalancePosted < 0);
    }
    
    [Fact]
    public async Task CreateLoanAccount_ShouldThrowForInvalidAccount()
    {
        // Arrange
        var invalidAccountId = (UInt128)9999999999;
        
        // Act & Assert
        await Assert.ThrowsAsync<AccountNotFoundException>(async () =>
            await _loanService.CreateLoanAccount(invalidAccountId, 10000_00ul)
        );
    }
    
    [Fact]
    public async Task CreateLoanAccount_ShouldThrowForZeroAmount()
    {
        // Arrange
        var debitAccountId = await _accountService.CreateTransactionalAccount(5000_00ul);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidLoanAmountException>(async () =>
            await _loanService.CreateLoanAccount(debitAccountId, 0ul)
        );
    }
    
    [Fact]
    public async Task PayInstallment_ShouldPayLoanInstallment()
    {
        // Arrange
        var debitAccountId = await _accountService.CreateTransactionalAccount(50000_00ul);
        await _transferService.PaySalary(debitAccountId);
        
        var loanAmount = 10000_00ul;
        var loanAccountId = await _loanService.CreateLoanAccount(debitAccountId, loanAmount);
        
        var loanAccountBefore = await _fixture.LedgerRepository.GetAccount(loanAccountId);
        var debitAccountBefore = await _fixture.LedgerRepository.GetAccount(debitAccountId);
        
        // Act
        await _loanService.PayInstallment(loanAccountId);
        
        // Assert
        var loanAccountAfter = await _fixture.LedgerRepository.GetAccount(loanAccountId);
        var debitAccountAfter = await _fixture.LedgerRepository.GetAccount(debitAccountId);
        
        Assert.NotNull(loanAccountAfter);
        Assert.NotNull(debitAccountAfter);
        
        // Loan balance should be reduced 
        Assert.True(loanAccountAfter.BalancePosted < loanAccountBefore!.BalancePosted);
        
        // Debit account balance should be reduced
        Assert.True(debitAccountAfter.BalancePosted > debitAccountBefore!.BalancePosted);
    }
}
