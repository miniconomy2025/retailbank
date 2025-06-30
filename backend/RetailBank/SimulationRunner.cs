using Microsoft.Extensions.Options;
using RetailBank.Models;
using RetailBank.Models.Options;
using RetailBank.Services;

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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.Value.Period == 0)
            throw new InvalidOperationException("Invalid simulation period '0'.");

        logger.LogInformation("SimulationRunner started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (simulationController.IsRunning)
                {
                    await RunSimulationStepAsync();
                    await Task.Delay(TimeSpan.FromSeconds(options.Value.Period), stoppingToken);
                    continue;
                }
            
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during simulation step.");
            }
        }

        logger.LogInformation("SimulationRunner stopping.");
    }

    private async Task RunSimulationStepAsync()
    {
        logger.LogInformation($"Running simulation step at {DateTime.UtcNow}");
        logger.LogInformation("Calculating interest on loan accounts and updating ledger accordingly.");

        var loanAccounts = await accountService.GetAccounts(LedgerAccountCode.Loan);
        var transactionalAccounts = await accountService.GetAccounts(LedgerAccountCode.Transactional);
        
        foreach (var account in loanAccounts)
        {
            logger.LogInformation("Computing interest for account: {account number}", account.Id);
            await loanService.ChargeInterest((ulong)account.Id);
        }

        // wait 15 seconds before continuing
        await Task.Delay(TimeSpan.FromSeconds(options.Value.Period));

        logger.LogInformation("Paying salaries...");
        foreach (var account in transactionalAccounts)
        {
            logger.LogInformation("Paying salary to: {account number}", account.Id);
            await transferService.PaySalary((ulong)account.Id);
        }

        logger.LogInformation("Paying installments");
        foreach (var account in loanAccounts)
        {
            logger.LogInformation("Paying installments for account no: {account number}", account.Id);
            if(account.DebitsPosted - account.CreditsPosted == 0){ continue; }
            await loanService.PayInstallment((ulong)account.Id);
        }
    }
}
