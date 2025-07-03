using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RetailBank.Exceptions;

namespace RetailBank.ExceptionHandlers;

public class UserExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not UserException userException)
            return false;

        var problemDetails = new ProblemDetails
        {
            Status = userException.Status,
            Title = userException.Title,
            Detail = userException.Message,
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? throw new InvalidOperationException();
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);
        
        return true;
    }
}
