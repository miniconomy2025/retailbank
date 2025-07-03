using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RetailBank.ExceptionHandlers;

public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        logger.LogError($"An unexpected error has occurred: {exception}");

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Service Error",
            Detail = "An unexpected error has occurred.",
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? throw new InvalidOperationException();

        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);

        return true;
    }
}
