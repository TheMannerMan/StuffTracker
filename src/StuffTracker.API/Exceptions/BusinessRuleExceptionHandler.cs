using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using StuffTracker.Domain.Exceptions;

namespace StuffTracker.API.Exceptions;

public class BusinessRuleExceptionHandler : IExceptionHandler
{

    private const int statusCode = StatusCodes.Status422UnprocessableEntity;
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException businessRuleException)
            return false;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Detail = businessRuleException.Message
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
