using RetailBank.Services;
using CliWrap;
using CliWrap.Buffered;
using RetailBank.Repositories;
using TigerBeetle;
using RetailBank.Exceptions;
using RetailBank.Models.Ledger;
using RetailBank.Models.Dtos;

namespace RetailBank.Endpoints;

public static class SimulationEndpoints
{
    private static readonly ulong InitialBankAccountBalance = 1_000_000_000ul;
    public static IEndpointRouteBuilder AddSimulationEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/simulation", StartSimulation)
            .Produces(StatusCodes.Status204NoContent)
            .WithSummary("Start Simulation");

        routes
            .MapDelete("/simulation", ResetSimulation)
            .Produces(StatusCodes.Status204NoContent)
            .WithSummary("Reset Simulation");

        return routes;
    }

    public static async Task<IResult> StartSimulation(
        ISimulationControllerService simulationController,
        ILedgerRepository ledgerRepository,
        StartSimulationRequest request
    )
    {
        simulationController.Start(request.UnixEpochStartTime * 1_000_000_000);

        foreach (var variant in Enum.GetValues<BankId>())
        {
            try
            {
                await ledgerRepository.CreateAccount(new LedgerAccount((ulong)variant, LedgerAccountType.Internal));
            }
            catch (TigerBeetleResultException<CreateAccountResult> ex) when (ex.ErrorCode == CreateAccountResult.Exists) { }
        }

        foreach (var variant in Enum.GetValues<LedgerAccountId>())
        {
            try
            {
                await ledgerRepository.CreateAccount(new LedgerAccount((ulong)variant, LedgerAccountType.Internal));
            }
            catch (TigerBeetleResultException<CreateAccountResult> ex) when (ex.ErrorCode == CreateAccountResult.Exists) { }
        }

        var mainAccount = await ledgerRepository.GetAccount((ulong)BankId.Retail).ConfigureAwait(false);

        if (mainAccount?.BalancePosted == 0)
        {
            await ledgerRepository.Transfer(new LedgerTransfer(ID.Create(), (ulong)BankId.Retail, (ulong)LedgerAccountId.OwnersEquity, InitialBankAccountBalance));
        }

        return Results.NoContent();
    }

    public static async Task<IResult> ResetSimulation(
        ILogger<SimulationRunner> logger,
        ISimulationControllerService simulationController,
        ITigerBeetleClientProvider tbClientProvider
    )
    {
        simulationController.Stop();

        logger.LogInformation("Resetting Simulation");
        
        var result = await Cli.Wrap("/bin/bash")
            .WithArguments(["setup-tigerbeetle.sh"])
            .WithWorkingDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            .ExecuteBufferedAsync();

        logger.LogInformation(result.StandardOutput);
        logger.LogError(result.StandardError);

        tbClientProvider.ResetClient();

        logger.LogInformation("Reset maybe complete...");

        return Results.NoContent();
    }
}


