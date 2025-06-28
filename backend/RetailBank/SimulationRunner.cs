using Microsoft.Extensions.Hosting;
using RetailBank.Models;
using RetailBank.Services;
using System;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TigerBeetle;

public class SimulationRunner(Client tbClient, ILogger<SimulationRunner> logger, ILoanService loanService, ITransactionService transactionService, IAccountService accountService) : BackgroundService
{
    private const ushort SIMULATION_PERIOD = 10;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SimulationRunner started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunSimulationStepAsync();
                await Task.Delay(TimeSpan.FromSeconds(SIMULATION_PERIOD), stoppingToken);
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
        logger.LogInformation("Running simulation step at {Time}", DateTime.UtcNow);
        logger.LogInformation("Calculating interest on loan accounts and updating ledger accordingly.");

        var loanAccounts = await accountService.GetAllAccountsByCodeAsync(AccountCode.Loan);
        Console.WriteLine(JsonSerializer.Serialize(loanAccounts));
        var savingsAccounts = await accountService.GetAllAccountsByCodeAsync(AccountCode.Savings);
        Console.WriteLine(JsonSerializer.Serialize(savingsAccounts));
        foreach (var account in loanAccounts)
        {
            logger.LogInformation("Computing interest for account: {account number}", account.Id);
            await loanService.ComputeInterest(account);
        }

        // wait 15 seconds before continuing
        await Task.Delay(TimeSpan.FromSeconds(SIMULATION_PERIOD));

        logger.LogInformation("Paying salaries...");
        foreach (var account in savingsAccounts)
        {
            logger.LogInformation("Paying salary to: {account number}", account.Id);
            await transactionService.PaySalary(account);
        }

        logger.LogInformation("Paying installments");
        Console.WriteLine(JsonSerializer.Serialize(loanAccounts));
        Console.WriteLine(JsonSerializer.Serialize(savingsAccounts));
        foreach (var account in loanAccounts)
        {
            logger.LogInformation("Paying installments for account no: {account number}", account.Id);
            await loanService.PayInstallment(account);
        }
    }


 

}
