using RetailBank.Models.Dtos;
using RetailBank.Services;

namespace RetailBank.Endpoints;

public static class LoanEndpoints
{
    public static IEndpointRouteBuilder AddLoanEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/loans", CreateLoanAccount)
            .Produces<CreateAccountResponse>(StatusCodes.Status200OK)
            .WithSummary("Issue a Loan to a Transactional Account")
            .WithDescription(
                """
                Create a loan account of the requested amount,
                and deposit the principal amount into the debtor's
                account. Each month, interest will be added to the
                to the loan account, and an installment will be
                tranferred from the debtor's account to the loan
                account. The loan will pay itself off after 60 months.
                """
            );

        return routes;
    }

    public static async Task<IResult> CreateLoanAccount(
        CreateLoanAccountRequest request,
        ILoanService loanService
    )
    {
        var accountId = await loanService.CreateLoanAccount(request.DebtorAccountNumber, request.LoanAmountCents);

        return Results.Ok(new CreateAccountResponse(accountId));
    }
}
