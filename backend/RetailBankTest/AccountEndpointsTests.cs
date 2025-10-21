using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using RetailBank.Endpoints;
using RetailBank.Models.Dtos;
using RetailBank.Models.Ledger;
using RetailBank.Repositories;
using RetailBank.Services;

namespace RetailBankTest;

public class AccountEndpointsTests
{
    private readonly Mock<ILedgerRepository> _mockLedgerRepository;
    private readonly AccountService _accountService;
    private readonly Mock<ILogger<AccountService>> _mockLogger;

    public AccountEndpointsTests()
    {
        _mockLedgerRepository = new Mock<ILedgerRepository>();
        _accountService = new AccountService(_mockLedgerRepository.Object);
        _mockLogger = new Mock<ILogger<AccountService>>();
    }

    [Fact]
    public async Task CreateTransactionalAccount_ValidRequest_ReturnsOkWithAccountId()
    {
        // Arrange
        var request = new CreateTransactionAccountRequest(SalaryCents: 50000);

        _mockLedgerRepository
            .Setup(r => r.CreateAccount(It.IsAny<LedgerAccount>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await AccountEndpoints.CreateTransactionalAccount(request, _accountService);

        // Assert
        var okResult = Assert.IsType<Ok<CreateTransactionalAccountResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(12, okResult.Value.AccountId.Length);
        Assert.StartsWith("1000", okResult.Value.AccountId);

        _mockLedgerRepository.Verify(
            r => r.CreateAccount(It.Is<LedgerAccount>(acc =>
                acc.AccountType == LedgerAccountType.Transactional &&
                acc.DebitOrder != null &&
                acc.DebitOrder.Amount == request.SalaryCents
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccounts_WithAccountType_ReturnsPaginatedAccounts()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/accounts";

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
                Cursor: 100
            ),
            new LedgerAccount(
                Id: new UInt128(0, 1000_0000_0002),
                AccountType: LedgerAccountType.Transactional,
                DebitOrder: new DebitOrder((ulong)Bank.Retail, 50000),
                Closed: false,
                DebitsPending: 0,
                DebitsPosted: 100000,
                CreditsPending: 0,
                CreditsPosted: 50000,
                Cursor: 99
            )
        };

        _mockLedgerRepository
            .Setup(r => r.GetAccounts(LedgerAccountType.Transactional, null, 25, 0))
            .ReturnsAsync(expectedAccounts);

        // Act
        var result = await AccountEndpoints.GetAccounts(
            httpContext,
            _accountService,
            LedgerAccountType.Transactional,
            25,
            0
        );

        // Assert
        var okResult = Assert.IsType<Ok<CursorPagination<AccountDto>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.Items.Count());
        Assert.Equal("/accounts?limit=25&cursorMax=98", okResult.Value.Next);

        _mockLedgerRepository.Verify(
            r => r.GetAccounts(LedgerAccountType.Transactional, null, 25, 0),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccounts_WithNullAccountType_ReturnsAllAccounts()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/accounts";

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
                Cursor: 100
            ),
            new LedgerAccount(
                Id: new UInt128(0, 3000_0000_0001),
                AccountType: LedgerAccountType.Loan,
                DebitOrder: new DebitOrder((ulong)Bank.Retail, 50000),
                Closed: false,
                DebitsPending: 0,
                DebitsPosted: 100000,
                CreditsPending: 0,
                CreditsPosted: 50000,
                Cursor: 99
            )
        };

        _mockLedgerRepository
            .Setup(r => r.GetAccounts(null, null, 25, 0))
            .ReturnsAsync(expectedAccounts);

        // Act
        var result = await AccountEndpoints.GetAccounts(
            httpContext,
            _accountService,
            null,
            25,
            0
        );

        // Assert
        var okResult = Assert.IsType<Ok<CursorPagination<AccountDto>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.Items.Count());
        Assert.Equal("/accounts?limit=25&cursorMax=98", okResult.Value.Next);
    }

