using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RetailBank.Exceptions;
using RetailBank.Models.Interbank;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;
using RetailBank.Repositories;
using RetailBank.Services;

namespace RetailBankTest;

public class TransferServiceTests
{
    private readonly Mock<ILedgerRepository> _mockLedgerRepository;
    private readonly Mock<InterbankClient> _mockInterbankClient;
    private readonly Mock<IOptions<TransferOptions>> _mockTransferOptions;
    private readonly Mock<IOptions<SimulationOptions>> _mockSimulationOptions;
    private readonly TransferService _transferService;

    public TransferServiceTests()
    {
        _mockLedgerRepository = new Mock<ILedgerRepository>();
        
        // Create a mock InterbankClient
        var mockHttpClient = new HttpClient();
        var mockInterbankOptions = new Mock<IOptions<InterbankTransferOptions>>();
        var mockLogger = new Mock<ILogger<InterbankClient>>();
        
        var interbankOptions = new InterbankTransferOptions
        {
            RetryCount = 3,
            DelaySeconds = 1,
            LoanAmountCents = 10_000_000__00,
            Banks = new Dictionary<Bank, InterbankTransferBankDetails>()
        };
        mockInterbankOptions.Setup(o => o.Value).Returns(interbankOptions);
        
        _mockInterbankClient = new Mock<InterbankClient>(
            mockHttpClient, 
            mockInterbankOptions.Object, 
            mockLogger.Object
        );
        
        _mockTransferOptions = new Mock<IOptions<TransferOptions>>();
        _mockSimulationOptions = new Mock<IOptions<SimulationOptions>>();

        // Setup default options
        var transferOptions = new TransferOptions
        {
            TransferFeePercent = 2.5m,
            DepositFeePercent = 1.0m
        };
        _mockTransferOptions.Setup(o => o.Value).Returns(transferOptions);

        var simulationOptions = new SimulationOptions
        {
            TimeScale = 720,
            SimulationStart = 2524600800000
        };
        _mockSimulationOptions.Setup(o => o.Value).Returns(simulationOptions);

        _transferService = new TransferService(
            _mockLedgerRepository.Object,
            _mockInterbankClient.Object,
            _mockTransferOptions.Object,
            _mockSimulationOptions.Object
        );
    }

    [Fact]
    public async Task GetTransfer_WithValidId_ReturnsTransfer()
    {
        // Arrange
        var transferId = new UInt128(0, 12345);
        var expectedTransfer = new LedgerTransfer(
            Id: transferId,
            DebitAccountId: new UInt128(0, 1000),
            CreditAccountId: new UInt128(0, 2000),
            Amount: new UInt128(0, 100),
            Reference: 0,
            TransferType: TransferType.Transfer
        );

        _mockLedgerRepository
            .Setup(r => r.GetTransfer(transferId))
            .ReturnsAsync(expectedTransfer);

        // Act
        var result = await _transferService.GetTransfer(transferId);

        // Assert
        Assert.Equal(expectedTransfer, result);
        _mockLedgerRepository.Verify(r => r.GetTransfer(transferId), Times.Once);
    }

    [Fact]
    public async Task GetTransfer_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var transferId = new UInt128(0, 99999);
        _mockLedgerRepository
            .Setup(r => r.GetTransfer(transferId))
            .ReturnsAsync((LedgerTransfer?)null);

        // Act
        var result = await _transferService.GetTransfer(transferId);

