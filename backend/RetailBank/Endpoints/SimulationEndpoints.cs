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
        SimulationControllerService simulationController,
        ILedgerRepository ledgerRepository,
        StartSimulationRequest request,
        InterbankClient interbankClient
    )
    {
        await ledgerRepository.InitialiseInternalAccounts();

        simulationController.Start(request.EpochStartTime * 1_000_000_000);

        return Results.NoContent();
    }

    public static async Task<IResult> ResetSimulation(
        ILogger<SimulationRunner> logger,
        SimulationControllerService simulationController,
        TigerBeetleClientProvider tbClientProvider
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


