using RetailBank.Services;
using CliWrap;
using CliWrap.Buffered;
using RetailBank.Models;
using RetailBank.Repositories;

namespace RetailBank.Endpoints;

public static class SimulationEndpoints
{
    private static readonly ulong InitialBankAccountBalance = 1_000_000_000ul;
    public static IEndpointRouteBuilder AddSimulationEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/simulation", StartSimulation)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routes
            .MapDelete("/simulation", ResetSimulation)
            .Produces(StatusCodes.Status200OK);

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
            await ledgerRepository.CreateAccount((ulong)variant, LedgerAccountCode.Bank);

        foreach (var variant in Enum.GetValues<LedgerAccountId>())
            await ledgerRepository.CreateAccount((ulong)variant, LedgerAccountCode.Internal);

        // seed the bank with money
        await ledgerRepository.Transfer(new LedgerTransfer((ulong)BankId.Retail, (ulong)LedgerAccountId.OwnersEquity, InitialBankAccountBalance));
        return Results.Ok();
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


