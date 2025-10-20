using Moq;
using RetailBank.Models.Ledger;
using RetailBank.Repositories;
using RetailBank.Services;

namespace RetailBankTest;

public class AccountServiceTests
{
    [Fact]
    public async Task CreateTransactionalAccount_CreatesAccountAndReturnsId()
    {
        // Arrange
        var mockLedgerRepository = new Mock<ILedgerRepository>();
        var salary = 50000ul;

        mockLedgerRepository
            .Setup(r => r.CreateAccount(It.IsAny<LedgerAccount>()))
            .Returns(Task.CompletedTask);

        var accountService = new AccountService(mockLedgerRepository.Object);

        // Act
        var accountId = await accountService.CreateTransactionalAccount(salary);

        // Assert
        Assert.NotEqual(UInt128.Zero, accountId);

        mockLedgerRepository.Verify(
            r => r.CreateAccount(It.Is<LedgerAccount>(acc =>
                acc.AccountType == LedgerAccountType.Transactional &&
                acc.DebitOrder != null &&
                acc.DebitOrder.DebitAccountId == (ulong)Bank.Retail &&
                acc.DebitOrder.Amount == salary
            )),
            Times.Once
        );

        Assert.True(accountId >= new UInt128(0, 1000_0000_0000ul) &&
            accountId < new UInt128(0, 1001_0000_0000ul));
    }

