using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using RetailBank.Models.Dtos;
using RetailBank.Services;
using TigerBeetle;

namespace RetailBank.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder AddAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapGet("/accounts/{id:long}", GetAccount)
            .Produces<AccountDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        routes
            .MapGet("/accounts/{id:long}/transfers", GetAccountTransfers)
            .Produces<CursorPagination<TransferEvent>>(StatusCodes.Status200OK);

        routes
            .MapPost("/accounts", CreateTransactionalAccount)
            .Produces<CreateAccountResponse>(StatusCodes.Status200OK);

        routes
            .MapPost("/loans", CreateLoanAccount)
            .Produces<CreateAccountResponse>(StatusCodes.Status200OK);

        routes
            .MapPost("/transfers", CreateTransfer)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        routes
            .MapGet("/transfers", GetTransfers)
            .Produces<CursorPagination<TransferEvent>>(StatusCodes.Status200OK);

        routes
            .MapGet("/transfers/{id}", GetTransfer)
            .Produces<TransferEvent>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routes;
    }

    public static async Task<IResult> GetAccount(
        ulong id,
        IAccountService accountService,
        ILogger<AccountService> logger
    )
    {
        var account = await accountService.GetAccount(id);

        if (!account.HasValue)
            return Results.NotFound();

        var balance = new AccountDto(account.Value);
        
        return Results.Ok(balance);
    }

    public static async Task<IResult> GetAccountTransfers(
        ulong id,
        HttpContext httpContext,
        IAccountService accountService,
        ILogger<AccountService> logger,
        [FromQuery] uint limit = 25,
        [FromQuery] ulong timestampMax = 0,
        [FromQuery] TransferSide side = TransferSide.Any
    )
    {
        var transfers = await accountService.GetAccountTransfers(id, limit, timestampMax, side);
            
        var transferDtos = transfers.Select(transfer => new TransferEvent(transfer)).ToArray();

        string? nextUri = null;
        if (transferDtos.Length > 0 && httpContext.Request.Path.HasValue)
        {
            var newMax = transferDtos[transferDtos.Length - 1].Timestamp;
            nextUri = $"{httpContext.Request.Path}?limit={limit}&timestampMax={newMax}";
        }

        var pagination = new CursorPagination<TransferEvent>(transferDtos, nextUri);

        return Results.Ok(pagination);
    }

    public static async Task<IResult> CreateTransactionalAccount(
        CreateTransactionAccountRequest request,
        IAccountService accountService,
        ILoanService loanService,
        ILogger<AccountService> logger
    )
    {
        var accountId = await accountService.CreateTransactionalAccount((ulong)request.SalaryCents);
        return Results.Ok(new CreateAccountResponse(accountId));
    }

    public static async Task<IResult> CreateLoanAccount(
        CreateLoanAccountRequest request, IAccountService accountService, ILoanService loanService, ILogger<AccountService> logger
    )
    {
        try
        {
            var accountId = await loanService.CreateLoanAccount((ulong)request.LoanAmount, (ulong)request.UserAccountNumber);
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
            await transactionService.Transfer(request.From, request.To, request.AmountCents);
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
            Console.WriteLine("The error code is" + ex.ErrorCode);
            if (ex.ErrorCode == CreateTransferResult.ExceedsCredits)
            {
                return Results.Problem(
                   statusCode: 409,
                   title: "Insufficient Funds",
                   detail: "The account does not have enough funds to complete this transfer.");
            }

            logger.LogError(ex, ex.Message);
            return Results.StatusCode(500);
        }
    }

    public static async Task<IResult> GetTransfers(
        HttpContext httpContext,
        ITransactionService transactionService,
        ILogger<TransactionService> logger,
        [FromQuery] uint limit = 25,
        [FromQuery] ulong timestampMax = 0
    )
    {
        var transfers = await transactionService.GetTransfers(limit, timestampMax);

        var transferDtos = transfers.Select(transfer => new TransferEvent(transfer)).ToArray();

        string? nextUri = null;
        if (transferDtos.Length > 0 && httpContext.Request.Path.HasValue)
        {
            var newMax = transferDtos[transferDtos.Length - 1].Timestamp;
            nextUri = $"{httpContext.Request.Path}?limit={limit}&timestampMax={newMax}";
        }

        var pagination = new CursorPagination<TransferEvent>(transferDtos, nextUri);

        return Results.Ok(pagination);
    }

    public static async Task<IResult> GetTransfer(
        string id,
        ITransactionService transactionService,
        ILogger<TransactionService> logger
    )
    {
        var transferId = UInt128.Parse(id, NumberStyles.HexNumber);
        var transfer = await transactionService.GetTransfer(transferId);

        if (!transfer.HasValue)
            return Results.NotFound();

        var dto = new TransferEvent(transfer.Value);
        
        return Results.Ok(dto);
    }
}
