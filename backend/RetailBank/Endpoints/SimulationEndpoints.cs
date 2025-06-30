using Microsoft.AspNetCore.Mvc;
using RetailBank.Models.Dtos;
using RetailBank.Services;
using RetailBank;
using TigerBeetle;
using CliWrap;
using CliWrap.Buffered;
using RetailBank.Models;
using RetailBank.Repositories;

namespace RetailBank.Endpoints;

public static class SimulationEndpoints
{
    private static readonly ulong InitialBankAccountBalance = 1000_000_000ul;
    public static IEndpointRouteBuilder AddSimulationEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/simulation", StartSimulation)
            .Produces(StatusCodes.Status200OK);

        routes
            .MapDelete("/simulation", ResetSimulation)
            .Produces(StatusCodes.Status200OK);

        return routes;
    }

    public static async Task<IResult> StartSimulation(ISimulationControllerService simulationController, ILedgerRepository ledgerRepository)
    {
        if (simulationController.IsRunning)
        {
            return Results.Conflict("The simulation has already begun.");
        }

        simulationController.IsRunning = true;
        // create the default bank accounts
        foreach (var variant in Enum.GetValues<LedgerAccountId>())
        {
            await ledgerRepository.CreateAccount((ulong)variant, LedgerAccountCode.Bank);
        }

        // seed the bank with money
        await ledgerRepository.Transfer(ID.Create(), (ulong)LedgerAccountId.Bank, (ulong)LedgerAccountId.OwnersEquity, InitialBankAccountBalance);
        return Results.Ok();
    }

    public static IResult ResetSimulation(ILogger<SimulationRunner> logger, ISimulationControllerService simulationController, ITigerBeetleClientProvider tbClientProvider)
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


