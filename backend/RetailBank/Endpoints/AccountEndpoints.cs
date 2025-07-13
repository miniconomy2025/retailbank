using Microsoft.AspNetCore.Mvc;
using RetailBank.Models.Dtos;
using RetailBank.Models.Ledger;
using RetailBank.Services;

namespace RetailBank.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder AddAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/accounts", CreateTransactionalAccount)
            .Produces<CreateTransactionalAccountResponse>(StatusCodes.Status200OK)
            .WithSummary("Create Transactional Account")
            .WithDescription(
                """
                Create a transactional account. Transactional 
                accounts have an account number consisting of 12 
                digits starting with `1000`.
                """
            );

        routes
            .MapGet("/accounts", GetAccounts)
            .Produces<IEnumerable<AccountDto>>(StatusCodes.Status200OK)
            .WithSummary("Get All Accounts")
            .WithDescription(
                """
                Lookup all accounts by account type in order of newest to oldest.
                `next` will contain the URL for the next batch of
                accounts, or it will be `null` if no accounts were returned.
                
                Notable accounts:
                - Retail Bank Main Account: `1000`
                - Owners Equity Account: `1001`
                - Interest Income Account: `1002`
                - Loan Control Account: `1003`
                - Bad Debts Account: `1004`
                - Commercial Bank: `2000`
                """
            );

        routes
            .MapGet("/accounts/{id:long}", GetAccount)
            .Produces<AccountDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get An Account")
            .WithDescription(
                """
                Get information about any account.
                
                Notable accounts:
                - Retail Bank Main Account: `1000`
                - Owners Equity Account: `1001`
                - Interest Income Account: `1002`
                - Loan Control Account: `1003`
                - Bad Debts Account: `1004`
                - Commercial Bank: `2000`
                """
            );

        routes
            .MapGet("/accounts/{id:long}/transfers", GetAccountTransfers)
            .Produces<CursorPagination<TransferDto>>(StatusCodes.Status200OK)
            .WithSummary("Get An Account's Transfers")
            .WithDescription(
                """
                Get all transfers credited/debited to an account,
                returns empty array if account does not exist.
                `next` will contain the URL for the next batch of
                tranfers, or it will be `null` if no transfers were returned.
                """
            );

        routes
            .MapGet("/accounts/{id:long}/loans", GetAccountLoans)
            .Produces<AccountDto[]>(StatusCodes.Status200OK)
            .WithSummary("Get An Account's Loans")
            .WithDescription(
                """
                Get all loans to this account, returns empty array if
                account does not exist.
                """
            );

        return routes;
    }

    public static async Task<IResult> CreateTransactionalAccount(
        CreateTransactionAccountRequest request,
        AccountService accountService
    )
    {
        var accountId = await accountService.CreateTransactionalAccount(request.SalaryCents);

        return Results.Ok(new CreateTransactionalAccountResponse(accountId.ToString()));
    }

    public static async Task<IResult> GetAccounts(
        HttpContext httpContext,
        AccountService accountService,
        [FromQuery] LedgerAccountType? accountType = null,
        [FromQuery] uint limit = 25,
        [FromQuery] ulong timestampMax = 0
    )
    {
        var accounts = (await accountService.GetAccounts(accountType, limit, timestampMax))
            .Select(account => new AccountDto(account));

        string? nextUri = null;
        if (accounts.Count() > 0 && httpContext.Request.Path.HasValue)
        {
            var newMax = accounts.Last().CreatedAt - 1;
            nextUri = $"{httpContext.Request.Path}?limit={limit}&timestampMax={newMax}";
        }

        var pagination = new CursorPagination<AccountDto>(accounts, nextUri);

        return Results.Ok(pagination);
    }

    public static async Task<IResult> GetAccount(
        ulong id,
        AccountService accountService,
        ILogger<AccountService> logger
    )
    {
        var account = await accountService.GetAccount(id);

        if (account == null)
            return Results.NotFound();

        return Results.Ok(new AccountDto(account));
    }

    public static async Task<IResult> GetAccountTransfers(
        ulong id,
        HttpContext httpContext,
        AccountService accountService,
        [FromQuery] uint limit = 25,
        [FromQuery] ulong timestampMax = 0,
        [FromQuery] TransferSide? side = null
    )
    {
        var transfers = (await accountService.GetAccountTransfers(id, limit, timestampMax, side))
            .Select(transfer => new TransferDto(transfer));
            
        string? nextUri = null;
        if (transfers.Count() > 0 && httpContext.Request.Path.HasValue)
        {
            var newMax = transfers.Last().Timestamp - 1;
            nextUri = $"{httpContext.Request.Path}?limit={limit}&timestampMax={newMax}";
        }

        var pagination = new CursorPagination<TransferDto>(transfers, nextUri);

        return Results.Ok(pagination);
    }

    public static async Task<IResult> GetAccountLoans(
        ulong id,
        HttpContext httpContext,
        AccountService accountService
    )
    {
        var accounts = (await accountService.GetAccountLoans(id))
            .Select(account => new AccountDto(account));

        return Results.Ok(accounts);
    }
}
