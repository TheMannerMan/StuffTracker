using StuffTracker.Domain.Enums;

namespace StuffTracker.Application.Locations.Dtos;

public class LocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LocationType LocationType { get; set; }
    public Guid? ParentId { get; set; }
}
