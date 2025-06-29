using Microsoft.AspNetCore.Mvc;
using RetailBank.Models.Dtos;
using RetailBank.Services;
using RetailBank;
using TigerBeetle;
using CliWrap;
using CliWrap.Buffered;

namespace RetailBank.Endpoints;

public static class SimulationEndpoints
{
    public static IEndpointRouteBuilder AddSimulationEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/simulation/start", StartSimulation)
            .Produces(StatusCodes.Status200OK);

        routes
            .MapGet("/simulation/reset", ResetSimulation)
            .Produces(StatusCodes.Status200OK);

        return routes;
    }

    public static IResult StartSimulation(ISimulationControllerService simulationController)
    {
        simulationController.IsRunning = true;
        return Results.Ok();
    }

    public static async Task<IResult> ResetSimulation(ILogger<SimulationRunner> logger, ISimulationControllerService simulationController, ITigerBeetleClientProvider tbClientProvider)
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

        // create the default bank accounts



        // seed the bank with money
    }

}


