using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StuffTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StuffTracker.Infrastructure.Configuration;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        builder.Property(l => l.LocationType)
            .HasConversion<string>();  // sparas som text i databasen

        builder.HasOne(l => l.Parent)
            .WithMany(l => l.Children)
            .HasForeignKey(l => l.ParentId)
            .OnDelete(DeleteBehavior.Restrict); // förhindrar cascade delete

        builder.HasIndex(l => l.ParentId);

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(l => l.HomeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.HomeId);
    }
}
