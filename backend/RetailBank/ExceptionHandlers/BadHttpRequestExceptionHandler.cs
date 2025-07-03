using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RetailBank.ExceptionHandlers;

public class BadHttpRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not BadHttpRequestException badHttpRequestException)
            return false;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = badHttpRequestException.Message,
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? throw new InvalidOperationException();
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);
        
        return true;
    }
}
