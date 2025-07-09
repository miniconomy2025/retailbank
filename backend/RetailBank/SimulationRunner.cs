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
    ITransferService transferService,
    IAccountService accountService,
    IOptions<SimulationOptions> options,
    ISimulationControllerService simulationController
) : BackgroundService
{
    const ulong PayPeriod = 7 * 24 * 3600; // 1 week
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.Value.TimeScale == 0)
            throw new InvalidOperationException("Invalid time scale '0'.");

        logger.LogInformation("Starting simulation");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (simulationController.IsRunning)
                {
                    await RunSimulationStepAsync();
                    await Task.Delay(TimeSpan.FromSeconds(PayPeriod / options.Value.TimeScale / 2), stoppingToken);
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

        const int BatchSize = 4096;

        // Pay Salaries

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
            
            transactionalAccounts = await accountService.GetAccounts(LedgerAccountType.Transactional, BatchSize, transactionalAccounts.Last().Timestamp - 1);
        }

        TimeSpan.FromSeconds(PayPeriod / options.Value.TimeScale / 2);

        // Charge Interest & Pay Installments

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

            loanAccounts = await accountService.GetAccounts(LedgerAccountType.Loan, BatchSize, loanAccounts.Last().Timestamp - 1);
        }
    }
}
