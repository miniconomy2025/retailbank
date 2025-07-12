using FluentValidation;
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
        LoanService loanService,
        IValidator<CreateLoanAccountRequest> validator
    )
    {
        validator.ValidateAndThrow(request);

        var debtorAccountId = UInt128.Parse(request.DebtorAccountId);
        var accountId = await loanService.CreateLoanAccount(debtorAccountId, request.LoanAmountCents);

        return Results.Ok(new CreateAccountResponse(accountId.ToString()));
    }
}
