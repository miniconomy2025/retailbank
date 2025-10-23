using RetailBank.Models.Ledger;
using RetailBank.Services;

namespace RetailBank.Tests.Integration;

[Collection("Integration")]
public class AccountServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly AccountService _accountService;
    
    public AccountServiceIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _accountService = new AccountService(_fixture.LedgerRepository);
    }
    
    [Fact]
    public async Task CreateTransactionalAccount_ShouldCreateAccountSuccessfully()
    {
        var salary = 5000_00ul; 
        
        var accountId = await _accountService.CreateTransactionalAccount(salary);
        
        Assert.NotEqual(0u, accountId);
        
        var account = await _accountService.GetAccount(accountId);
        Assert.NotNull(account);
        Assert.Equal(LedgerAccountType.Transactional, account.AccountType);
        Assert.NotNull(account.DebitOrder);
        Assert.Equal(salary, account.DebitOrder.Amount);
        Assert.Equal((ulong)Bank.Retail, account.DebitOrder.DebitAccountId);
    }
    
    [Fact]
    public async Task GetAccounts_ShouldReturnAccountsByType()
    {
        var accountId1 = await _accountService.CreateTransactionalAccount(3000_00ul);
        var accountId2 = await _accountService.CreateTransactionalAccount(4000_00ul);
        
        var accounts = await _accountService.GetAccounts(LedgerAccountType.Transactional, 100, 0);
        
        Assert.Contains(accounts, a => a.Id == accountId1);
        Assert.Contains(accounts, a => a.Id == accountId2);
    }
    
    [Fact]
    public async Task GetAccountTransfers_ShouldReturnEmptyForNewAccount()
    {
        var accountId = await _accountService.CreateTransactionalAccount(3000_00ul);
        
        var transfers = await _accountService.GetAccountTransfers(accountId, 100, 0, null, null);
        
        Assert.Collection(transfers, [(transfer) => Assert.Equal(3000_00ul, transfer.Amount)]);
    }
}
