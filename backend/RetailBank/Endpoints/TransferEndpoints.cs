using System.Globalization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RetailBank.Exceptions;
using RetailBank.Extensions;
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
            .Produces<CreateTransferResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Transfer")
            .WithDescription(
                """
                Transfer money between accounts. Debiting account
                `from` and crediting account `to`. For idempotency,
                the all four fields (`from`, `to`, `amount`, `reference`)
                must together be unique. `reference` should be kept
                the same for repeated requests to ensure multiple
                transactions are not mistakenly created. If you need
                to check if a transfer was created with this request if 
                this is an existing transfer, check the `creationStatus` 
                field in the response. This can be used 
                for transfers between retail bank accounts, and 
                for transfers to the commercial bank. Retail bank 
                account numbers start with `1000`, while commercial 
                bank account numbers start with `2000`.
                """
            );

        routes
            .MapGet("/transfers", GetTransfers)
            .Produces<CursorPagination<TransferDto>>(StatusCodes.Status200OK)
            .WithSummary("Get All Transfers")
            .WithDescription(
                """
                Get all transfers. `next` will contain the URL for
                the next batch of tranfers, or it will be `null` if
                no transfers were returned.
                """
            );

        routes
            .MapGet("/transfers/{id}", GetTransfer)
            .Produces<TransferDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get A Transfer")
            .WithDescription(
                """
                Get information about a transfer.
                """
            );

        return routes;
    }

    public static async Task<IResult> CreateTransfer(
        CreateTransferRequest request,
        TransferService transferService,
        ILogger<TransferService> logger,
        IValidator<CreateTransferRequest> validator,
        IdempotencyCache idempotencyCache
    )
    {
        validator.ValidateAndThrow(request);
        
        var fromAccount = UInt128.Parse(request.From);
        var toAccount = UInt128.Parse(request.To);

        idempotencyCache.InsertAndThrow(request);
        
        try
        {
            var id = await transferService.Transfer(fromAccount, toAccount, request.AmountCents, request.Reference);
            return Results.Ok(new CreateTransferResponse(id.ToHex()));
        }
        catch (TigerBeetleResultException<CreateTransferResult> ex)
        {
            idempotencyCache.Clear(request);
            
            if (ex.ErrorCode == CreateTransferResult.ExceedsCredits)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Insufficient Funds",
                    detail: "The account does not have enough funds to complete this transfer."
                );
            }
            
            logger.LogError("An unexpected exception has occurred while executing transfer, {}: {}", request, ex);
            
            return Results.StatusCode(500);
        }
        catch
        {
            idempotencyCache.Clear(request);
            throw;
        }
    }

    public static async Task<IResult> GetTransfers(
        HttpContext httpContext,
        TransferService transferService,
        SimulationControllerService simulationService,
        [FromQuery] uint limit = 25,
        [FromQuery] ulong cursorMax = 0,
        [FromQuery] ulong? reference = null
    )
    {
        var transfers = await transferService.GetTransfers(limit, cursorMax, reference);

        string? nextUri = null;
        if (transfers.Count() > 0 && httpContext.Request.Path.HasValue)
        {
            var newMax = transfers.Last().Cursor - 1;
            nextUri = $"{httpContext.Request.Path}?limit={limit}&cursorMax={newMax}";
        }

        var pagination = new CursorPagination<TransferDto>(transfers.Select(transfer => new TransferDto(transfer, simulationService)), nextUri);

        return Results.Ok(pagination);
    }

    public static async Task<IResult> GetTransfer(
        string id,
        TransferService transferService,
        SimulationControllerService simulationService
    )
    {
        var transferId = UInt128.Parse(id, NumberStyles.HexNumber);
        var transfer = await transferService.GetTransfer(transferId);

        if (transfer == null)
            return Results.NotFound();
        
        return Results.Ok(new TransferDto(transfer, simulationService));
    }
}
