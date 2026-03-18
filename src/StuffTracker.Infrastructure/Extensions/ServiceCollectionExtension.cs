using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StuffTracker.Infrastructure.Persistance;
using System;
using System.Collections.Generic;
using System.Text;

namespace StuffTracker.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StuffTrackerDb");

        services.AddDbContext<StuffTrackerDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
    }
}