        // Assert
        Assert.Null(result);
        _mockLedgerRepository.Verify(r => r.GetTransfer(transferId), Times.Once);
    }

    [Fact]
    public async Task GetTransfers_WithValidParameters_ReturnsTransfers()
    {
        // Arrange
        const uint limit = 10;
        const ulong cursorMax = 1000;
        const ulong reference = 123;
        var expectedTransfers = new List<LedgerTransfer>
        {
            new(new UInt128(0, 1), new UInt128(0, 1000), new UInt128(0, 2000), new UInt128(0, 100), 0, TransferType.Transfer),
            new(new UInt128(0, 2), new UInt128(0, 3000), new UInt128(0, 4000), new UInt128(0, 200), 0, TransferType.Transfer)
        };

        _mockLedgerRepository
            .Setup(r => r.GetTransfers(limit, cursorMax, reference))
            .ReturnsAsync(expectedTransfers);

        // Act
        var result = await _transferService.GetTransfers(limit, cursorMax, reference);

        // Assert
        Assert.Equal(expectedTransfers, result);
        _mockLedgerRepository.Verify(r => r.GetTransfers(limit, cursorMax, reference), Times.Once);
    }

    [Fact]
    public async Task Transfer_InternalTransfer_CreatesLinkedTransfersWithFee()
    {
        // Arrange
        var payerAccountId = new UInt128(0, 100010001);  // Retail bank account
        var payeeAccountId = new UInt128(0, 100020002);  // Retail bank account
        var amount = new UInt128(0, 10000);  // 100.00
        const ulong reference = 12345;

        var payerAccount = new LedgerAccount(
            Id: payerAccountId,
            AccountType: LedgerAccountType.Transactional,
            DebitOrder: null,
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 50000,
            CreditsPending: 0,
            CreditsPosted: 30000,
            Cursor: 0
        );

        var payeeAccount = new LedgerAccount(
            Id: payeeAccountId,
            AccountType: LedgerAccountType.Transactional,
            DebitOrder: null,
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 0,
            CreditsPending: 0,
            CreditsPosted: 20000,
            Cursor: 0
        );

        var expectedTransferIds = new List<UInt128> { new(0, 1), new(0, 2) };

        _mockLedgerRepository
            .Setup(r => r.GetAccount(payerAccountId))
            .ReturnsAsync(payerAccount);

        _mockLedgerRepository
            .Setup(r => r.GetAccount(payeeAccountId))
            .ReturnsAsync(payeeAccount);

        _mockLedgerRepository
            .Setup(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()))
            .ReturnsAsync(expectedTransferIds);

        // Act
        var result = await _transferService.Transfer(payerAccountId, payeeAccountId, amount, reference);

        // Assert
        Assert.Equal(expectedTransferIds[0], result);

        _mockLedgerRepository.Verify(
            r => r.TransferLinked(It.Is<IEnumerable<LedgerTransfer>>(transfers =>
                transfers.Count() == 2 &&
                transfers.ElementAt(0).DebitAccountId == payerAccountId &&
                transfers.ElementAt(0).CreditAccountId == payeeAccountId &&
                transfers.ElementAt(0).Amount == amount &&
                transfers.ElementAt(0).Reference == reference &&
                transfers.ElementAt(0).TransferType == TransferType.Transfer &&
                transfers.ElementAt(1).DebitAccountId == payerAccountId &&
                transfers.ElementAt(1).CreditAccountId == (ulong)LedgerAccountId.FeeIncome &&
                transfers.ElementAt(1).Amount == new UInt128(0, 250) && // 2.5% of 10000
                transfers.ElementAt(1).TransferType == TransferType.Transfer
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task Transfer_WithNonExistentPayerAccount_ThrowsAccountNotFoundException()
    {
        // Arrange
        var payerAccountId = new UInt128(0, 100010001);
        var payeeAccountId = new UInt128(0, 100020002);
        var amount = new UInt128(0, 10000);
        const ulong reference = 12345;

        _mockLedgerRepository
            .Setup(r => r.GetAccount(payerAccountId))
            .ReturnsAsync((LedgerAccount?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(
            () => _transferService.Transfer(payerAccountId, payeeAccountId, amount, reference)
        );

        Assert.Contains(payerAccountId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Transfer_WithNonTransactionalPayerAccount_ThrowsInvalidAccountException()
    {
        // Arrange
        var payerAccountId = new UInt128(0, 100010001);
        var payeeAccountId = new UInt128(0, 100020002);
        var amount = new UInt128(0, 10000);
        const ulong reference = 12345;

        var payerAccount = new LedgerAccount(
            Id: payerAccountId,
            AccountType: LedgerAccountType.Loan,  // Not transactional
            DebitOrder: null,
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 50000,
            CreditsPending: 0,
            CreditsPosted: 30000,
            Cursor: 0
        );

        _mockLedgerRepository
            .Setup(r => r.GetAccount(payerAccountId))
            .ReturnsAsync(payerAccount);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidAccountException>(
            () => _transferService.Transfer(payerAccountId, payeeAccountId, amount, reference)
        );

        Assert.Contains("loan", exception.Message.ToLower());
        Assert.Contains("transactional", exception.Message.ToLower());
    }

    [Fact]
    public async Task Transfer_WithNonExistentPayeeAccount_ThrowsAccountNotFoundException()
    {
        // Arrange
        var payerAccountId = new UInt128(0, 100010001);
        var payeeAccountId = new UInt128(0, 100020002);
        var amount = new UInt128(0, 10000);
        const ulong reference = 12345;

        var payerAccount = new LedgerAccount(
            Id: payerAccountId,
            AccountType: LedgerAccountType.Transactional,
            DebitOrder: null,
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 50000,
            CreditsPending: 0,
            CreditsPosted: 30000,
            Cursor: 0
        );

        _mockLedgerRepository
            .Setup(r => r.GetAccount(payerAccountId))
            .ReturnsAsync(payerAccount);

        _mockLedgerRepository
            .Setup(r => r.GetAccount(payeeAccountId))
            .ReturnsAsync((LedgerAccount?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(
            () => _transferService.Transfer(payerAccountId, payeeAccountId, amount, reference)
        );

        Assert.Contains(payeeAccountId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Transfer_WithInvalidBankCode_ThrowsInvalidDataException()
    {
        // Arrange
        var payerAccountId = new UInt128(0, 100010001);  // Retail bank account
        var payeeAccountId = new UInt128(0, 500020002);  // Invalid bank account (5000 prefix)
        var amount = new UInt128(0, 10000);
        const ulong reference = 12345;

        var payerAccount = new LedgerAccount(
            Id: payerAccountId,
            AccountType: LedgerAccountType.Transactional,
            DebitOrder: null,
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 50000,
            CreditsPending: 0,
            CreditsPosted: 30000,
            Cursor: 0
        );

        _mockLedgerRepository
            .Setup(r => r.GetAccount(payerAccountId))
            .ReturnsAsync(payerAccount);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(
            () => _transferService.Transfer(payerAccountId, payeeAccountId, amount, reference)
        );
    }

    [Fact]
    public async Task PaySalary_WithValidAccountAndDebitOrder_CreatesTransfers()
    {
        // Arrange
        var accountId = new UInt128(0, 100010001);
        var debitOrderAccountId = new UInt128(0, 100020002);
        const ulong salaryAmount = 50000;

        var debitOrder = new DebitOrder(debitOrderAccountId, salaryAmount);
        var account = new LedgerAccount(
            Id: accountId,
            AccountType: LedgerAccountType.Transactional,
            DebitOrder: debitOrder,
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 0,
            CreditsPending: 0,
            CreditsPosted: 0,
            Cursor: 0
        );

        _mockLedgerRepository
            .Setup(r => r.GetAccount(accountId))
            .ReturnsAsync(account);

        _mockLedgerRepository
            .Setup(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()))
            .ReturnsAsync(new List<UInt128> { new(0, 1), new(0, 2) });

        // Act
        await _transferService.PaySalary(accountId);

        // Assert
        _mockLedgerRepository.Verify(
            r => r.TransferLinked(It.Is<IEnumerable<LedgerTransfer>>(transfers =>
                transfers.Count() == 2 &&
                transfers.ElementAt(0).DebitAccountId == debitOrderAccountId &&
                transfers.ElementAt(0).CreditAccountId == accountId &&
                transfers.ElementAt(0).Amount == salaryAmount &&
                transfers.ElementAt(0).TransferType == TransferType.Transfer &&
                transfers.ElementAt(1).DebitAccountId == accountId &&
                transfers.ElementAt(1).CreditAccountId == (ulong)LedgerAccountId.FeeIncome &&
                transfers.ElementAt(1).Amount == 500 && // 1% of 50000
                transfers.ElementAt(1).TransferType == TransferType.Transfer
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task PaySalary_WithNonExistentAccount_ThrowsAccountNotFoundException()
    {
        // Arrange
        var accountId = new UInt128(0, 100010001);

        _mockLedgerRepository
            .Setup(r => r.GetAccount(accountId))
            .ReturnsAsync((LedgerAccount?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(
            () => _transferService.PaySalary(accountId)
        );

        Assert.Contains(accountId.ToString(), exception.Message);
    }

    [Fact]
    public async Task PaySalary_WithNonTransactionalAccount_ThrowsInvalidAccountException()
    {
        // Arrange
        var accountId = new UInt128(0, 100010001);
        var account = new LedgerAccount(
            Id: accountId,
            AccountType: LedgerAccountType.Loan,
            DebitOrder: null,
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 0,
            CreditsPending: 0,
            CreditsPosted: 0,
            Cursor: 0
        );

        _mockLedgerRepository
            .Setup(r => r.GetAccount(accountId))
            .ReturnsAsync(account);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidAccountException>(
            () => _transferService.PaySalary(accountId)
        );

        Assert.Contains("loan", exception.Message.ToLower());
        Assert.Contains("transactional", exception.Message.ToLower());
    }

    [Fact]
    public async Task PaySalary_WithNoDebitOrder_DoesNothing()
    {
        // Arrange
        var accountId = new UInt128(0, 100010001);
        var account = new LedgerAccount(
            Id: accountId,
            AccountType: LedgerAccountType.Transactional,
            DebitOrder: null,
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 0,
            CreditsPending: 0,
            CreditsPosted: 0,
            Cursor: 0
        );

        _mockLedgerRepository
            .Setup(r => r.GetAccount(accountId))
            .ReturnsAsync(account);

        // Act
        await _transferService.PaySalary(accountId);

        // Assert
        _mockLedgerRepository.Verify(
            r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()),
            Times.Never
        );
    }

    [Fact]
    public async Task PaySalary_WithZeroDebitOrderAmount_DoesNothing()
    {
        // Arrange
        var accountId = new UInt128(0, 100010001);
        var debitOrder = new DebitOrder(new UInt128(0, 100020002), 0);
        var account = new LedgerAccount(
            Id: accountId,
            AccountType: LedgerAccountType.Transactional,
            DebitOrder: debitOrder,
            Closed: false,
            DebitsPending: 0,
            DebitsPosted: 0,
            CreditsPending: 0,
            CreditsPosted: 0,
            Cursor: 0
        );

        _mockLedgerRepository
            .Setup(r => r.GetAccount(accountId))
            .ReturnsAsync(account);

        // Act
        await _transferService.PaySalary(accountId);

        // Assert
        _mockLedgerRepository.Verify(
            r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData(1000, Bank.Retail)]
    [InlineData(2000, Bank.Commercial)]
    public void GetBankCode_WithValidBankPrefix_ReturnsCorrectBank(ushort prefix, Bank expectedBank)
    {
        // Arrange
        var accountNumber = new UInt128(0, (ulong)prefix * 1000000);  // Add some digits after prefix

        // Act
        var result = TransferService.GetBankCode(accountNumber);

        // Assert
        Assert.Equal(expectedBank, result);
    }

    [Fact]
    public void GetBankCode_WithInvalidBankPrefix_ReturnsNull()
    {
        // Arrange
        var accountNumber = new UInt128(0, 5000123456);  // Invalid bank prefix

        // Act
        var result = TransferService.GetBankCode(accountNumber);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetRecentVolume_WithNoTransfers_ReturnsZero()
    {
        // Arrange
        _mockLedgerRepository
            .Setup(r => r.GetTransfers(It.IsAny<uint>(), It.IsAny<ulong>(), It.IsAny<ulong?>()))
            .ReturnsAsync(new List<LedgerTransfer>());

        // Act
        var result = await _transferService.GetRecentVolume();

        // Assert
        Assert.Equal(UInt128.Zero, result);
    }

    [Fact]
    public async Task GetRecentVolume_WithRecentTransfers_ReturnsCorrectVolume()
    {
        // Arrange
        var timeScale = 720ul;
        var recentTimePeriod = 24ul * 3600ul * 1000000000ul / timeScale;
        var currentTime = recentTimePeriod + 1000000ul; // Ensure current time is well above the time period
        
        // Create transfers with proper cursors - transfers should be ordered by cursor descending
        var transfers = new List<LedgerTransfer>
        {
            // Recent transfers (within the time period)
            new(new UInt128(0, 1), new UInt128(0, 1000), new UInt128(0, 2000), new UInt128(0, 100), 0, TransferType.Transfer, null, currentTime),
            new(new UInt128(0, 2), new UInt128(0, 1000), new UInt128(0, 2000), new UInt128(0, 200), 0, TransferType.CompleteTransfer, null, currentTime - 500000),
            // Old transfer (outside the time period) - cursor should be less than minCursor
            new(new UInt128(0, 3), new UInt128(0, 1000), new UInt128(0, 2000), new UInt128(0, 300), 0, TransferType.Transfer, null, currentTime - recentTimePeriod - 100000)
        };

        // Setup mock to return our test transfers first, then empty on subsequent calls
        _mockLedgerRepository
            .SetupSequence(r => r.GetTransfers(It.IsAny<uint>(), It.IsAny<ulong>(), It.IsAny<ulong?>()))
            .ReturnsAsync(transfers)
            .ReturnsAsync(new List<LedgerTransfer>());

        // Act
        var result = await _transferService.GetRecentVolume();

        // Assert
        // Should include the first two transfers (100 + 200 = 300), excluding the old one
        Assert.Equal(new UInt128(0, 300), result);
    }
}