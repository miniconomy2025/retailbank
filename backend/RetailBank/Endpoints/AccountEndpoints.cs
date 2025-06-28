using RetailBank.Models;
using RetailBank.Services;
using TigerBeetle;

namespace RetailBank.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder AddAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/accounts", CreateAccount);

        routes
            .MapPost("/transfers/internal", TransferInternal);

        routes
            .MapPost("/transfers/external", TransferExternal);

        return routes;
    }

    public static async Task<IResult> CreateAccount(
        CreateAccountRequest request, ITransactionService transactionService, ILogger<TransactionService> logger
    )
    {
        try
        {
            return Results.Ok(new CreateAccountResponse(await transactionService.CreateAccount(request.SalaryCents)));
        }
        catch (TigerBeetleResultException<CreateAccountResult> ex)
        {
            logger.LogError(ex, ex.Message);
            return Results.StatusCode(500);
        }
    }

    public static async Task<IResult> TransferInternal(
        InternalTransferRequest request,
        ITransactionService transactionService,
        ILogger<TransactionService> logger
    )
    {
        try
        {
            await transactionService.InternalTransfer(request.From, request.To, request.Amount);
            return Results.Created();
        }

        catch (InvalidAccountException ex)
        {
            return Results.BadRequest(ex.Message);
        }

        catch (AccountNotFoundException ex)
        {
            return Results.BadRequest(ex.Message);
        }

        catch (TigerBeetleResultException<CreateTransferResult> ex)
        {
            logger.LogError(ex, ex.Message);
            return Results.StatusCode(500);
        }
    }

    public static async Task<IResult> TransferExternal(
        ExternalTransferRequest request,
        ITransactionService transactionService,
        ILogger<TransactionService> logger
    )
    {
        try
        {
            await transactionService.ExternalTransfer(request.From, request.To, request.Amount);
            return Results.Created();
        }
        catch (InvalidAccountException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (AccountNotFoundException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (ExternalTransferFailedException ex)
        {
            return Results.Problem(
            detail: $"Downstream service is unavailable. {ex.Message}",
            statusCode: 503,
            title: "Service Unavailable");
        }

        catch (TigerBeetleResultException<CreateTransferResult> ex)
        {
            logger.LogError(ex, ex.Message);
            return Results.StatusCode(500);
        }
    }
}
