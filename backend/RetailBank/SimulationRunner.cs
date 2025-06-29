using RetailBank.Models;
using RetailBank.Services;
namespace RetailBank;

public class SimulationRunner(ILogger<SimulationRunner> logger, ILoanService loanService, ITransactionService transactionService, IAccountService accountService, ISimulationControllerService simulationController) : BackgroundService
{
    private const ushort SimulationPeriod = 1;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SimulationRunner started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (simulationController.IsRunning)
                {
                    await RunSimulationStepAsync();
                    await Task.Delay(TimeSpan.FromSeconds(SimulationPeriod), stoppingToken);
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

        var loanAccounts = await accountService.GetAllAccountsByCodeAsync(LedgerAccountCode.Loan);
        var savingsAccounts = await accountService.GetAllAccountsByCodeAsync(LedgerAccountCode.Transactional);
        foreach (var account in loanAccounts)
        {
            logger.LogInformation("Computing interest for account: {account number}", account.Id);
            await loanService.ProcessInterest(account);
        }

        // wait 15 seconds before continuing
        await Task.Delay(TimeSpan.FromSeconds(SimulationPeriod));

        logger.LogInformation("Paying salaries...");
        foreach (var account in savingsAccounts)
        {
            logger.LogInformation("Paying salary to: {account number}", account.Id);
            await transactionService.PaySalary(account);
        }

        logger.LogInformation("Paying installments");
        foreach (var account in loanAccounts)
        {
            logger.LogInformation("Paying installments for account no: {account number}", account.Id);
            if(account.DebitsPosted - account.CreditsPosted == 0){ continue; }
            await loanService.PayInstallment(account);
        }
    }
}
