using Microsoft.Extensions.DependencyInjection;
using Moq;
using RetailBank.Models.Interbank;
using RetailBank.Models.Ledger;
using RetailBank.Repositories;
using RetailBank.Services;

namespace RetailBankTest;

public class MockServices
{
    public static readonly UInt128 InterbankUnknownFailureAccNumber = UInt128.CreateSaturating(200011111111L);
    public static readonly UInt128 InterbankSucceededAccNumber = UInt128.CreateSaturating(200022222222L);
    public static readonly UInt128 InterbankRejectedAccNumber = UInt128.CreateSaturating(200033333333L);
    public static readonly UInt128 InterbankAccountNotFoundAccNumber = UInt128.CreateSaturating(200044444444L);

    public IServiceProvider MockServiceProvider()
    {
        var services = new ServiceCollection();

        var interbankMock = new Mock<IInterbankClient>();
        interbankMock.Setup(interbank => interbank.TryExternalTransfer(Bank.Commercial, It.IsAny<UInt128>(), InterbankUnknownFailureAccNumber, It.IsAny<UInt128>(), It.IsAny<ulong>())).ReturnsAsync(NotificationResult.UnknownFailure);
        interbankMock.Setup(interbank => interbank.TryExternalTransfer(Bank.Commercial, It.IsAny<UInt128>(), InterbankSucceededAccNumber, It.IsAny<UInt128>(), It.IsAny<ulong>())).ReturnsAsync(NotificationResult.Succeeded);
        interbankMock.Setup(interbank => interbank.TryExternalTransfer(Bank.Commercial, It.IsAny<UInt128>(), InterbankRejectedAccNumber, It.IsAny<UInt128>(), It.IsAny<ulong>())).ReturnsAsync(NotificationResult.Rejected);
        interbankMock.Setup(interbank => interbank.TryExternalTransfer(Bank.Commercial, It.IsAny<UInt128>(), InterbankAccountNotFoundAccNumber, It.IsAny<UInt128>(), It.IsAny<ulong>())).ReturnsAsync(NotificationResult.AccountNotFound);

        var ledgerMock = new Mock<ILedgerRepository>();
        // todo: ledger mock

        services.AddTransient(_provider => interbankMock.Object);
        services.AddTransient(_provider => ledgerMock.Object);
        services.AddTransient<AccountService>();
        services.AddTransient<LoanService>();
        services.AddTransient<TransferService>();
        
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task MockServiceProviderWorking()
    {
        var provider = MockServiceProvider();
        var account = provider.GetRequiredService<AccountService>();
        var accs = await account.GetAccounts(null, 25, 0);

        Assert.Empty(accs);
    }
}
