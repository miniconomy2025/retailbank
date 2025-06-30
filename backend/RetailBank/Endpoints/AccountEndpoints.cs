using Microsoft.AspNetCore.Mvc;
using RetailBank.Models.Dtos;
using RetailBank.Services;

namespace RetailBank.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder AddAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/accounts", CreateTransactionalAccount)
            .Produces<CreateAccountResponse>(StatusCodes.Status200OK);

        routes
            .MapGet("/accounts/{id:long}", GetAccount)
            .Produces<LedgerAccount>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        routes
            .MapGet("/accounts/{id:long}/transfers", GetAccountTransfers)
            .Produces<CursorPagination<TransferEvent>>(StatusCodes.Status200OK);

        return routes;
    }

    public static async Task<IResult> CreateTransactionalAccount(
        CreateTransactionAccountRequest request,
        IAccountService accountService
    )
    {
        var accountId = await accountService.CreateTransactionalAccount(request.SalaryCents);

        return Results.Ok(new CreateAccountResponse(accountId));
    }

    public static async Task<IResult> GetAccount(
        ulong id,
        IAccountService accountService,
        ILogger<AccountService> logger
    )
    {
        var account = await accountService.GetAccount(id);

        if (account == null)
            return Results.NotFound();

        return Results.Ok(account);
    }

    public static async Task<IResult> GetAccountTransfers(
        ulong id,
        HttpContext httpContext,
        IAccountService accountService,
        [FromQuery] uint limit = 25,
        [FromQuery] ulong timestampMax = 0,
        [FromQuery] TransferSide side = TransferSide.Any
    )
    {
        var transfers = (await accountService.GetAccountTransfers(id, limit, timestampMax, side)).ToArray();
            
        string? nextUri = null;
        if (transfers.Length > 0 && httpContext.Request.Path.HasValue)
        {
            var newMax = transfers[transfers.Length - 1].Timestamp - 1;
            nextUri = $"{httpContext.Request.Path}?limit={limit}&timestampMax={newMax}";
        }

        var pagination = new CursorPagination<TransferEvent>(transfers, nextUri);

        return Results.Ok(pagination);
    }
}