    [Fact]
    public async Task GetAccounts_WithFilters_ReturnsMatchingAccounts()
    {
        // Arrange
        var mockLedgerRepository = new Mock<ILedgerRepository>();
        var expectedAccounts = new List<LedgerAccount>
        {
            new LedgerAccount(
                Id: new UInt128(0, 1000_0000_0001),
                AccountType: LedgerAccountType.Transactional,
                DebitOrder: new DebitOrder((ulong)Bank.Retail, 50000),
                Closed: false,
                DebitsPending: 0,
                DebitsPosted: 100000,
                CreditsPending: 0,
                CreditsPosted: 50000,
                Cursor: 1
            ),
            new LedgerAccount(
                Id: new UInt128(0, 1000_0000_0002),
                AccountType: LedgerAccountType.Transactional,
                DebitOrder: new DebitOrder((ulong)Bank.Retail, 60000),
                Closed: false,
                DebitsPending: 0,
                DebitsPosted: 200000,
                CreditsPending: 0,
                CreditsPosted: 100000,
                Cursor: 2
            )
        };

        mockLedgerRepository
            .Setup(r => r.GetAccounts(LedgerAccountType.Transactional, null, 100, 0))
            .ReturnsAsync(expectedAccounts);

        var accountService = new AccountService(mockLedgerRepository.Object);

        // Act
        var accounts = await accountService.GetAccounts(LedgerAccountType.Transactional, 100, 0);

        // Assert
        Assert.Equal(2, accounts.Count());
        Assert.Equal(expectedAccounts, accounts);

        mockLedgerRepository.Verify(
            r => r.GetAccounts(LedgerAccountType.Transactional, null, 100, 0),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccount_WithExistingAccountId_ReturnsAccount()
    {
        // Arrange
        var mockLedgerRepository = new Mock<ILedgerRepository>();
        var accountId = new UInt128(0, 1000_0000_0001);

        var expectedAccount = new LedgerAccount(
            Id: accountId,
            AccountType: LedgerAccountType.Transactional,
            DebitOrder: new DebitOrder((ulong)Bank.Retail, 50000),
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 100000,
            CreditsPending: 0,
            CreditsPosted: 50000,
            Cursor: 1
        );

        mockLedgerRepository
            .Setup(r => r.GetAccount(accountId))
            .ReturnsAsync(expectedAccount);

        var accountService = new AccountService(mockLedgerRepository.Object);

        // Act
        var account = await accountService.GetAccount(accountId);

        // Assert
        Assert.NotNull(account);
        Assert.Equal(accountId, account.Id);
        Assert.Equal(expectedAccount, account);

        mockLedgerRepository.Verify(
            r => r.GetAccount(accountId),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccountTransfers_WithNoFilters_ReturnsAllTransfers()
    {
        // Arrange
        var accountId = new UInt128(0, 1000_0000_0001);
        var mockLedgerRepository = new Mock<ILedgerRepository>();

        var expectedTransfers = new List<LedgerTransfer>
        {
            new LedgerTransfer(
                Id: new UInt128(0, 1),
                DebitAccountId: accountId,
                CreditAccountId: new UInt128(0, 1000_0000_0002),
                Amount: 10000,
                Reference: 123456,
                TransferType: TransferType.Transfer
            ),
            new LedgerTransfer(
                Id: new UInt128(0, 2),
                DebitAccountId: new UInt128(0, 1000_0000_0003),
                CreditAccountId: accountId,
                Amount: 5000,
                Reference: 123457,
                TransferType: TransferType.Transfer
            )
        };

        mockLedgerRepository
            .Setup(r => r.GetAccountTransfers(accountId, 100, 0, null, null))
            .ReturnsAsync(expectedTransfers);

        var accountService = new AccountService(mockLedgerRepository.Object);

        // Act
        var transfers = await accountService.GetAccountTransfers(accountId, 100, 0, null, null);

        // Assert
        Assert.Equal(2, transfers.Count());
        Assert.Equal(expectedTransfers, transfers);

        mockLedgerRepository.Verify(
            r => r.GetAccountTransfers(accountId, 100, 0, null, null),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccountLoans_WithExistingLoans_ReturnsLoanAccounts()
    {
        // Arrange
        var accountId = new UInt128(0, 1000_0000_0001);
        var mockLedgerRepository = new Mock<ILedgerRepository>();
        const uint BatchMax = 8189;

        var expectedLoanAccounts = new List<LedgerAccount>
        {
            new LedgerAccount(
                Id: new UInt128(0, 3000_0000_0001),
                AccountType: LedgerAccountType.Loan,
                DebitOrder: new DebitOrder(accountId, 2000),
                Closed: false,
                DebitsPending: 0,
                DebitsPosted: 0,
                CreditsPending: 0,
                CreditsPosted: 100000,
                Cursor: 1
            ),
            new LedgerAccount(
                Id: new UInt128(0, 3000_0000_0002),
                AccountType: LedgerAccountType.Loan,
                DebitOrder: new DebitOrder(accountId, 3000),
                Closed: false,
                DebitsPending: 0,
                DebitsPosted: 50000,
                CreditsPending: 0,
                CreditsPosted: 200000,
                Cursor: 2
            )
        };

        mockLedgerRepository
            .Setup(r => r.GetAccounts(LedgerAccountType.Loan, accountId, BatchMax, 0))
            .ReturnsAsync(expectedLoanAccounts);

        var accountService = new AccountService(mockLedgerRepository.Object);

        // Act
        var loanAccounts = await accountService.GetAccountLoans(accountId);

        // Assert
        Assert.Equal(2, loanAccounts.Count());

        mockLedgerRepository.Verify(
            r => r.GetAccounts(LedgerAccountType.Loan, accountId, BatchMax, 0),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccounts_WithNullAccountType_ReturnsAllAccountTypes()
    {
        // Arrange
        var mockLedgerRepository = new Mock<ILedgerRepository>();

        var expectedAccounts = new List<LedgerAccount>
        {
            new LedgerAccount(
                Id: new UInt128(0, 1000_0000_0001),
                AccountType: LedgerAccountType.Transactional,
                DebitOrder: new DebitOrder((ulong)Bank.Retail, 50000),
                Closed: false,
                DebitsPending: 0,
                DebitsPosted: 100000,
                CreditsPending: 0,
                CreditsPosted: 50000,
                Cursor: 1
            ),
            new LedgerAccount(
                Id: new UInt128(0, 3000_0000_0001),
                AccountType: LedgerAccountType.Loan,
                DebitOrder: new DebitOrder((ulong)Bank.Retail, 2000),
                Closed: false,
                DebitsPending: 0,
                DebitsPosted: 0,
                CreditsPending: 0,
                CreditsPosted: 100000,
                Cursor: 1
            )
        };

        mockLedgerRepository
            .Setup(r => r.GetAccounts(null, null, 100, 0))
            .ReturnsAsync(expectedAccounts);

        var accountService = new AccountService(mockLedgerRepository.Object);

        // Act
        var accounts = await accountService.GetAccounts(null, 100, 0);

        // Assert
        Assert.Equal(2, accounts.Count());
        Assert.Contains(accounts, a => a.AccountType == LedgerAccountType.Transactional);
        Assert.Contains(accounts, a => a.AccountType == LedgerAccountType.Loan);

        mockLedgerRepository.Verify(
            r => r.GetAccounts(null, null, 100, 0),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccount_WithNonExistentAccountId_ReturnsNull()
    {
        // Arrange
        var mockLedgerRepository = new Mock<ILedgerRepository>();
        var nonExistentAccountId = new UInt128(0, 9999_9999_9999);

        mockLedgerRepository
            .Setup(r => r.GetAccount(nonExistentAccountId))
            .ReturnsAsync((LedgerAccount?)null);

        var accountService = new AccountService(mockLedgerRepository.Object);

        // Act
        var account = await accountService.GetAccount(nonExistentAccountId);

        // Assert
        Assert.Null(account);

        mockLedgerRepository.Verify(
            r => r.GetAccount(nonExistentAccountId),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccountTransfers_WithFilters_PassesFiltersToRepository()
    {
        // Arrange
        var accountId = new UInt128(0, 1000_0000_0001);
        var mockLedgerRepository = new Mock<ILedgerRepository>();
        const ulong reference = 123456;
        const TransferSide side = TransferSide.Debit;

        var expectedTransfers = new List<LedgerTransfer>
        {
            new LedgerTransfer(
                Id: new UInt128(0, 1),
                DebitAccountId: accountId,
                CreditAccountId: new UInt128(0, 1000_0000_0002),
                Amount: 10000,
                Reference: reference,
                TransferType: TransferType.Transfer
            )
        };

        mockLedgerRepository
            .Setup(r => r.GetAccountTransfers(accountId, 50, 100, reference, side))
            .ReturnsAsync(expectedTransfers);

        var accountService = new AccountService(mockLedgerRepository.Object);

        // Act
        var transfers = await accountService.GetAccountTransfers(accountId, 50, 100, reference, side);

        // Assert
        Assert.Single(transfers);
        Assert.Equal(reference, transfers.First().Reference);

        mockLedgerRepository.Verify(
            r => r.GetAccountTransfers(accountId, 50, 100, reference, side),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccountTransfers_WithEmptyResults_ReturnsEmptyList()
    {
        // Arrange
        var accountId = new UInt128(0, 1000_0000_0001);
        var mockLedgerRepository = new Mock<ILedgerRepository>();

        mockLedgerRepository
            .Setup(r => r.GetAccountTransfers(accountId, 100, 0, null, null))
            .ReturnsAsync(new List<LedgerTransfer>());

        var accountService = new AccountService(mockLedgerRepository.Object);

        // Act
        var transfers = await accountService.GetAccountTransfers(accountId, 100, 0, null, null);

        // Assert
        Assert.Empty(transfers);

        mockLedgerRepository.Verify(
            r => r.GetAccountTransfers(accountId, 100, 0, null, null),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccountLoans_WithNoLoans_ReturnsEmptyList()
    {
        // Arrange
        var accountId = new UInt128(0, 1000_0000_0001);
        var mockLedgerRepository = new Mock<ILedgerRepository>();
        const uint BatchMax = 8189;

        mockLedgerRepository
            .Setup(r => r.GetAccounts(LedgerAccountType.Loan, accountId, BatchMax, 0))
            .ReturnsAsync(new List<LedgerAccount>());

        var accountService = new AccountService(mockLedgerRepository.Object);

        // Act
        var loanAccounts = await accountService.GetAccountLoans(accountId);

        // Assert
        Assert.Empty(loanAccounts);

        mockLedgerRepository.Verify(
            r => r.GetAccounts(LedgerAccountType.Loan, accountId, BatchMax, 0),
            Times.Once
        );
    }
}