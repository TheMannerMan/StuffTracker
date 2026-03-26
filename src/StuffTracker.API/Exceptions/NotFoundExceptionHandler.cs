using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using StuffTracker.Domain.Exceptions;

namespace StuffTracker.API.Exceptions;

public class NotFoundExceptionHandler : IExceptionHandler
{
    private const int statusCode = StatusCodes.Status404NotFound;
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundException)
            return false;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Detail = notFoundException.Message
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
