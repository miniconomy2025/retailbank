using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using RetailBank.Models.Dtos;
using RetailBank.Services;
using TigerBeetle;

namespace RetailBank.Endpoints;

public static class TransferEndpoints
{
    public static IEndpointRouteBuilder AddTransferEndpoints(this IEndpointRouteBuilder routes)
    {
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

    public static async Task<IResult> CreateTransfer(
        CreateTransferRequest request,
        ITransferService transactionService,
        ILogger<TransferService> logger
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
        ITransferService transferService,
        [FromQuery] uint limit = 25,
        [FromQuery] ulong timestampMax = 0
    )
    {
        var transfers = (await transferService.GetTransfers(limit, timestampMax)).ToArray();

        string? nextUri = null;
        if (transfers.Length > 0 && httpContext.Request.Path.HasValue)
        {
            var newMax = transfers[transfers.Length - 1].Timestamp - 1;
            nextUri = $"{httpContext.Request.Path}?limit={limit}&timestampMax={newMax}";
        }

        var pagination = new CursorPagination<TransferEvent>(transfers, nextUri);

        return Results.Ok(pagination);
    }

    public static async Task<IResult> GetTransfer(
        string id,
        ITransferService transferService
    )
    {
        var transferId = UInt128.Parse(id, NumberStyles.HexNumber);
        var transfer = await transferService.GetTransfer(transferId);

        if (transfer == null)
            return Results.NotFound();
        
        return Results.Ok(transfer);
    }
}
