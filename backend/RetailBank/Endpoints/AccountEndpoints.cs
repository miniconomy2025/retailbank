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
            .MapGet("/accounts/{id:long}/balance", GetAccountBalance)
            .Produces<GetAccountBalanceResponse>(StatusCodes.Status200OK);

        routes
            .MapGet("/accounts/{id:long}/transfers", GetAccountTransfers)
            .Produces<TransferHistory>(StatusCodes.Status200OK);

        routes
            .MapPost("/accounts", CreateTransactionalAccount)
            .Produces<CreateAccountResponse>(StatusCodes.Status200OK);

        routes
            .MapPost("/loans", CreateLoanAccount)
            .Produces<CreateAccountResponse>(StatusCodes.Status200OK);

        routes
            .MapPost("/transfers", CreateTransfer)
            .Produces(StatusCodes.Status201Created);

        // routes
        //     .MapGet("/transfers", GetTransfers)
        //     .Produces<>(StatusCodes.Status201Created);

        return routes;
    }

    public static async Task<IResult> GetAccountBalance(
        ulong id,
        IAccountService accountService,
        ILogger<AccountService> logger
    )
    {
        try
        {
            var account = await accountService.GetAccount(id);

            if (!account.HasValue)
                return Results.NotFound();

            var balance = new GetAccountBalanceResponse(
                (Int128)account.Value.CreditsPending - (Int128)account.Value.DebitsPending,
                (Int128)account.Value.CreditsPosted - (Int128)account.Value.DebitsPosted
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
        IAccountService accountService,
        ILogger<AccountService> logger,
        [FromQuery] uint limit = 100,
        [FromQuery] ulong timeStampMax = 0
    )
    {
        try
        {
            var transfers = await accountService.GetAccountTransfers(id, limit, timeStampMax);
            
            var transferDtos = transfers.Select(transfer =>
            {
                var status = TransferEventType.Transfer;
                if ((transfer.Flags & TransferFlags.Pending) > 0)
                {
                    status = TransferEventType.StartTransfer;
                }
                else if ((transfer.Flags & TransferFlags.PostPendingTransfer) > 0)
                {
                    status = TransferEventType.CompleteTransfer;
                }
                else if ((transfer.Flags & TransferFlags.VoidPendingTransfer) > 0)
                {
                    status = TransferEventType.CancelTransfer;
                }

                return new TransferEvent(
                    transfer.Id.ToString("X"),
                    (ulong)transfer.DebitAccountId,
                    (ulong)transfer.CreditAccountId,
                    transfer.Amount,
                    transfer.PendingId > 0 ? transfer.PendingId.ToString("X") : null,
                    transfer.Timestamp,
                    status
                );
            });

            var balance = new TransferHistory(transferDtos);

            return Results.Ok(balance);
        }
        catch (TigerBeetleResultException<CreateAccountResult> ex)
        {
            logger.LogError(ex, ex.Message);
            return Results.StatusCode(500);
        }
    }

    public static async Task<IResult> CreateTransactionalAccount(
        CreateTransactionAccountRequest request, IAccountService accountService, ILoanService loanService, ILogger<AccountService> logger
    )
    {
        try
        {
            if (request?.SalaryCents == null)
            {
                return Results.BadRequest("Missing required property 'salaryCents'");
            }
            var accountId = await accountService.CreateSavingAccount((ulong)request.SalaryCents);
            return Results.Ok(new CreateAccountResponse(accountId));
        }
        catch (TigerBeetleResultException<CreateAccountResult> ex)
        {
            logger.LogError(ex, ex.Message);
            return Results.StatusCode(500);
        }
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
}
