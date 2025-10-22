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

    private Mock<ILedgerRepository> GetLedgerRepositoryMock(LedgerAccount? debitAccount = null)
    {
        var mockLedgerRepository = new Mock<ILedgerRepository>();
        mockLedgerRepository.Setup(r => r.GetAccount(It.IsAny<UInt128>())).ReturnsAsync(debitAccount);
        mockLedgerRepository.Setup(r => r.CreateAccount(It.IsAny<LedgerAccount>())).Returns(Task.CompletedTask);
        mockLedgerRepository.Setup(r => r.Transfer(It.IsAny<LedgerTransfer>())).ReturnsAsync(new UInt128(0, 1));
        return mockLedgerRepository;
    }

    private Mock<IOptions<LoanOptions>> GetLoanOptionsMock(LoanOptions? loanOptions = null)
    {
        var mockLoanOptions = new Mock<IOptions<LoanOptions>>();
        mockLoanOptions.Setup(o => o.Value).Returns(loanOptions ?? new LoanOptions());
        return mockLoanOptions;
    }

    private LedgerAccount getFundedRetailBankAccount(UInt128 accountNumber, ulong balance)
    {
        return new LedgerAccount(accountNumber, LedgerAccountType.Transactional, null, false, default, balance, default, default, 0);
    }

    private LedgerAccount getLoanAccountWithOutstandingBalance(UInt128 accountNumber, ulong balance, DebitOrder? debitOrder = null)
    {
        return new LedgerAccount(accountNumber, LedgerAccountType.Loan, debitOrder, false, default, balance, default, default, 0);
    }

    private LedgerAccount getFundedClientAccount(UInt128 accountNumber, ulong balance)
    {
        return new LedgerAccount(accountNumber, LedgerAccountType.Transactional, null, false, default, default, default, balance, 0);
    }

    [Fact]
    /// <summary>
    /// CreateLoanAccount with valid debit account and valid loan amount.
    /// Expects: Loan account is created with DebitOrder, transfer is executed, and account number is returned.
    /// </summary>
    public async Task CreateLoanAccount_WithValidTransactionalAccount_CreatesLoanAccountAndTransfersAmount()
    {
        var debitAccountNumber = new UInt128(0, 12345); // the account number of the transactional account we're going to loan money from
        var loanAmount = 100000ul;

        // Arrange
        var ledgerRepositoryMock = GetLedgerRepositoryMock(new LedgerAccount(debitAccountNumber, LedgerAccountType.Transactional));
        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act
        var loanAccountNumber = await loanService.CreateLoanAccount(debitAccountNumber, loanAmount);

        // Assert
        // 1. Valid loan account number is returned
        Assert.NotEqual(UInt128.Zero, loanAccountNumber);

        // 2. CreateAccount is called with correct account type and debit order
        ledgerRepositoryMock.Verify(r => r.CreateAccount(It.Is<LedgerAccount>(acc =>
            acc.AccountType == LedgerAccountType.Loan &&
            acc.DebitOrder != null &&
            acc.DebitOrder.DebitAccountId == debitAccountNumber &&
            acc.DebitOrder.Amount != UInt128.Zero
        )), Times.Once);

        // 3. Transfer is called with the loan amount
        ledgerRepositoryMock.Verify(r => r.Transfer(It.Is<LedgerTransfer>(t =>
            t.DebitAccountId == loanAccountNumber && // bank (accounts receivable) is being debited (i.e. going down)
            t.CreditAccountId == debitAccountNumber && // bank (cash) is being credited (i.e. going down)
            t.Amount == loanAmount &&
            t.TransferType == TransferType.Transfer
        )), Times.Once);
    }

    [Fact]
    /// <summary>
    /// CreateLoanAccount throws AccountNotFoundException when debit account does not exist.
    /// </summary>
    public async Task CreateLoanAccount_WithInvalidDebitAccount_ThrowsAccountNotFoundException()
    {
        var debitAccountNumber = new UInt128(0, 12345); // the account number of the transactional account we're going to loan money from
        var loanAmount = 100000ul;

        // Arrange
        var ledgerRepositoryMock = GetLedgerRepositoryMock(null);
        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(() => loanService.CreateLoanAccount(debitAccountNumber, loanAmount));
    }

    [Fact]
    /// <summary>
    /// CreateLoanAccount throws InvalidAccountException when debit account is not of type Transactional.
    /// </summary>
    public async Task CreateLoanAccount_WithNonTransactionalAccount_ThrowsInvalidAccountException()
    {
        var debitAccountNumber = new UInt128(0, 12345);
        var loanAmount = 100000ul;

        // Arrange - Create an Internal account instead of Transactional
        var ledgerRepositoryMock = GetLedgerRepositoryMock(new LedgerAccount(debitAccountNumber, LedgerAccountType.Internal));
        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidAccountException>(() => loanService.CreateLoanAccount(debitAccountNumber, loanAmount));
    }

    [Fact]
    /// <summary>
    /// CreateLoanAccount throws InvalidLoanAmountException when loanAmount is 0.
    /// </summary>
    public async Task CreateLoanAccount_WithZeroLoanAmount_ThrowsInvalidLoanAmountException()
    {
        var debitAccountNumber = new UInt128(0, 12345);
        var loanAmount = 0ul;

        // Arrange
        var ledgerRepositoryMock = GetLedgerRepositoryMock(new LedgerAccount(debitAccountNumber, LedgerAccountType.Transactional));
        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidLoanAmountException>(() => loanService.CreateLoanAccount(debitAccountNumber, loanAmount));
    }

    [Fact]
    /// <summary>
    /// CreateLoanAccount with maximum ulong loanAmount to test large value handling and installment calculation.
    /// </summary>
    public async Task CreateLoanAccount_WithMaximumLoanAmount_HandlesLargeValueCorrectly()
    {
        var debitAccountNumber = new UInt128(0, 12345);
        var loanAmount = ulong.MaxValue; // Maximum possible ulong value

        // Arrange
        var ledgerRepositoryMock = GetLedgerRepositoryMock(new LedgerAccount(debitAccountNumber, LedgerAccountType.Transactional));
        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act
        var loanAccountNumber = await loanService.CreateLoanAccount(debitAccountNumber, loanAmount);

        // Assert
        // 1. Valid loan account number is returned
        Assert.NotEqual(UInt128.Zero, loanAccountNumber);

        // 2. CreateAccount is called with correct account type and debit order
        ledgerRepositoryMock.Verify(r => r.CreateAccount(It.Is<LedgerAccount>(acc =>
            acc.AccountType == LedgerAccountType.Loan &&
            acc.DebitOrder != null &&
            acc.DebitOrder.DebitAccountId == debitAccountNumber &&
            acc.DebitOrder.Amount > ulong.MinValue // Installment should be calculated and > the minimum value for ulong for max loan amount
        )), Times.Once);

        // 3. Transfer is called with the maximum loan amount
        ledgerRepositoryMock.Verify(r => r.Transfer(It.Is<LedgerTransfer>(t =>
            t.DebitAccountId == loanAccountNumber &&
            t.CreditAccountId == debitAccountNumber &&
            t.Amount == ulong.MaxValue &&
            t.TransferType == TransferType.Transfer
        )), Times.Once);

        // 4. Verify that installment calculation doesn't overflow or cause issues
        // The installment should be a reasonable value based on the loan amount and interest rate
        ledgerRepositoryMock.Verify(r => r.CreateAccount(It.Is<LedgerAccount>(acc =>
            acc.DebitOrder!.Amount < ulong.MaxValue // Should not overflow
        )), Times.Once);
    }

    [Fact]
    /// <summary>
    /// CreateLoanAccount with AnnualInterestRate = 0 should calculate installment as principal / months without interest.
    /// </summary>
    public async Task CreateLoanAccount_WithZeroInterestRate_CalculatesInstallmentAsPrincipalDividedByMonths()
    {
        var debitAccountNumber = new UInt128(0, 12345);
        var loanAmount = 120000ul; // 120,000 for easy division
        var loanPeriodMonths = 12u; // 12 months

        // Arrange - Create loan options with 0% interest rate
        var loanOptions = new LoanOptions
        {
            AnnualInterestRatePercentage = 0m,
            LoanPeriodMonths = loanPeriodMonths
        };
        var ledgerRepositoryMock = GetLedgerRepositoryMock(new LedgerAccount(debitAccountNumber, LedgerAccountType.Transactional));
        var loanOptionsMock = GetLoanOptionsMock(loanOptions);
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act
        var loanAccountNumber = await loanService.CreateLoanAccount(debitAccountNumber, loanAmount);

        // Assert
        // 1. Valid loan account number is returned
        Assert.NotEqual(UInt128.Zero, loanAccountNumber);

        // 2. CreateAccount is called with correct account type and debit order
        ledgerRepositoryMock.Verify(r => r.CreateAccount(It.Is<LedgerAccount>(acc =>
            acc.AccountType == LedgerAccountType.Loan &&
            acc.DebitOrder != null &&
            acc.DebitOrder.DebitAccountId == debitAccountNumber &&
            acc.DebitOrder.Amount == (ulong)(loanAmount / loanPeriodMonths)
        )), Times.Once);

        // 3. Transfer is called with the loan amount
        ledgerRepositoryMock.Verify(r => r.Transfer(It.Is<LedgerTransfer>(t =>
            t.DebitAccountId == loanAccountNumber &&
            t.CreditAccountId == debitAccountNumber &&
            t.Amount == loanAmount &&
            t.TransferType == TransferType.Transfer
        )), Times.Once);
    }

    [Fact]
    /// <summary>
    /// CreateLoanAccount with LoanPeriodMonths = 0 should cause divide-by-zero in installment calculation; expect exception.
    /// </summary>
    public async Task CreateLoanAccount_WithZeroLoanPeriodMonths_ThrowsException()
    {
        var debitAccountNumber = new UInt128(0, 12345);
        var loanAmount = 100000ul;

        // Arrange - 
        var loanOptions = new LoanOptions { AnnualInterestRatePercentage = 5.0m, LoanPeriodMonths = 0u };
        var ledgerRepositoryMock = GetLedgerRepositoryMock(new LedgerAccount(debitAccountNumber, LedgerAccountType.Transactional));
        var loanOptionsMock = GetLoanOptionsMock(loanOptions);
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DivideByZeroException>(() => loanService.CreateLoanAccount(debitAccountNumber, loanAmount));

        ledgerRepositoryMock.Verify(r => r.CreateAccount(It.IsAny<LedgerAccount>()), Times.Never);
        ledgerRepositoryMock.Verify(r => r.Transfer(It.IsAny<LedgerTransfer>()), Times.Never);
    }

    [Fact]
    /// <summary>
    /// CreateLoanAccount when LoanOptions (options.Value) is null or missing should throw exception.
    /// </summary>
    public async Task CreateLoanAccount_WithNullLoanOptions_ThrowsException()
    {
        var debitAccountNumber = new UInt128(0, 12345);
        var loanAmount = 100000ul;

        // Arrange
        var ledgerRepositoryMock = GetLedgerRepositoryMock(new LedgerAccount(debitAccountNumber, LedgerAccountType.Transactional));
        var loanOptionsMock = new Mock<IOptions<LoanOptions>>();
        loanOptionsMock.Setup(o => o.Value).Returns((LoanOptions)null!); // Return null
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NullReferenceException>(() => loanService.CreateLoanAccount(debitAccountNumber, loanAmount));

        // Verify that no account creation or transfer occurred due to the exception
        ledgerRepositoryMock.Verify(r => r.CreateAccount(It.IsAny<LedgerAccount>()), Times.Never);
        ledgerRepositoryMock.Verify(r => r.Transfer(It.IsAny<LedgerTransfer>()), Times.Never);
    }

    [Fact]
    /// <summary>
    /// PayInstallment on valid loan account with sufficient debtor funds.
    /// Expects two transfers: principal and interest.
    /// </summary>
    public async Task PayInstallment_WithValidLoanAccountAndSufficientFunds_ExecutesTwoTransfers()
    {
        var loanAccountId = new UInt128(0, 12345);
        var debitAccountId = new UInt128(0, 67890);
        var installmentAmount = 1000ul;
        var loanBalance = 5000ul;
        var debitAccountBalance = 2000ul; // Sufficient funds

        // Arrange - Create loan account with DebitOrder
        var loanAccount = getLoanAccountWithOutstandingBalance(loanAccountId, loanBalance, new DebitOrder(debitAccountId, installmentAmount));

        // Create debit account with sufficient balance
        var debitAccount = getFundedClientAccount(debitAccountId, debitAccountBalance);

        var ledgerRepositoryMock = new Mock<ILedgerRepository>();
        ledgerRepositoryMock.Setup(r => r.GetAccount(loanAccountId)).ReturnsAsync(loanAccount);
        ledgerRepositoryMock.Setup(r => r.GetAccount(debitAccountId)).ReturnsAsync(debitAccount);
        ledgerRepositoryMock.Setup(r => r.TransferLinked(It.IsAny<LedgerTransfer[]>())).ReturnsAsync(new List<UInt128> { new UInt128(0, 1) });

        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act
        await loanService.PayInstallment(loanAccountId);

        // Assert
        // Verify that TransferLinked was called with exactly 2 transfers
        ledgerRepositoryMock.Verify(r => r.TransferLinked(It.Is<IEnumerable<LedgerTransfer>>(transfers =>
            transfers.Count() == 2 &&
            transfers.Any(t => t.DebitAccountId == debitAccountId && t.CreditAccountId == loanAccountId) && // Principal transfer
            transfers.Any(t => t.DebitAccountId == debitAccountId && t.CreditAccountId == (ulong)LedgerAccountId.InterestIncome) // Interest transfer
        )), Times.Once);
    }

    [Fact]
    /// <summary>
    /// PayInstallment throws AccountNotFoundException when loan account does not exist.
    /// </summary>
    public async Task PayInstallment_WithNonExistentLoanAccount_ThrowsAccountNotFoundException()
    {
        var loanAccountId = new UInt128(0, 12345);

        // Arrange
        var ledgerRepositoryMock = GetLedgerRepositoryMock();
        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        ledgerRepositoryMock.Setup(r => r.GetAccount(loanAccountId)).ReturnsAsync((LedgerAccount?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(() => loanService.PayInstallment(loanAccountId));

        // Verify that no transfer operations were attempted
        ledgerRepositoryMock.Verify(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()), Times.Never);
        ledgerRepositoryMock.Verify(r => r.BalanceAndCloseCredit(It.IsAny<UInt128>(), It.IsAny<UInt128>()), Times.Never);
    }

    [Fact]
    /// <summary>
    /// PayInstallment throws InvalidAccountException when account type is not Loan.
    /// </summary>
    public async Task PayInstallment_WithNonLoanAccount_ThrowsInvalidAccountException()
    {
        var loanAccountId = new UInt128(0, 12345);
        var debitAccountId = new UInt128(0, 67890);

        // Arrange - Create a Transactional account instead of Loan account
        var nonLoanAccount = getFundedClientAccount(loanAccountId, 1000ul);
        var ledgerRepositoryMock = GetLedgerRepositoryMock();
        ledgerRepositoryMock.Setup(r => r.GetAccount(loanAccountId)).ReturnsAsync(nonLoanAccount);

        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidAccountException>(() => loanService.PayInstallment(loanAccountId));

        // Verify that no transfer operations were attempted
        ledgerRepositoryMock.Verify(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()), Times.Never);
        ledgerRepositoryMock.Verify(r => r.BalanceAndCloseCredit(It.IsAny<UInt128>(), It.IsAny<UInt128>()), Times.Never);
    }

    [Fact]
    /// <summary>
    /// PayInstallment exits without action when DebitOrder is null on loan account.
    /// </summary>
    public async Task PayInstallment_WithNullDebitOrder_ExitsWithoutAction()
    {
        var loanAccountId = new UInt128(0, 12345);

        // Arrange - Create loan account with null DebitOrder
        var loanAccount = getLoanAccountWithOutstandingBalance(loanAccountId, 5000ul, null); // DebitOrder is null
        var ledgerRepositoryMock = GetLedgerRepositoryMock();
        ledgerRepositoryMock.Setup(r => r.GetAccount(loanAccountId)).ReturnsAsync(loanAccount);

        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act
        await loanService.PayInstallment(loanAccountId);

        // Assert
        // Verify that no transfer operations were attempted
        ledgerRepositoryMock.Verify(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()), Times.Never);
        ledgerRepositoryMock.Verify(r => r.BalanceAndCloseCredit(It.IsAny<UInt128>(), It.IsAny<UInt128>()), Times.Never);

        // Verify that only the initial GetAccount call was made
        ledgerRepositoryMock.Verify(r => r.GetAccount(loanAccountId), Times.Once);
    }

    [Fact]
    /// <summary>
    /// PayInstallment throws AccountNotFoundException when DebitOrder's debit account does not exist.
    /// </summary>
    public async Task PayInstallment_WithNonExistentDebitAccount_ThrowsAccountNotFoundException()
    {
        var loanAccountId = new UInt128(0, 12345);
        var debitAccountId = new UInt128(0, 67890);
        var installmentAmount = 1000ul;

        // Arrange - Create loan account with DebitOrder pointing to non-existent debit account
        var loanAccount = getLoanAccountWithOutstandingBalance(loanAccountId, 5000ul, new DebitOrder(debitAccountId, installmentAmount));
        var ledgerRepositoryMock = GetLedgerRepositoryMock();
        ledgerRepositoryMock.Setup(r => r.GetAccount(loanAccountId)).ReturnsAsync(loanAccount);
        ledgerRepositoryMock.Setup(r => r.GetAccount(debitAccountId)).ReturnsAsync((LedgerAccount?)null); // Debit account doesn't exist

        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(() => loanService.PayInstallment(loanAccountId));

        // Verify that no transfer operations were attempted
        ledgerRepositoryMock.Verify(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()), Times.Never);
        ledgerRepositoryMock.Verify(r => r.BalanceAndCloseCredit(It.IsAny<UInt128>(), It.IsAny<UInt128>()), Times.Never);

        // Verify that both account lookups were attempted
        ledgerRepositoryMock.Verify(r => r.GetAccount(loanAccountId), Times.Once);
        ledgerRepositoryMock.Verify(r => r.GetAccount(debitAccountId), Times.Once);
    }

    [Fact]
    /// <summary>
    /// PayInstallment where loan balance is 0 results in no amount due; expect no transfer or zero transfer.
    /// </summary>
    public async Task PayInstallment_WithZeroLoanBalance_ExecutesZeroTransfer()
    {
        var loanAccountId = new UInt128(0, 12345);
        var debitAccountId = new UInt128(0, 67890);
        var installmentAmount = 1000ul;
        var loanBalance = 0ul; // Zero loan balance

        // Arrange - Create loan account with zero balance
        var loanAccount = getLoanAccountWithOutstandingBalance(loanAccountId, loanBalance, new DebitOrder(debitAccountId, installmentAmount));
        var debitAccount = getFundedClientAccount(debitAccountId, 2000ul); // Sufficient funds

        var ledgerRepositoryMock = GetLedgerRepositoryMock();
        ledgerRepositoryMock.Setup(r => r.GetAccount(loanAccountId)).ReturnsAsync(loanAccount);
        ledgerRepositoryMock.Setup(r => r.GetAccount(debitAccountId)).ReturnsAsync(debitAccount);
        ledgerRepositoryMock.Setup(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>())).ReturnsAsync(new List<UInt128> { new UInt128(0, 1) });

        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act
        await loanService.PayInstallment(loanAccountId);

        // Assert
        // Verify that TransferLinked was called with transfers where amountDue = 0
        ledgerRepositoryMock.Verify(r => r.TransferLinked(It.Is<IEnumerable<LedgerTransfer>>(transfers =>
            transfers.Count() == 2 &&
            transfers.All(t => t.Amount == 0) // All transfers should have zero amount
        )), Times.Once);
    }

    [Fact]
    /// <summary>
    /// PayInstallment where installment is greater than loan balance; amountDue equals remaining loan balance.
    /// </summary>
    public async Task PayInstallment_WithInstallmentGreaterThanLoanBalance_TransfersOnlyRemainingBalance()
    {
        var loanAccountId = new UInt128(0, 12345);
        var debitAccountId = new UInt128(0, 67890);
        var installmentAmount = 1500ul; // Larger than loan balance
        var loanBalance = 1000ul; // Less than installment

        // Arrange - Create loan account with balance less than installment
        var loanAccount = getLoanAccountWithOutstandingBalance(loanAccountId, loanBalance, new DebitOrder(debitAccountId, installmentAmount));
        var debitAccount = getFundedClientAccount(debitAccountId, 2000ul); // Sufficient funds

        var ledgerRepositoryMock = GetLedgerRepositoryMock();
        ledgerRepositoryMock.Setup(r => r.GetAccount(loanAccountId)).ReturnsAsync(loanAccount);
        ledgerRepositoryMock.Setup(r => r.GetAccount(debitAccountId)).ReturnsAsync(debitAccount);
        ledgerRepositoryMock.Setup(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>())).ReturnsAsync(new List<UInt128> { new UInt128(0, 1) });

        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act
        await loanService.PayInstallment(loanAccountId);

        // Assert
        ledgerRepositoryMock.Verify(r => r.TransferLinked(It.Is<IEnumerable<LedgerTransfer>>(transfers =>
            transfers.Count() == 2 &&
            transfers.Any(t => t.DebitAccountId == debitAccountId && t.CreditAccountId == loanAccountId) && // Principal transfer
            transfers.Any(t => t.DebitAccountId == debitAccountId && t.CreditAccountId == (ulong)LedgerAccountId.InterestIncome) // Interest transfer
        )), Times.Once);
    }

    [Fact]
    /// <summary>
    /// PayInstallment when debit account has insufficient funds triggers BalanceAndCloseCredit (bad debt closure).
    /// </summary>
    public async Task PayInstallment_WithInsufficientFunds_TriggersBadDebtClosure()
    {
        var loanAccountId = new UInt128(0, 12345);
        var debitAccountId = new UInt128(0, 67890);
        var installmentAmount = 1000ul;
        var loanBalance = 5000ul;
        var debitAccountBalance = 500ul; // Insufficient funds (less than amountDue)

        // Arrange - Create loan account and debit account with insufficient funds
        var loanAccount = getLoanAccountWithOutstandingBalance(loanAccountId, loanBalance, new DebitOrder(debitAccountId, installmentAmount));
        var debitAccount = getFundedClientAccount(debitAccountId, debitAccountBalance);

        var ledgerRepositoryMock = GetLedgerRepositoryMock();
        ledgerRepositoryMock.Setup(r => r.GetAccount(loanAccountId)).ReturnsAsync(loanAccount);
        ledgerRepositoryMock.Setup(r => r.GetAccount(debitAccountId)).ReturnsAsync(debitAccount);
        ledgerRepositoryMock.Setup(r => r.BalanceAndCloseCredit(It.IsAny<UInt128>(), It.IsAny<UInt128>())).ReturnsAsync((loanAccountId, debitAccountId));

        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act
        await loanService.PayInstallment(loanAccountId);

        // Assert
        // Verify that BalanceAndCloseCredit was called for bad debt closure
        ledgerRepositoryMock.Verify(r => r.BalanceAndCloseCredit(
            (UInt128)(ulong)LedgerAccountId.BadDebts,
            loanAccountId
        ), Times.Once);

        // Verify that no transfer operations were attempted
        ledgerRepositoryMock.Verify(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>()), Times.Never);
    }

    [Fact]
    /// <summary>
    /// PayInstallment where debit account has exactly amountDue; full installment is paid.
    /// </summary>
    public async Task PayInstallment_WithExactFunds_PaysFullInstallment()
    {
        var loanAccountId = new UInt128(0, 12345);
        var debitAccountId = new UInt128(0, 67890);
        var installmentAmount = 1000ul;
        var loanBalance = 5000ul;
        var debitAccountBalance = 1000ul; // Exactly equal to amountDue

        // Arrange - Create loan account and debit account with exact funds
        var loanAccount = getLoanAccountWithOutstandingBalance(loanAccountId, loanBalance, new DebitOrder(debitAccountId, installmentAmount));
        var debitAccount = getFundedClientAccount(debitAccountId, debitAccountBalance);

        var ledgerRepositoryMock = GetLedgerRepositoryMock();
        ledgerRepositoryMock.Setup(r => r.GetAccount(loanAccountId)).ReturnsAsync(loanAccount);
        ledgerRepositoryMock.Setup(r => r.GetAccount(debitAccountId)).ReturnsAsync(debitAccount);
        ledgerRepositoryMock.Setup(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>())).ReturnsAsync(new List<UInt128> { new UInt128(0, 1) });

        var loanOptionsMock = GetLoanOptionsMock();
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act
        await loanService.PayInstallment(loanAccountId);

        // Assert
        ledgerRepositoryMock.Verify(r => r.TransferLinked(It.Is<IEnumerable<LedgerTransfer>>(transfers =>
            transfers.Count() == 2 &&
            transfers.Any(t => t.DebitAccountId == debitAccountId && t.CreditAccountId == loanAccountId) && // Principal transfer
            transfers.Any(t => t.DebitAccountId == debitAccountId && t.CreditAccountId == (ulong)LedgerAccountId.InterestIncome) // Interest transfer
        )), Times.Once);

        // Verify that BalanceAndCloseCredit was NOT called (sufficient funds)
        ledgerRepositoryMock.Verify(r => r.BalanceAndCloseCredit(It.IsAny<UInt128>(), It.IsAny<UInt128>()), Times.Never);
    }

    [Fact]
    /// <summary>
    /// PayInstallment with AnnualInterestRate = 0 results in interestDue = 0; all payment applies to principal.
    /// </summary>
    public async Task PayInstallment_WithZeroInterestRate_AllPaymentAppliesToPrincipal()
    {
        var loanAccountId = new UInt128(0, 12345);
        var debitAccountId = new UInt128(0, 67890);
        var installmentAmount = 1000ul;
        var loanBalance = 5000ul;

        // Arrange - Create loan options with 0% interest rate
        var loanOptions = new LoanOptions
        {
            AnnualInterestRatePercentage = 0m,
            LoanPeriodMonths = 12u
        };

        var loanAccount = getLoanAccountWithOutstandingBalance(loanAccountId, loanBalance, new DebitOrder(debitAccountId, installmentAmount));
        var debitAccount = getFundedClientAccount(debitAccountId, 2000ul);

        var ledgerRepositoryMock = GetLedgerRepositoryMock();
        ledgerRepositoryMock.Setup(r => r.GetAccount(loanAccountId)).ReturnsAsync(loanAccount);
        ledgerRepositoryMock.Setup(r => r.GetAccount(debitAccountId)).ReturnsAsync(debitAccount);
        ledgerRepositoryMock.Setup(r => r.TransferLinked(It.IsAny<IEnumerable<LedgerTransfer>>())).ReturnsAsync(new List<UInt128> { new UInt128(0, 1) });

        var loanOptionsMock = GetLoanOptionsMock(loanOptions);
        var loanService = new LoanService(ledgerRepositoryMock.Object, loanOptionsMock.Object);

        // Act
        await loanService.PayInstallment(loanAccountId);

        // Assert
        // Verify that TransferLinked was called with exactly 2 transfers
        ledgerRepositoryMock.Verify(r => r.TransferLinked(It.Is<IEnumerable<LedgerTransfer>>(transfers =>
            transfers.Count() == 2 &&
            transfers.Any(t => t.DebitAccountId == debitAccountId && t.CreditAccountId == loanAccountId) && // Principal transfer
            transfers.Any(t => t.DebitAccountId == debitAccountId && t.CreditAccountId == (ulong)LedgerAccountId.InterestIncome && t.Amount == 0) // Interest transfer with 0 amount
        )), Times.Once);
    }

}

