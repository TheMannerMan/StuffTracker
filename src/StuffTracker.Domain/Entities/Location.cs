using StuffTracker.Domain.Enums;
using StuffTracker.Domain.Exceptions;

using System;
using System.Collections.Generic;
using System.Text;

namespace StuffTracker.Domain.Entities;

public class Location
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    // Prepared for Step 2 (image upload via Azure Blob Storage)
    public string? ImageUrl { get; set; }

    public LocationType LocationType { get; set; }

    // Points to the parent in the hierarchy. Null if this is a Home node.
    public Guid? ParentId { get; set; }

    // Denormalized FK to the root Home node. Equals own Id for Home nodes, copied from parent for all child locations.
    public Guid HomeId { get; set; }

    // Navigation property upward — EF Core populates this automatically if included in your query
    public Location? Parent { get; set; }

    // Navigation property downward — all nodes that have this node as their parent
    public ICollection<Location> Children { get; set; } = new List<Location>();

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static (Location home, Location unsorted) CreateHome(string name, string? description = null)
    {
        var home = new Location
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            LocationType = LocationType.Home,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        home.HomeId = home.Id;

        var unsorted = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Unsorted",
            LocationType = LocationType.Unsorted,
            ParentId = home.Id,
            HomeId = home.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return (home, unsorted);
    }

    public static Location CreateLocation(string name, string? description, LocationType locationType, Location parent)
    {

        // TODO: From a SRP perspective, do we want to move this validation logic into a separate LocationFactory or LocationService class?

        if (parent.LocationType == LocationType.Unsorted)
        {
            throw new BusinessRuleException("Cannot create a location under the Unsorted location.");
        }

        if (locationType == LocationType.Home || locationType == LocationType.Unsorted)
        {
            throw new BusinessRuleException("Invalid location type for creation. Use CreateHome method to create Home and Unsorted locations.");
        }

        if (parent.LocationType == LocationType.Room && locationType != LocationType.Storage)
        {
            throw new BusinessRuleException("Can only create Storage locations directly under a Room.");
        }

        if (parent.LocationType == LocationType.Storage && locationType != LocationType.Storage)
        {
            throw new BusinessRuleException("You can only create another Storage under a Storage location");
        }

        if (parent.LocationType == LocationType.Home && locationType != LocationType.Room)
        {
            throw new BusinessRuleException("Can only create a Room directly under a Home.");
        }

        var newLocation = new Location
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            LocationType = locationType,
            ParentId = parent.Id,
            HomeId = parent.HomeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return newLocation;
    }
}
