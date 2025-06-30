using RetailBank.Models.Dtos;
using RetailBank.Services;

namespace RetailBank.Endpoints;

public static class LoanEndpoints
{
    public static IEndpointRouteBuilder AddLoanEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/loans", CreateLoanAccount)
            .Produces<CreateAccountResponse>(StatusCodes.Status200OK);

        return routes;
    }

    public static async Task<IResult> CreateLoanAccount(
        CreateLoanAccountRequest request,
        ILoanService loanService
    )
    {
        var accountId = await loanService.CreateLoanAccount(request.UserAccountNumber, request.LoanAmount);

        return Results.Ok(new CreateAccountResponse(accountId));
    }
}
