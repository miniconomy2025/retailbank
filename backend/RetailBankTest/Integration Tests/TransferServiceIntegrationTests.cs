using Microsoft.Extensions.Options;
using RetailBank.Exceptions;
using RetailBank.Models.Options;
using RetailBank.Services;

namespace RetailBank.Tests.Integration;

[Collection("Integration")]
public class TransferServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly TransferService _transferService;
    private readonly AccountService _accountService;
    
    public TransferServiceIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        
        var transferOptions = Options.Create(new TransferOptions
        {
            TransferFeePercent = 1.0m,
            DepositFeePercent = 0.5m
        });
        
        var simOptions = Options.Create(new SimulationOptions
        {
            TimeScale = 1000
        });
        
        _transferService = new TransferService(
            _fixture.LedgerRepository,
            null!, 
            transferOptions,
            simOptions
        );
        
        _accountService = new AccountService(_fixture.LedgerRepository);
    }
    
    [Fact]
    public async Task Transfer_ShouldTransferBetweenAccounts()
    {
        var payerAccountId = await _accountService.CreateTransactionalAccount(5000_00ul);
        var payeeAccountId = await _accountService.CreateTransactionalAccount(3000_00ul);
        
        await _transferService.PaySalary(payerAccountId);
        
        var transferAmount = 1000_00ul; 
        var reference = 12345ul;
        
        var payerBefore = await _fixture.LedgerRepository.GetAccount(payerAccountId);
        var payeeBefore = await _fixture.LedgerRepository.GetAccount(payeeAccountId);
        
        var transferId = await _transferService.Transfer(
            payerAccountId,
            payeeAccountId,
            transferAmount,
            reference
        );
        
        Assert.NotEqual(0u, transferId);
        
        var payerAfter = await _fixture.LedgerRepository.GetAccount(payerAccountId);
        var payeeAfter = await _fixture.LedgerRepository.GetAccount(payeeAccountId);
        
        Assert.NotNull(payerAfter);
        Assert.NotNull(payeeAfter);
        
        var expectedReduction = transferAmount + (UInt128)((decimal)transferAmount * 1.0m / 100.0m);
        Assert.Equal(
            -payerBefore!.BalancePosted - (Int128)expectedReduction,
            -payerAfter.BalancePosted
        );
        
        Assert.Equal(
            payeeBefore!.BalancePosted + (Int128)transferAmount,
            -payeeAfter.BalancePosted
        );
        
        var transfer = await _transferService.GetTransfer(transferId);
        Assert.NotNull(transfer);
        Assert.Equal(payerAccountId, transfer.DebitAccountId);
        Assert.Equal(payeeAccountId, transfer.CreditAccountId);
        Assert.Equal(transferAmount, transfer.Amount);
        Assert.Equal(reference, transfer.Reference);
    }
    
    [Fact]
    public async Task Transfer_ShouldThrowForNonexistentAccount()
    {
        var payeeId = await _accountService.CreateTransactionalAccount(5000_00ul);
        var invalidPayerId = (UInt128)9999999999;
        
        await Assert.ThrowsAsync<AccountNotFoundException>(async () =>
            await _transferService.Transfer(invalidPayerId, payeeId, 1000_00ul, 0)
        );
    }
    
    [Fact]
    public async Task PaySalary_ShouldDepositSalaryWithFee()
    {
        var salary = 5000_00ul;
        var accountId = await _accountService.CreateTransactionalAccount(salary);
        
        var accountBefore = await _fixture.LedgerRepository.GetAccount(accountId);
        await _transferService.PaySalary(accountId);
        
        var accountAfter = await _fixture.LedgerRepository.GetAccount(accountId);
        Assert.NotNull(accountAfter);
        
        var depositFee = (ulong)(salary * 0.5m / 100.0m);
        var expectedBalance = (Int128)(salary - depositFee);
        
        Assert.Equal(
            -accountBefore!.BalancePosted + expectedBalance,
            -accountAfter.BalancePosted
        );
    }
    
    [Fact]
    public async Task GetTransfers_ShouldReturnAllTransfers()
    {
        var account1 = await _accountService.CreateTransactionalAccount(5000_00ul);
        var account2 = await _accountService.CreateTransactionalAccount(3000_00ul);
        
        await _transferService.PaySalary(account1);
        
        var transferId = await _transferService.Transfer(account1, account2, 1000_00ul, 99999ul);
        
        var transfers = await _transferService.GetTransfers(100, 0, null);
        
        Assert.NotEmpty(transfers);
        Assert.Contains(transfers, t => t.Id == transferId);
    }
    
    [Fact]
    public async Task GetTransfers_ShouldFilterByReference()
    {
        var account1 = await _accountService.CreateTransactionalAccount(5000_00ul);
        var account2 = await _accountService.CreateTransactionalAccount(3000_00ul);
        
        await _transferService.PaySalary(account1);
        
        var reference = 88888ul;
        await _transferService.Transfer(account1, account2, 500_00ul, reference);
        
        var transfers = await _transferService.GetTransfers(100, 0, reference);
        
        Assert.NotEmpty(transfers);
        Assert.All(transfers, t => Assert.Equal(reference, t.Reference));
    }
    
    [Fact]
    public async Task GetRecentVolume_ShouldCalculateTotalVolume()
    {
        var account1 = await _accountService.CreateTransactionalAccount(10000_00ul);
        var account2 = await _accountService.CreateTransactionalAccount(5000_00ul);
        
        await _transferService.PaySalary(account1);
        
        var amount1 = 1000_00ul;
        var amount2 = 500_00ul;
        
        await _transferService.Transfer(account1, account2, amount1, 1ul);
        await _transferService.Transfer(account1, account2, amount2, 2ul);
        
        var volume = await _transferService.GetRecentVolume();
        
        Assert.True(volume >= amount1 + amount2);
    }
}