    [Fact]
    public async Task GetAccounts_LimitOne_ReturnsSingleAccountWithCorrectNextCursor()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/accounts";

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
                Cursor: 50
            )
        };

        _mockLedgerRepository
            .Setup(r => r.GetAccounts(null, null, 1, 0))
            .ReturnsAsync(expectedAccounts);

        // Act
        var result = await AccountEndpoints.GetAccounts(
            httpContext,
            _accountService,
            null,
            1,
            0
        );

        // Assert
        var okResult = Assert.IsType<Ok<CursorPagination<AccountDto>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Single(okResult.Value.Items);
        Assert.Equal("/accounts?limit=1&cursorMax=49", okResult.Value.Next);

        _mockLedgerRepository.Verify(
            r => r.GetAccounts(null, null, 1, 0),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccount_ExistingAccount_ReturnsOkWithAccountDto()
    {
        // Arrange
        ulong accountId = 1000_0000_0001;
        var expectedAccount = new LedgerAccount(
            Id: new UInt128(0, accountId),
            AccountType: LedgerAccountType.Transactional,
            DebitOrder: new DebitOrder((ulong)Bank.Retail, 50000),
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 100000,
            CreditsPending: 0,
            CreditsPosted: 50000,
            Cursor: 1
        );

        _mockLedgerRepository
            .Setup(r => r.GetAccount(new UInt128(0, accountId)))
            .ReturnsAsync(expectedAccount);

        // Act
        var result = await AccountEndpoints.GetAccount(accountId, _accountService, _mockLogger.Object);

        // Assert
        var okResult = Assert.IsType<Ok<AccountDto>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(new UInt128(0, accountId).ToString(), okResult.Value.Id);
        Assert.Equal(LedgerAccountType.Transactional, okResult.Value.AccountType);

        _mockLedgerRepository.Verify(
            r => r.GetAccount(new UInt128(0, accountId)),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccount_NonExistentAccount_ReturnsNotFound()
    {
        // Arrange
        ulong accountId = 9999_9999_9999;

        _mockLedgerRepository
            .Setup(r => r.GetAccount(new UInt128(0, accountId)))
            .ReturnsAsync((LedgerAccount?)null);

        // Act
        var result = await AccountEndpoints.GetAccount(accountId, _accountService, _mockLogger.Object);

        // Assert
        Assert.IsType<NotFound>(result);

        _mockLedgerRepository.Verify(
            r => r.GetAccount(new UInt128(0, accountId)),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccountTransfers_WithTransfers_ReturnsPaginatedTransfers()
    {
        // Arrange
        ulong accountId = 1000_0000_0001;
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = $"/accounts/{accountId}/transfers";

        var expectedTransfers = new List<LedgerTransfer>
        {
            new LedgerTransfer(
                Id: new UInt128(0, 1),
                DebitAccountId: new UInt128(0, accountId),
                CreditAccountId: new UInt128(0, 1000_0000_0002),
                Amount: 10000,
                Reference: 123456,
                TransferType: TransferType.Transfer,
                Cursor: 100
            ),
            new LedgerTransfer(
                Id: new UInt128(0, 2),
                DebitAccountId: new UInt128(0, accountId),
                CreditAccountId: new UInt128(0, 1000_0000_0003),
                Amount: 5000,
                Reference: 123456,
                TransferType: TransferType.Transfer,
                Cursor: 99
            )
        };

        _mockLedgerRepository
            .Setup(r => r.GetAccountTransfers(new UInt128(0, accountId), 25, 0, null, null))
            .ReturnsAsync(expectedTransfers);

        // Act
        var result = await AccountEndpoints.GetAccountTransfers(
            accountId,
            httpContext,
            _accountService,
            25,
            0,
            null,
            null
        );

        // Assert
        var okResult = Assert.IsType<Ok<CursorPagination<TransferDto>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.Items.Count());
        Assert.Equal($"/accounts/{accountId}/transfers?limit=25&cursorMax=98", okResult.Value.Next);

        _mockLedgerRepository.Verify(
            r => r.GetAccountTransfers(new UInt128(0, accountId), 25, 0, null, null),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccountLoans_WithLoans_ReturnsLoanAccounts()
    {
        // Arrange
        ulong accountId = 1000_0000_0001;
        var httpContext = new DefaultHttpContext();

        var expectedLoans = new List<LedgerAccount>
        {
            new LedgerAccount(
                Id: new UInt128(0, 3000_0000_0001),
                AccountType: LedgerAccountType.Loan,
                DebitOrder: new DebitOrder((ulong)Bank.Retail, 50000),
                Closed: false,
                DebitsPending: 0,
                DebitsPosted: 100000,
                CreditsPending: 0,
                CreditsPosted: 50000,
                Cursor: 1
            ),
            new LedgerAccount(
                Id: new UInt128(0, 3000_0000_0002),
                AccountType: LedgerAccountType.Loan,
                DebitOrder: new DebitOrder((ulong)Bank.Retail, 50000),
                Closed: false,
                DebitsPending: 0,
                DebitsPosted: 100000,
                CreditsPending: 0,
                CreditsPosted: 50000,
                Cursor: 1
            )
        };

        _mockLedgerRepository
            .Setup(r => r.GetAccounts(LedgerAccountType.Loan, new UInt128(0, accountId), 8189, 0))
            .ReturnsAsync(expectedLoans);

        // Act
        var result = await AccountEndpoints.GetAccountLoans(
            accountId,
            httpContext,
            _accountService
        );

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<AccountDto>>>(result);
        Assert.NotNull(okResult.Value);
        var loanDtos = okResult.Value.ToList();
        Assert.Equal(2, loanDtos.Count);
        Assert.All(loanDtos, dto => Assert.Equal(LedgerAccountType.Loan, dto.AccountType));

        _mockLedgerRepository.Verify(
            r => r.GetAccounts(LedgerAccountType.Loan, new UInt128(0, accountId), 8189, 0),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccountLoans_NoLoans_ReturnsEmptyArray()
    {
        // Arrange
        ulong accountId = 1000_0000_0001;
        var httpContext = new DefaultHttpContext();

        _mockLedgerRepository
            .Setup(r => r.GetAccounts(LedgerAccountType.Loan, new UInt128(0, accountId), 8189, 0))
            .ReturnsAsync(new List<LedgerAccount>());

        // Act
        var result = await AccountEndpoints.GetAccountLoans(
            accountId,
            httpContext,
            _accountService
        );

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<AccountDto>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Empty(okResult.Value);

        _mockLedgerRepository.Verify(
            r => r.GetAccounts(LedgerAccountType.Loan, new UInt128(0, accountId), 8189, 0),
            Times.Once
        );
    }
}
