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
        CreateAccountRequest request, IAccountService accountService, ILoanService loanService, ILogger<AccountService> logger
    )
    {
        try
        {
            if (request.AccountType == CreateAccountType.Savings)
            {
                if (request?.SalaryCents == null)
                {
                    return Results.BadRequest("Missing required property 'salaryCents'");
                }
                var accountId = await accountService.CreateSavingAccount((ulong)request.SalaryCents);
                return Results.Ok(new CreateAccountResponse(accountId));
            }
            else if (request.AccountType == CreateAccountType.Loan)
            {
                if (request?.LoanAmount == null) {
                    return Results.BadRequest("Missing required property 'loanAmount'");
                }
                if (request?.userAccountNo == null) {
                    return Results.BadRequest("Missing required property 'userAccountNo'");
                }
                var accountId = await loanService.CreateLoanAccount((ulong)request.LoanAmount, (ulong)request.userAccountNo);
                return Results.Ok(new CreateAccountResponse(accountId));
            }
            else
            {
                // this should technically never happen but hey
                return Results.BadRequest();
            }
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
