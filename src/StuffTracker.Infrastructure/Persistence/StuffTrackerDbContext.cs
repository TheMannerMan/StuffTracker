using Microsoft.EntityFrameworkCore;
using StuffTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StuffTracker.Infrastructure.Persistence;

internal class StuffTrackerDbContext(DbContextOptions<StuffTrackerDbContext> options) : DbContext(options)
{
    internal DbSet<Location> Locations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StuffTrackerDbContext).Assembly);
        modelBuilder.Entity<Location>().HasQueryFilter(l => !l.IsDeleted);
    }
}
