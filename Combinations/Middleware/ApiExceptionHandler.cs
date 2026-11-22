using Combinations.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Combinations.Middleware;

/// <summary>Maps domain exceptions to ProblemDetails: bad input -> 400, not started -> 409.</summary>
public class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Invalid argument"),
            NotStartedException => (StatusCodes.Status409Conflict, "Not started"),
            _ => (0, null)
        };

        if (status == 0)
            return false;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails { Status = status, Title = title, Detail = exception.Message },
            cancellationToken);
        return true;
    }
}
