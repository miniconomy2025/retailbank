using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RetailBank.Endpoints;
using RetailBank.Exceptions;
using RetailBank.Extensions;
using RetailBank.Models.Dtos;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;
using RetailBank.Repositories;
using RetailBank.Services;
using RetailBank.Validation;
using TigerBeetle;

namespace RetailBankTest;

public class TransferEndpointsTests
{
    private static TransferService CreateTransferService(
        Mock<ILedgerRepository> ledgerRepository,
        Mock<IInterbankClient>? interbankClient = null
    )
    {
        var transferOptions = Options.Create(new TransferOptions
        {
            TransferFeePercent = 2.5m,
            DepositFeePercent = 1.0m
        });

        var simulationOptions = Options.Create(new SimulationOptions
        {
            TimeScale = 1,
            SimulationStart = 0
        });

        var interbank = interbankClient?.Object ?? new Mock<IInterbankClient>().Object;

        return new TransferService(ledgerRepository.Object, interbank, transferOptions, simulationOptions);
    }

    private static LedgerAccount CreateTransactionalAccount(UInt128 id) => new(
        Id: id,
        AccountType: LedgerAccountType.Transactional
    );

    private static CreateTransferRequest CreateValidRequest() => new(
        From: "100000000001",
        To: "100000000002",
        AmountCents: UInt128.Parse("10000"),
        Reference: 987654321
    );

