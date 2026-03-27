using Microsoft.AspNetCore.OpenApi;
using System.Text.Json.Serialization;

using StuffTracker.API.Exceptions;

namespace StuffTracker.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddPresentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }
}
