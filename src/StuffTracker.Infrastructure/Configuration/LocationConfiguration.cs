using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StuffTracker.Domain.Entities;
using StuffTracker.Domain.Constants;

namespace StuffTracker.Infrastructure.Configuration;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(LocationConstants.LocationNameMaxLength);

        builder.Property(l => l.Description)
            .HasMaxLength(LocationConstants.LocationDescriptionMaxLength);

        builder.Property(l => l.LocationType)
            .HasConversion<string>();  //saved as text in the database

        builder.HasOne(l => l.Parent)
            .WithMany(l => l.Children)
            .HasForeignKey(l => l.ParentId)
            .OnDelete(DeleteBehavior.Restrict); // prevents cascade delete 

        builder.HasIndex(l => l.ParentId);

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(l => l.HomeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.HomeId);
        
        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}
