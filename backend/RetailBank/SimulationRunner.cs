using Microsoft.Extensions.Options;
using RetailBank.Exceptions;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;
using RetailBank.Services;
using TigerBeetle;

namespace RetailBank;

public class SimulationRunner(
    ILogger<SimulationRunner> logger,
    ILoanService loanService,
    TransferService transferService,
    AccountService accountService,
    SimulationControllerService simulationController
) : BackgroundService
{
    const ulong PayPeriod = 7 * 24 * 3600; // 1 week
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (simulationController.TimeScale == 0)
            throw new InvalidOperationException("Invalid time scale '0'.");

        logger.LogInformation("Starting simulation");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (simulationController.IsRunning)
                {
                    await RunSimulationStepAsync();
                    await Task.Delay(TimeSpan.FromSeconds(PayPeriod / simulationController.TimeScale / 2), stoppingToken);
                    continue;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during simulation step.");
            }
        }

        logger.LogInformation("Stopping simulation");
    }

    private async Task RunSimulationStepAsync()
    {
        logger.LogInformation($"Running simulation step at {DateTime.UtcNow}");

        await PaySalaries();

        TimeSpan.FromSeconds(PayPeriod / simulationController.TimeScale / 2);

        await PayInstallments();
    }

    private const int BatchSize = 4096;

    private async Task PaySalaries()
    {
        var transactionalAccounts = await accountService.GetAccounts(LedgerAccountType.Transactional, BatchSize, 0);

        while (transactionalAccounts.Count() > 0)
        {
            foreach (var account in transactionalAccounts)
            {
                if (account.Closed)
                    continue;

                try
                {
                    await transferService.PaySalary((ulong)account.Id);
                }
                catch (TigerBeetleResultException<CreateTransferResult> exception)
                {
                    logger.LogError($"Failed to pay salary to {account.Id}: {exception.Message}");
                }
            }

            transactionalAccounts = await accountService.GetAccounts(LedgerAccountType.Transactional, BatchSize, transactionalAccounts.Last().Cursor - 1);
        }
    }

    private async Task PayInstallments()
    {
        var loanAccounts = await accountService.GetAccounts(LedgerAccountType.Loan, BatchSize, 0);

        while (loanAccounts.Count() > 0)
        {
            foreach (var account in loanAccounts)
            {
                if (account.Closed || account.BalancePosted == 0)
                    continue;

                logger.LogTrace($"Paying installments for {account.Id}");

                try
                {
                    await loanService.PayInstallment((ulong)account.Id);
                }
                catch (TigerBeetleResultException<CreateTransferResult> exception)
                {
                    logger.LogError($"Paying installments for {account.Id}: {exception.Message}");
                }
            }

            loanAccounts = await accountService.GetAccounts(LedgerAccountType.Loan, BatchSize, loanAccounts.Last().Cursor - 1);
        }
    }
}
