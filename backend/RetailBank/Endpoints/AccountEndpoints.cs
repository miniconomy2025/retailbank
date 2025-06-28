using RetailBank.Models.Dtos;
using RetailBank.Services;
using TigerBeetle;

namespace RetailBank.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder AddAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapGet("/accounts/{id:long}/balance", GetAccountBalance)
            .Produces<GetAccountBalanceResponse>(StatusCodes.Status200OK);

        routes
            .MapGet("/accounts/{id:long}/transfers", GetAccountTransfers)
            .Produces<GetAccountBalanceResponse>(StatusCodes.Status200OK);

        routes
            .MapPost("/accounts", CreateAccount)
            .Produces<CreateAccountResponse>(StatusCodes.Status200OK);

        routes
            .MapPost("/transfers", CreateTransfer)
            .Produces(StatusCodes.Status201Created);

        return routes;
    }

    public static async Task<IResult> GetAccountBalance(
        ulong id,
        ITransactionService transactionService,
        ILogger<TransactionService> logger
    )
    {
        try
        {
            var account = await transactionService.GetAccount(id);

            if (!account.HasValue)
                return Results.NotFound();

            var balance = new GetAccountBalanceResponse(
                ((Int128)account.Value.CreditsPending - (Int128)account.Value.DebitsPending).ToString(),
                ((Int128)account.Value.CreditsPosted - (Int128)account.Value.DebitsPosted).ToString()
            );
            if (account.HasValue)
                return Results.Ok(balance);
            else
                return Results.NotFound();
        }
        catch (TigerBeetleResultException<CreateAccountResult> ex)
        {
            logger.LogError(ex, ex.Message);
            return Results.StatusCode(500);
        }
    }

    public static async Task<IResult> GetAccountTransfers(
        ulong id,
        ITransactionService transactionService,
        ILogger<TransactionService> logger
    )
    {
        try
        {
            var account = await transactionService.GetAccount(id);

            if (!account.HasValue)
                return Results.NotFound();

            var balance = new GetAccountBalanceResponse(
                ((Int128)account.Value.CreditsPending - (Int128)account.Value.DebitsPending).ToString(),
                ((Int128)account.Value.CreditsPosted - (Int128)account.Value.DebitsPosted).ToString()
            );
            
            return Results.Ok(balance);
        }
        catch (TigerBeetleResultException<CreateAccountResult> ex)
        {
            logger.LogError(ex, ex.Message);
            return Results.StatusCode(500);
        }
    }

    public static async Task<IResult> CreateAccount(
        CreateAccountRequest request, ITransactionService transactionService, ILogger<TransactionService> logger
    )
    {
        try
        {
            var accountId = await transactionService.CreateAccount(request.SalaryCents);
            return Results.Ok(new CreateAccountResponse(accountId));
        }
        catch (TigerBeetleResultException<CreateAccountResult> ex)
        {
            logger.LogError(ex, ex.Message);
            return Results.StatusCode(500);
        }
    }

    public static async Task<IResult> CreateTransfer(
        CreateTransferRequest request,
        ITransactionService transactionService,
        ILogger<TransactionService> logger
    )
    {
        try
        {
            var amount = UInt128.Parse(request.AmountCents);
            await transactionService.Transfer(request.From, request.To, amount);
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
