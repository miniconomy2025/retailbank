using RetailBank.Services;
using CliWrap;
using CliWrap.Buffered;
using RetailBank.Repositories;
using TigerBeetle;
using RetailBank.Exceptions;
using RetailBank.Models.Ledger;

namespace RetailBank.Endpoints;

public static class SimulationEndpoints
{
    private static readonly ulong InitialBankAccountBalance = 1_000_000_000ul;
    public static IEndpointRouteBuilder AddSimulationEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/simulation", StartSimulation)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Start Simulation");

        routes
            .MapDelete("/simulation", ResetSimulation)
            .Produces(StatusCodes.Status202Accepted)
            .WithSummary("Reset Simulation");

        return routes;
    }

    public static async Task<IResult> StartSimulation(
        ISimulationControllerService simulationController,
        ILedgerRepository ledgerRepository
    )
    {
        if (simulationController.IsRunning)
            return Results.Problem(
                detail: "The simulation has already begun.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request"
            );

        simulationController.IsRunning = true;

        foreach (var variant in Enum.GetValues<BankId>())
        {
            try
            {
                await ledgerRepository.CreateAccount((ulong)variant, LedgerAccountCode.Internal);
            }
            catch (TigerBeetleResultException<CreateAccountResult> ex) when (ex.ErrorCode == CreateAccountResult.Exists) { }
        }

        foreach (var variant in Enum.GetValues<LedgerAccountId>())
        {
            try
            {
                await ledgerRepository.CreateAccount((ulong)variant, LedgerAccountCode.Internal);
            }
            catch (TigerBeetleResultException<CreateAccountResult> ex) when (ex.ErrorCode == CreateAccountResult.Exists) { }
        }

        var mainAccount = await ledgerRepository.GetAccount((ulong)BankId.Retail).ConfigureAwait(false);

        if (mainAccount?.BalancePosted == 0)
        {
            // seed initial funds
            await ledgerRepository.Transfer(new LedgerTransfer(ID.Create(), (ulong)BankId.Retail, (ulong)LedgerAccountId.OwnersEquity, InitialBankAccountBalance));
        }

        return Results.NoContent();
    }

    public static IResult ResetSimulation(
        ILogger<SimulationRunner> logger,
        ISimulationControllerService simulationController,
        ITigerBeetleClientProvider tbClientProvider
    )
    {
        simulationController.IsRunning = false;
        // run the script
        _ = Task.Run(async () =>
        {
            var result = await Cli.Wrap("/bin/bash")
            .WithArguments(["setup-tigerbeetle.sh"])
            .WithWorkingDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            .ExecuteBufferedAsync();

            logger.LogInformation(result.StandardOutput);
            logger.LogError(result.StandardError);

            // now that the tigerbeetle service has hopefully restarted, reset the client
            tbClientProvider.ResetClient();
        });

        return Results.Accepted();
    }
}


