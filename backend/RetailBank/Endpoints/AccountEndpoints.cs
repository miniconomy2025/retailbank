using RetailBank.Models;

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
        CreateAccountRequest request
    )
    {
        return Results.Ok(new CreateAccountResponse(UInt128.MinValue));
    }

    public static async Task<IResult> TransferInternal(
        InternalTransferRequest request
    )
    {
        return Results.Created();
    }

    public static async Task<IResult> TransferExternal(
        ExternalTransferRequest request
    )
    {
        return Results.Created();
    }
}
