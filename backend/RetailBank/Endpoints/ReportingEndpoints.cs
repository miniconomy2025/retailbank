using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Mvc;
using RetailBank.Models.Dtos;
using RetailBank.Models.Ledger;
using RetailBank.Services;

namespace RetailBank.Endpoints;

public static class ReportingEndpoints
{
    public static IEndpointRouteBuilder AddReportingEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapGet("/report", GetReport);
        return routes;
    }

    public static async Task<IResult> GetReport(IAccountService accountService)
    {
        return Results.Ok(
            new Report(
               (uint)(await accountService.GetAccounts(LedgerAccountType.Transactional, uint.MaxValue, 0)).Count(),
               (uint)(await accountService.GetAccounts(LedgerAccountType.Loan, uint.MaxValue, 0)).Count(),
               (await accountService.GetAccount((ulong)BankId.Retail))?.BalancePosted ?? 0,
               await accountService.GetTotalVolume()
            )
        );
    }
    
}
