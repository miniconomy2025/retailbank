using RetailBank.Models.Dtos;
using RetailBank.Models.Ledger;
using RetailBank.Services;

namespace RetailBank.Endpoints;

public static class ReportingEndpoints
{
    public static IEndpointRouteBuilder AddReportingEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapGet("/report", GetReport)
            .Produces<Report>(StatusCodes.Status200OK)
            .WithSummary("Generate Report");
        return routes;
    }

    public static async Task<IResult> GetReport(AccountService accountService, TransferService transferService, SimulationControllerService simulationController)
    {
        return Results.Ok(
            new Report(
               (uint)(await accountService.GetAccounts(LedgerAccountType.Transactional, uint.MaxValue, 0)).Count(),
               (uint)(await accountService.GetAccounts(LedgerAccountType.Loan, uint.MaxValue, 0)).Count(),
               (await accountService.GetAccount((ulong)Bank.Retail))?.BalancePosted ?? 0,
               await transferService.GetRecentVolume(),
               DateTimeOffset.FromUnixTimeMilliseconds(
                   (long)simulationController.TimestampToSim(
                       (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                   )
               ).LocalDateTime.ToString()
            )
        );
    }
    
}
