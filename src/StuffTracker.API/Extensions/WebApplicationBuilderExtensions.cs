using Microsoft.AspNetCore.OpenApi;

using StuffTracker.API.Exceptions;

namespace StuffTracker.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddPresentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddControllers();
    }
}