    [Fact]
    public async Task CreateTransfer_ValidRequest_ReturnsOkWithTransferId()
    {
        // Arrange
        var request = CreateValidRequest();
        var fromAccountId = UInt128.Parse(request.From);
        var toAccountId = UInt128.Parse(request.To);
        var expectedTransferId = new UInt128(0, 42);

        var ledgerRepository = new Mock<ILedgerRepository>();
        ledgerRepository
            .Setup(r => r.GetAccount(fromAccountId))
            .ReturnsAsync(CreateTransactionalAccount(fromAccountId));
        ledgerRepository
            .Setup(r => r.GetAccount(toAccountId))
            .ReturnsAsync(CreateTransactionalAccount(toAccountId));
        ledgerRepository
            .Setup(r => r.TransferLinked(It.Is<IEnumerable<LedgerTransfer>>(transfers =>
                transfers.Count() == 2 &&
                transfers.First().DebitAccountId == fromAccountId &&
                transfers.First().CreditAccountId == toAccountId &&
                transfers.First().Amount == request.AmountCents &&
                transfers.First().Reference == request.Reference &&
                transfers.First().TransferType == TransferType.Transfer
            )))
            .ReturnsAsync(new[] { expectedTransferId, new UInt128(0, 84) });

        var transferService = CreateTransferService(ledgerRepository);
        var logger = new Mock<ILogger<TransferService>>();
        var validator = new CreateTransferRequestValidator();
        var idempotencyCache = new IdempotencyCache(new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = await TransferEndpoints.CreateTransfer(
            request,
            transferService,
            logger.Object,
            validator,
            idempotencyCache
        );

        // Assert
        var okResult = Assert.IsType<Ok<CreateTransferResponse>>(result);
        Assert.Equal(expectedTransferId.ToHex(), okResult.Value?.TransferId);
        ledgerRepository.Verify(
            r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateTransfer_WhenExceedsCredits_ReturnsConflictProblem()
    {
        // Arrange
        var request = CreateValidRequest();
        var fromAccountId = UInt128.Parse(request.From);
        var toAccountId = UInt128.Parse(request.To);

        var ledgerRepository = new Mock<ILedgerRepository>();
        ledgerRepository
            .Setup(r => r.GetAccount(fromAccountId))
            .ReturnsAsync(CreateTransactionalAccount(fromAccountId));
        ledgerRepository
            .Setup(r => r.GetAccount(toAccountId))
            .ReturnsAsync(CreateTransactionalAccount(toAccountId));
        ledgerRepository
            .Setup(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()))
            .ThrowsAsync(new TigerBeetleResultException<CreateTransferResult>(CreateTransferResult.ExceedsCredits));

        var transferService = CreateTransferService(ledgerRepository);
        var logger = new Mock<ILogger<TransferService>>();
        var validator = new CreateTransferRequestValidator();
        var idempotencyCache = new IdempotencyCache(new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = await TransferEndpoints.CreateTransfer(
            request,
            transferService,
            logger.Object,
            validator,
            idempotencyCache
        );

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problemResult.StatusCode);
        Assert.Equal("Insufficient Funds", problemResult.ProblemDetails?.Title);
        Assert.Equal(
            "The account does not have enough funds to complete this transfer.",
            problemResult.ProblemDetails?.Detail
        );
    }

    [Fact]
    public async Task CreateTransfer_WhenUnexpectedLedgerError_ReturnsInternalServerError()
    {
        // Arrange
        var request = CreateValidRequest();
        var fromAccountId = UInt128.Parse(request.From);
        var toAccountId = UInt128.Parse(request.To);

        var ledgerRepository = new Mock<ILedgerRepository>();
        ledgerRepository
            .Setup(r => r.GetAccount(fromAccountId))
            .ReturnsAsync(CreateTransactionalAccount(fromAccountId));
        ledgerRepository
            .Setup(r => r.GetAccount(toAccountId))
            .ReturnsAsync(CreateTransactionalAccount(toAccountId));
        ledgerRepository
            .Setup(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()))
            .ThrowsAsync(new TigerBeetleResultException<CreateTransferResult>(CreateTransferResult.Exists));

        var transferService = CreateTransferService(ledgerRepository);
        var logger = new Mock<ILogger<TransferService>>();
        var validator = new CreateTransferRequestValidator();
        var idempotencyCache = new IdempotencyCache(new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = await TransferEndpoints.CreateTransfer(
            request,
            transferService,
            logger.Object,
            validator,
            idempotencyCache
        );

        // Assert
        var statusResult = Assert.IsType<StatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetTransfers_WithResults_ReturnsPaginatedTransfers()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/transfers";

        const uint limit = 10;
        const ulong cursorMax = 0;
        const ulong reference = 12345;

        var transfers = new List<LedgerTransfer>
        {
            new(
                Id: new UInt128(0, 1),
                DebitAccountId: new UInt128(0, 100000000001),
                CreditAccountId: new UInt128(0, 100000000002),
                Amount: UInt128.Parse("5000"),
                Reference: 111,
                TransferType: TransferType.Transfer,
                ParentId: null,
                Cursor: 100
            ),
            new(
                Id: new UInt128(0, 2),
                DebitAccountId: new UInt128(0, 100000000003),
                CreditAccountId: new UInt128(0, 100000000004),
                Amount: UInt128.Parse("2500"),
                Reference: 222,
                TransferType: TransferType.Transfer,
                ParentId: null,
                Cursor: 90
            )
        };

        var ledgerRepository = new Mock<ILedgerRepository>();
        ledgerRepository
            .Setup(r => r.GetTransfers(limit, cursorMax, reference))
            .ReturnsAsync(transfers);

        var transferService = CreateTransferService(ledgerRepository);

        // Act
        var result = await TransferEndpoints.GetTransfers(
            httpContext,
            transferService,
            limit,
            cursorMax,
            reference
        );

        // Assert
        var okResult = Assert.IsType<Ok<CursorPagination<TransferDto>>>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<TransferDto>>(okResult.Value?.Items).ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal(transfers[0].Id.ToHex(), items[0].TransferId);
        Assert.Equal(transfers[1].Id.ToHex(), items[1].TransferId);
        Assert.Equal("/transfers?limit=10&cursorMax=89", okResult.Value?.Next);
    }

    [Fact]
    public async Task GetTransfers_WhenNoTransfers_ReturnsEmptyCollection()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/transfers";

        var ledgerRepository = new Mock<ILedgerRepository>();
        ledgerRepository
            .Setup(r => r.GetTransfers(It.IsAny<uint>(), It.IsAny<ulong>(), It.IsAny<ulong?>()))
            .ReturnsAsync(Enumerable.Empty<LedgerTransfer>());

        var transferService = CreateTransferService(ledgerRepository);

        // Act
        var result = await TransferEndpoints.GetTransfers(
            httpContext,
            transferService,
            limit: 25,
            cursorMax: 0,
            reference: null
        );

        // Assert
        var okResult = Assert.IsType<Ok<CursorPagination<TransferDto>>>(result);
        Assert.Empty(okResult.Value?.Items ?? Enumerable.Empty<TransferDto>());
        Assert.Null(okResult.Value?.Next);
    }

    [Fact]
    public async Task GetTransfer_WithExistingTransfer_ReturnsOk()
    {
        // Arrange
        var transferId = new UInt128(0, 1234);
        var ledgerTransfer = new LedgerTransfer(
            Id: transferId,
            DebitAccountId: new UInt128(0, 100000000001),
            CreditAccountId: new UInt128(0, 100000000002),
            Amount: UInt128.Parse("7500"),
            Reference: 42,
            TransferType: TransferType.Transfer,
            ParentId: null,
            Cursor: 88
        );

        var ledgerRepository = new Mock<ILedgerRepository>();
        ledgerRepository
            .Setup(r => r.GetTransfer(transferId))
            .ReturnsAsync(ledgerTransfer);

        var transferService = CreateTransferService(ledgerRepository);

        // Act
        var result = await TransferEndpoints.GetTransfer(
            transferId.ToString("X"),
            transferService
        );

        // Assert
        var okResult = Assert.IsType<Ok<TransferDto>>(result);
        Assert.Equal(transferId.ToHex(), okResult.Value?.TransferId);
    }

    [Fact]
    public async Task GetTransfer_WhenTransferMissing_ReturnsNotFound()
    {
        // Arrange
        var transferId = new UInt128(0, 9999);
        var ledgerRepository = new Mock<ILedgerRepository>();
        ledgerRepository
            .Setup(r => r.GetTransfer(transferId))
            .ReturnsAsync((LedgerTransfer?)null);

        var transferService = CreateTransferService(ledgerRepository);

        // Act
        var result = await TransferEndpoints.GetTransfer(
            transferId.ToString("X"),
            transferService
        );

        // Assert
        Assert.IsType<NotFound>(result);
    }
}
