using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using StuffTracker.Application.Common.Behaviors;

namespace StuffTracker.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;

        services.AddAutoMapper(cfg => cfg.AddMaps(applicationAssembly));

        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
    }
}