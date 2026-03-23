using StuffTracker.Domain.Enums;
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
}
