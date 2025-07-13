using RetailBank.Services;
using CliWrap;
using CliWrap.Buffered;
using RetailBank.Repositories;
using RetailBank.Models.Dtos;
using FluentValidation;

namespace RetailBank.Endpoints;

public static class SimulationEndpoints
{
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
        LedgerRepository ledgerRepository,
        StartSimulationRequest request,
        InterbankClient interbankClient,
        IValidator<StartSimulationRequest> validator
    )
    {
        validator.ValidateAndThrow(request);

        await ledgerRepository.InitialiseInternalAccounts();

        simulationController.Start(request.EpochStartTime * 1000);

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


