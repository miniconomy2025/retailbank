using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RetailBank.Exceptions;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;
using RetailBank.Repositories;
using RetailBank.Services;
using Xunit;

namespace RetailBank.Tests.Integration;


[Collection("Integration")]
public class EndToEndScenarioTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly AccountService _accountService;
    private readonly LoanService _loanService;
    private readonly TransferService _transferService;
    
    public EndToEndScenarioTests(IntegrationTestFixture fixture)
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
        
        _accountService = new AccountService(_fixture.LedgerRepository);
        _loanService = new LoanService(_fixture.LedgerRepository, loanOptions);
        _transferService = new TransferService(
            _fixture.LedgerRepository,
            null!,
            transferOptions,
            simOptions
        );
    }
    
    [Fact]
    public async Task CompleteCustomerLifecycle_ShouldWorkEndToEnd()
    {
        // create customer account for this test
        var salary = 6000_00ul;
        var customerAccountId = await _accountService.CreateTransactionalAccount(salary);
        Assert.NotEqual(0u, customerAccountId);
        
        // pay salary
        await _transferService.PaySalary(customerAccountId);
        var accountAfterSalary = await _accountService.GetAccount(customerAccountId);
        Assert.NotNull(accountAfterSalary);

        // credit balance so negative
        Assert.True(accountAfterSalary.BalancePosted < 0);
        
        // take out loan
        var loanAmount = 15000_00ul;
        var loanAccountId = await _loanService.CreateLoanAccount(customerAccountId, loanAmount);
        Assert.NotEqual(0u, loanAccountId);
        
        var accountAfterLoan = await _accountService.GetAccount(customerAccountId);
        Assert.NotNull(accountAfterLoan);
        Assert.True(accountAfterLoan.BalancePosted < accountAfterSalary.BalancePosted);

        // Make transfer
        var receiverSalary = 4000_00ul;
        var receiverAccountId = await _accountService.CreateTransactionalAccount(receiverSalary);
        var transferAmount = 2000_00ul;
        var transferId = await _transferService.Transfer(
            customerAccountId,
            receiverAccountId,
            transferAmount,
            777ul
        );
        Assert.NotEqual(0u, transferId);
        
        // verify if the receiver actually received the money?
        var receiverAccount = await _accountService.GetAccount(receiverAccountId);
        Assert.NotNull(receiverAccount);
        Assert.Equal((Int128)(transferAmount + receiverSalary), -receiverAccount.BalancePosted);
        
        // pay loan installment
        var loanBefore = await _accountService.GetAccount(loanAccountId);
        await _loanService.PayInstallment(loanAccountId);
        var loanAfter = await _accountService.GetAccount(loanAccountId);
        
        Assert.NotNull(loanBefore);
        Assert.NotNull(loanAfter);
        Assert.True(loanAfter.BalancePosted < loanBefore.BalancePosted); // the loan balance is less than before since asset account
        
        // verify entire history
        var customerTransfers = await _accountService.GetAccountTransfers(
            customerAccountId,
            100,
            0,
            null,
            null
        );
        Assert.NotEmpty(customerTransfers);
        
        var loans = await _accountService.GetAccountLoans(customerAccountId);
        Assert.Contains(loans, l => l.Id == loanAccountId);
    }
}